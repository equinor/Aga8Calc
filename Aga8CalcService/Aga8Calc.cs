﻿using Opc.Ua;
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
        private readonly object WorkerLock = new object();
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
            NodeIdCollection nodes = new NodeIdCollection();
            List<Type> types = new List<Type>();
            List<object> result = new List<object>();
            List<ServiceResult> errors = new List<ServiceResult>();

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
                    if (string.IsNullOrEmpty(component.Identifier))
                    {
                        logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Component Value: {1} Name: {2}",
                            c.Name, component.GetScaledValue(), component.Name);
                        continue;
                    }

                    if (StatusCode.IsGood(errors[it].StatusCode))
                    {
                        component.Value = Convert.ToDouble(result[it], CultureInfo.InvariantCulture);
                        logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Component Value: {1} Name: {2} NodeId: \"{3}\"",
                            c.Name, component.GetScaledValue(), component.Name, component.NodeId);
                    }
                    else
                    {
                        logger.Warn(CultureInfo.InvariantCulture, "\"{0}\" NodeId: \"{1}\" Quality: \"{2}\"",
                            c.Name, component.NodeId, errors[it].ToString());
                    }

                    it++;
                }

                foreach (var pt in c.PressureTemperatureList.Item)
                {
                    foreach (PressureMeasurement pm in pt.PressureFunction.Item)
                    {
                        if (StatusCode.IsGood(errors[it].StatusCode))
                        {
                            pm.Value = Convert.ToDouble(result[it], CultureInfo.InvariantCulture);
                            logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" PressureFunc Value: {1} Unit: \"{2}\" NodeId: \"{3}\"",
                                pm.Name, pm.Value, pm.Unit, pm.NodeId);
                        }
                        else
                        {
                            logger.Warn(CultureInfo.InvariantCulture, "\"{0}\" NodeId: \"{1}\" Quality: \"{2}\"",
                                pm.Name, pm.NodeId, errors[it].ToString());
                        }

                        it++;
                    }

                    logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Pressure function: \"{1}\" Result value: {2} kPa",
                        pt.Name, pt.PressureFunction.MathFunction.ToString(), pt.PressureFunction.GetValue());

                    foreach (TemperatureMeasurement tm in pt.TemperatureFunction.Item)
                    {
                        if (StatusCode.IsGood(errors[it].StatusCode))
                        {
                            tm.Value = Convert.ToDouble(result[it], CultureInfo.InvariantCulture);
                            logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" TemperatureFunc Value: {1} Unit: \"{2}\" NodeId: \"{3}\"",
                                tm.Name, tm.Value, tm.Unit, tm.NodeId);
                        }
                        else
                        {
                            logger.Warn(CultureInfo.InvariantCulture, "\"{0}\" NodeId: \"{1}\" Quality: \"{2}\"",
                                tm.Name, tm.NodeId, errors[it].ToString());
                        }

                        it++;
                    }

                    logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Temperature function: \"{1}\" Result value: {2} K",
                        pt.Name, pt.TemperatureFunction.MathFunction.ToString(), pt.TemperatureFunction.GetValue());
                }
            }
        }

        private void Calculate()
        {
            var compositionError = new CompositionError();
            var densityError = new DensityError();

            IEquation equation;

            switch (conf.EquationOfState)
            {
                case ConfigModel.Equation.AGA8Detail:
                    equation = new AGA8Detail();
                    break;
                case ConfigModel.Equation.Gerg2008:
                    equation = new Gerg2008();
                    break;
                default:
                    equation = new AGA8Detail();
                    break;
            }

            foreach (var c in conf.ConfigList.Item)
            {
                equation.SetComposition(c.Composition.GetScaledValues(), ref compositionError);

                if (compositionError != CompositionError.Ok)
                {
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
            WriteValueCollection wvc = new WriteValueCollection();

            foreach (var c in conf.ConfigList.Item)
            {
                foreach (var pt in c.PressureTemperatureList.Item)
                {
                    foreach (var property in pt.Properties.Item)
                    {
                        wvc.Add(new WriteValue
                        {
                            NodeId = property.NodeId,
                            AttributeId = Attributes.Value,
                            Value = new DataValue { Value = property.GetTypedValue() }
                        });
                    }
                }
            }

            foreach (var item in wvc)
            {
                logger.Debug(CultureInfo.InvariantCulture, "Item to write: \"{0}\" Value: {1}",
                    item.NodeId.ToString(),
                    item.Value.Value);
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
                foreach (Component comp in c.Composition.Item)
                {
                    string namespaceURI = conf.DefaultNamespaceURI;
                    if (!string.IsNullOrEmpty(comp.NamespaceURI)) { namespaceURI = comp.NamespaceURI; }
                    int namespaceIndex = Array.IndexOf(_client.OpcSession.NamespaceUris.ToArray(), namespaceURI);
                    if (namespaceIndex < 0)
                    {
                        logger.Error(CultureInfo.InvariantCulture, "Namespace URI \"{0}\" not found.", namespaceURI);
                        continue;
                    }

                    comp.NodeId = String.Format("ns={0};{1}", namespaceIndex, comp.Identifier);
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
