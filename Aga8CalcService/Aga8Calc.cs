using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Timers;

namespace Aga8CalcService
{
    public sealed class Aga8Calc : IDisposable
    {
        private readonly Timer _timer;
        private readonly Aga8OpcClient _client;
        private readonly ConfigModel conf;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly object WorkerLock = new();
        private bool working = false;

        public Aga8Calc()
        {
            Assembly assem = Assembly.GetExecutingAssembly();
            string version = assem.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            string title = assem.GetName().Name;

            logger.Info("Initializing \"{0}\" version \"{1}\".", title, version);

            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "Aga8Calc.config";
            try
            {
                logger.Info("Reading configuration from file \"{0}\".", TagConfFile);
                conf = ConfigModel.ReadConfig(TagConfFile);

            }
            catch (Exception e)
            {
                logger.Fatal(e, "Failed to read configuration file.");
                throw;
            }
            _timer = new Timer(conf.Interval) { AutoReset = true, SynchronizingObject = null };
            _timer.Elapsed += Worker;

            _client = new Aga8OpcClient(conf.OpcUrl, conf.OpcUser, conf.OpcPassword);
        }

        public void Dispose()
        {
            _client.Dispose();
            _timer.Dispose();
        }

        private void Worker(object sender, ElapsedEventArgs ea)
        {
            logger.Debug(CultureInfo.InvariantCulture, "Worker triggered.");

            if (working)
            {
                logger.Warn(CultureInfo.InvariantCulture, "Worker not completed within Interval. Interval might be too short.");
                lock (WorkerLock)
                {
                    working = false;
                }
                return;
            }

            Stopwatch watch = Stopwatch.StartNew();

            lock (WorkerLock)
            {
                working = true;
                ReadFromOPC();
                Calculate();
                WriteToOPC();
                working = false;
            }

            watch.Stop();

            logger.Debug(CultureInfo.InvariantCulture, "Worker elapsed time: {0} ms.", watch.ElapsedMilliseconds);
        }

        private void ReadFromOPC()
        {
            NodeIdCollection nodes = new();
            List<Type> types = new();
            List<object> result;
            List<ServiceResult> errors;

            // Make a list of all the OPC NodeIds that we want to read
            foreach (var c in conf.ConfigList.Item)
            {
                foreach (Component comp in c.Composition.Item)
                {
                    if (string.IsNullOrEmpty(comp.NodeId)) { continue; }

                    nodes.Add(comp.NodeId); types.Add(typeof(object));
                }

                foreach (PressureTemperature pt in c.PressureTemperatureList.Item)
                {
                    foreach (PressureMeasurement pm in pt.PressureFunction.Item)
                    {
                        nodes.Add(pm.NodeId); types.Add(typeof(object));
                    }
                    foreach (TemperatureMeasurement tm in pt.TemperatureFunction.Item)
                    {
                        nodes.Add(tm.NodeId); types.Add(typeof(object));
                    }
                }
            }

            foreach (var item in nodes)
            {
                logger.Debug(CultureInfo.InvariantCulture, "NodeIds to read: \"{0}\"", item.ToString());
            }

            // Read all of the inputs
            try
            {
                _client.OpcSession.ReadValues(nodes, types, out result, out errors);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error reading values from OPC.");
                return;
            }

            int it = 0;
            foreach (var c in conf.ConfigList.Item)
            {
                foreach (var component in c.Composition.Item)
                {
                    component.Quality = errors[it].StatusCode;
                    if (string.IsNullOrEmpty(component.NodeId))
                    {
                        // If NodeId is empty, then we assume that Value is given a
                        // constant value in the config file
                        logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Component Value: {1} Name: {2} constant value",
                            c.Name, component.GetScaledValue(), component.Name);
                        continue;
                    }

                    if (StatusCode.IsGood(component.Quality))
                    {
                        component.Value = Convert.ToDouble(result[it], CultureInfo.InvariantCulture);
                        logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Component Value: {1} Name: {2} NodeId: \"{3}\"",
                            c.Name, component.GetScaledValue(), component.Name, component.NodeId);
                    }
                    else
                    {
                        logger.Warn(CultureInfo.InvariantCulture, "\"{0}\" NodeId: \"{1}\" Quality: \"{2}\"",
                            c.Name, component.NodeId, component.Quality.ToString());
                    }

                    it++;
                }

                foreach (var pt in c.PressureTemperatureList.Item)
                {
                    foreach (PressureMeasurement pm in pt.PressureFunction.Item)
                    {
                        pm.Quality = errors[it].StatusCode;
                        if (StatusCode.IsGood(pm.Quality))
                        {
                            pm.Value = Convert.ToDouble(result[it], CultureInfo.InvariantCulture);
                            logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" PressureFunc Value: {1} Unit: \"{2}\" NodeId: \"{3}\"",
                                pm.Name, pm.Value, pm.Unit, pm.NodeId);
                        }
                        else
                        {
                            pt.PressureFunction.Quality = pm.Quality;
                            logger.Warn(CultureInfo.InvariantCulture, "\"{0}\" NodeId: \"{1}\" Quality: \"{2}\"",
                                pm.Name, pm.NodeId, pm.Quality.ToString());
                        }

                        it++;
                    }

                    logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Pressure function: \"{1}\" Result value: {2} kPa {3}",
                        pt.Name, pt.PressureFunction.MathFunction.ToString(), pt.PressureFunction.GetValue(), pt.PressureFunction.Quality.ToString());

                    foreach (TemperatureMeasurement tm in pt.TemperatureFunction.Item)
                    {
                        tm.Quality = errors[it].StatusCode;
                        if (StatusCode.IsGood(tm.Quality))
                        {
                            tm.Value = Convert.ToDouble(result[it], CultureInfo.InvariantCulture);
                            logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" TemperatureFunc Value: {1} Unit: \"{2}\" NodeId: \"{3}\"",
                                tm.Name, tm.Value, tm.Unit, tm.NodeId);
                        }
                        else
                        {
                            pt.TemperatureFunction.Quality = tm.Quality;
                            logger.Warn(CultureInfo.InvariantCulture, "\"{0}\" NodeId: \"{1}\" Quality: \"{2}\"",
                                tm.Name, tm.NodeId, tm.Quality.ToString());
                        }

                        it++;
                    }

                    logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Temperature function: \"{1}\" Result value: {2} K {3}",
                        pt.Name, pt.TemperatureFunction.MathFunction.ToString(), pt.TemperatureFunction.GetValue(), pt.TemperatureFunction.Quality.ToString());
                }
            }
        }

        private void Calculate()
        {
            var compositionError = new CompositionError();
            var densityError = new DensityError();
            IEquation equation = conf.EquationOfState switch
            {
                ConfigModel.Equation.AGA8Detail => new AGA8Detail(),
                ConfigModel.Equation.Gerg2008 => new Gerg2008(),
                _ => new AGA8Detail(),
            };

            foreach (var c in conf.ConfigList.Item)
            {
                equation.SetComposition(c.Composition.GetScaledValues(), ref compositionError);

                if (compositionError != CompositionError.Ok)
                {
                    c.Composition.Quality = StatusCodes.Bad;
                    logger.Error(CultureInfo.InvariantCulture, "Invalid composition for {0}: {1}",
                        c.Name, compositionError.ToString());
                    continue;
                }

                foreach (var pt in c.PressureTemperatureList.Item)
                {
                    equation.SetPressure(pt.PressureFunction.GetValue());
                    equation.SetTemperature(pt.TemperatureFunction.GetValue());
                    equation.CalculateDensity(ref densityError);
                    if (densityError != DensityError.Ok)
                    {
                        logger.Error(CultureInfo.InvariantCulture, "Failed to calculate density for {0}: {1}",
                            pt.Name, densityError.ToString());
                        continue;
                    }

                    equation.CalculateProperties();
                    foreach (var property in pt.Properties.Item)
                    {
                        property.Value = equation.GetProperty(property.Property);
                        logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Property: \"{1}\" Value: {2}",
                            pt.Name, property.Property.ToString(), property.Value);
                    }
                }
            }

            equation.Dispose();
        }

        private void WriteToOPC()
        {
            // Make a list of all the OPC items that we want to write
            WriteValueCollection wvc = new();

            foreach (var c in conf.ConfigList.Item)
            {
                foreach (var pt in c.PressureTemperatureList.Item)
                {
                    foreach (var property in pt.Properties.Item)
                    {
                        StatusCode status = new() { Code = StatusCodes.Good };

                        if (StatusCode.IsNotGood(c.Composition.Quality)
                            | StatusCode.IsNotGood(pt.TemperatureFunction.Quality)
                            | StatusCode.IsNotGood(pt.PressureFunction.Quality))
                        {
                            status.Code = StatusCodes.Uncertain;
                        }

                        wvc.Add(new WriteValue
                        {
                            NodeId = property.NodeId,
                            AttributeId = Attributes.Value,
                            Value = new DataValue { Value = property.GetTypedValue(), StatusCode = status }
                        });
                    }
                }
            }

            foreach (var item in wvc)
            {
                logger.Debug(CultureInfo.InvariantCulture, "Item to write: \"{0}\" Value: {1} {2}",
                    item.NodeId.ToString(),
                    item.Value.Value,
                    item.Value.StatusCode.ToString());
            }

            try
            {
                _client.OpcSession.Write(null, wvc, out StatusCodeCollection results, out DiagnosticInfoCollection diagnosticInfos);

                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].Code != 0)
                    {
                        logger.Error(CultureInfo.InvariantCulture, "Write result: \"{0}\" Tag: \"{1}\" Value: \"{2}\" Type: \"{3}\"",
                            results[i].ToString(), wvc[i].NodeId, wvc[i].Value.Value, wvc[i].Value.Value.GetType().ToString());
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error writing OPC items");
            }
        }

        public async void Start()
        {
            logger.Info("Starting service.");
            await _client.Connect();
            GenerateNodeIds();
            _timer.Start();
        }

        public void Stop()
        {
            logger.Info("Stop service command received.");
            _timer.Stop();
            logger.Info("Waiting for current worker.");
            lock (WorkerLock)
            {
                logger.Info("Worker is done.");
                logger.Info("Disconnecting from OPC server.");
                _client.DisConnect();
            }
            logger.Info("Stopping service.");
        }

        private string GenerateNodeIdString(Measurement m)
        {
            if (m.Identifier == null)
            {
                return null;
            }
            // Normally use the default namespace URI
            string namespaceURI = conf.DefaultNamespaceURI;
            // Use custom namespace URI if it exists
            if (!string.IsNullOrEmpty(m.NamespaceURI)) { namespaceURI = m.NamespaceURI; }
            int namespaceIndex = Array.IndexOf(_client.OpcSession.NamespaceUris.ToArray(), namespaceURI);
            if (namespaceIndex < 0)
            {
                logger.Error(CultureInfo.InvariantCulture, "Namespace URI \"{0}\" not found.", namespaceURI);
                return null;
            }

            return String.Format("ns={0};{1}", namespaceIndex, m.Identifier);
        }
        private void GenerateNodeIds()
        {
            foreach (var c in conf.ConfigList.Item)
            {
                BrowsePathCollection pathsToTranslate = new();
                List<string> paths = new();
                TypeTable typeTable = new(new NamespaceTable());

                foreach (Component comp in c.Composition.Item)
                {
                    if (string.IsNullOrEmpty(comp.Identifier) && string.IsNullOrEmpty(comp.RelativePath))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(comp.Identifier) && !string.IsNullOrEmpty(comp.RelativePath))
                    {
                        logger.Warn(CultureInfo.InvariantCulture, "Identifier \"{0}\" and RelativePath \"{1}\" defined for \"{2}\". Identifier will be used and RelativePath will be ignored.", comp.Identifier, comp.RelativePath, comp.Name);
                    }

                    string namespaceURI = conf.DefaultNamespaceURI;
                    if (!string.IsNullOrEmpty(comp.NamespaceURI)) { namespaceURI = comp.NamespaceURI; }
                    int namespaceIndex = Array.IndexOf(_client.OpcSession.NamespaceUris.ToArray(), namespaceURI);
                    if (namespaceIndex < 0)
                    {
                        logger.Error(CultureInfo.InvariantCulture, "Namespace URI \"{0}\" not found.", namespaceURI);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(comp.RelativePath) && string.IsNullOrEmpty(comp.Identifier))
                    {
                        BrowsePath pathToTranslate = new();
                        NodeId startNode;
                        
                        if (!string.IsNullOrEmpty(comp.StartIdentifier))
                        {
                            startNode = new NodeId(String.Format("ns={0};{1}", namespaceIndex, comp.StartIdentifier));
                        }
                        else
                        {
                            startNode = new NodeId(ObjectIds.ObjectsFolder);
                        }

                        paths.Add(comp.RelativePath);

                        RelativePath path = RelativePath.Parse(comp.RelativePath, typeTable);
                        path.Elements.ForEach(e => { e.TargetName = new QualifiedName(e.TargetName.Name, (ushort)namespaceIndex); });

                        pathToTranslate.StartingNode = startNode;
                        pathToTranslate.RelativePath = path;
                        pathsToTranslate.Add(pathToTranslate);
                    }

                    if (!string.IsNullOrEmpty(comp.Identifier))
                    {
                        comp.NodeId = String.Format("ns={0};{1}", namespaceIndex, comp.Identifier);
                    }
                }

                BrowsePathResultCollection results = null;
                DiagnosticInfoCollection diagnosticInfos = null;
                ResponseHeader response_header;

                try
                {
                    response_header = _client.OpcSession.TranslateBrowsePathsToNodeIds(null, pathsToTranslate, out results, out diagnosticInfos);
                }
                catch (Exception)
                {

                    throw;
                }

                foreach (var comp in c.Composition.Item)
                {
                    if (string.IsNullOrEmpty(comp.Identifier) && !string.IsNullOrEmpty(comp.RelativePath))
                    {
                        int index = Array.IndexOf(paths.ToArray(), comp.RelativePath);
                        if (index >= 0 && StatusCode.IsGood(results[index].StatusCode))
                        {
                            comp.NodeId = results[index].Targets[0].TargetId.ToString();
                            logger.Debug(CultureInfo.InvariantCulture, "RelativePath \"{0}\" translates to NodeId \"{1}\"", comp.RelativePath, comp.NodeId);
                        }
                        else
                        {
                            logger.Error(CultureInfo.InvariantCulture, "RelativePath \"{0}\" failed to translate: \"{1}\"", comp.RelativePath, results[index].StatusCode);
                        }
                    }
                }


                foreach (PressureTemperature pt in c.PressureTemperatureList.Item)
                {
                    foreach (PressureMeasurement pm in pt.PressureFunction.Item)
                    {
                        pm.NodeId = GenerateNodeIdString(pm);
                    }
                    foreach (TemperatureMeasurement tm in pt.TemperatureFunction.Item)
                    {
                        tm.NodeId = GenerateNodeIdString(tm);
                    }

                    foreach (var property in pt.Properties.Item)
                    {
                        property.NodeId = GenerateNodeIdString(property);
                    }
                }
            }
        }
    }
}
