using Opc.Ua;
using Opc.Ua.Client;
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
        private Subscription subscription;

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
                Calculate();
                WriteToOPC();
                working = false;
            }

            watch.Stop();

            logger.Debug(CultureInfo.InvariantCulture, "Worker elapsed time: {0} ms.", watch.ElapsedMilliseconds);
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
                double compositionSum = 0.0;
                foreach (var comp in c.Composition.Item)
                {
                    compositionSum += comp.GetScaledValue();
                    logger.Debug(CultureInfo.InvariantCulture, "Config {0} Component {1}, scaled value {2}", c.Name, comp.Name, comp.GetScaledValue());
                }
                logger.Debug(CultureInfo.InvariantCulture, "Composition sum {0}", compositionSum);

                if (Math.Abs(compositionSum - 1.0) > 0.2)
                {
                    c.Composition.Quality = StatusCodes.Bad;
                    logger.Error(CultureInfo.InvariantCulture, "Calculation aborted. Cause: invalid composition for {0}: {1}",
                        c.Name, compositionError.ToString());
                    continue;
                }

                Aga8Composition composition = c.Composition.GetScaledValues();
                if (c.Composition.Normalize)
                {
                    logger.Debug(CultureInfo.InvariantCulture, "Normalizing composition for {0}", c.Name);
                    double factor = 1.0 / compositionSum;

                    composition.Methane *= factor;
                    composition.Nitrogen *= factor;
                    composition.CarbonDioxide *= factor;
                    composition.Ethane *= factor;
                    composition.Propane *= factor;
                    composition.IsoButane *= factor;
                    composition.NormalButane *= factor;
                    composition.IsoPentane *= factor;
                    composition.NormalPentane *= factor;
                    composition.Hexane *= factor;
                    composition.Heptane *= factor;
                    composition.Octane *= factor;
                    composition.Nonane *= factor;
                    composition.Decane *= factor;
                    composition.Hydrogen *= factor;
                    composition.Oxygen *= factor;
                    composition.CarbonMonoxide *= factor;
                    composition.Water *= factor;
                    composition.HydrogenSulfide *= factor;
                    composition.Helium *= factor;
                    composition.Argon *= factor;
                }

                equation.SetComposition(composition, ref compositionError);

                c.Composition.Quality = StatusCodes.Good;
                if (compositionError != CompositionError.Ok)
                {
                    c.Composition.Quality = StatusCodes.Bad;
                    logger.Error(CultureInfo.InvariantCulture, "Calculation aborted. Cause: invalid composition for {0}: {1}",
                        c.Name, compositionError.ToString());
                    continue;
                }

                foreach (var pt in c.PressureTemperatureList.Item)
                {
                    logger.Debug(CultureInfo.InvariantCulture, "{0} Pressure function {1}", pt.Name, pt.PressureFunction.GetValue());
                    logger.Debug(CultureInfo.InvariantCulture, "{0} Temperature function {1}", pt.Name, pt.TemperatureFunction.GetValue());

                    equation.SetPressure(pt.PressureFunction.GetValue());
                    equation.SetTemperature(pt.TemperatureFunction.GetValue());
                    equation.CalculateDensity(ref densityError);
                    if (densityError != DensityError.Ok)
                    {
                        logger.Error(CultureInfo.InvariantCulture, "Calculation aborted. Cause: failed to calculate density for {0}: {1}",
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
            WriteValueCollection wvc = [];

            foreach (var c in conf.ConfigList.Item)
            {
                foreach (var pt in c.PressureTemperatureList.Item)
                {
                    StatusCode status = new() { Code = StatusCodes.Good };

                    if (StatusCode.IsNotGood(c.Composition.Quality)
                        | StatusCode.IsNotGood(pt.TemperatureFunction.Quality)
                        | StatusCode.IsNotGood(pt.PressureFunction.Quality))
                    {
                        status.Code = StatusCodes.Bad;
                        logger.Error(CultureInfo.InvariantCulture, "PT point \"{0}\" invalid. Property values will not be written to OPC.", pt.Name);
                        continue;
                    }

                    foreach (var property in pt.Properties.Item)
                    {
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

            if (conf.ReadOnly)
            {
                logger.Info("Read only mode active. No values written to OPC server.");
                return;
            }

            try
            {
                StatusCodeCollection results = [];
                if (wvc.Count > 0)
                {
                    _client.OpcSession.Write(null, wvc, out results, out DiagnosticInfoCollection diagnosticInfos);
                }

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

            subscription = new Subscription(_client.OpcSession.DefaultSubscription)
            {
                DisplayName = "Aga8Calc",
                PublishingEnabled = true,
                PublishingInterval = Convert.ToInt32(conf.Interval / 2.0),
                LifetimeCount = 0,
                MinLifetimeInterval = 120_000,
            };
            _client.OpcSession.AddSubscription(subscription);
            subscription.Create();
            logger.Info("Subscription created with SubscriptionId = {0}", subscription.Id);

            foreach (var c in conf.ConfigList.Item)
            {
                foreach (Component comp in c.Composition.Item)
                {
                    if (string.IsNullOrEmpty(comp.NodeId)) { continue; }

                    MonitoredItem item = new(subscription.DefaultItem)
                    {
                        StartNodeId = comp.NodeId,
                        AttributeId = Attributes.Value,
                        DisplayName = comp.Name.ToString(),
                        SamplingInterval = c.Composition.SamplingInterval,
                        QueueSize = 1,
                        DiscardOldest = true,
                        MonitoringMode = MonitoringMode.Reporting
                    };
                    item.Notification += OnMonitoredItemNotification;

                    if (comp.SamplingInterval != -2)
                    {
                        item.SamplingInterval = comp.SamplingInterval;
                    }

                    subscription.AddItem(item);
                }

                foreach (PressureTemperature pt in c.PressureTemperatureList.Item)
                {
                    foreach (PressureMeasurement pm in pt.PressureFunction.Item)
                    {
                        if (string.IsNullOrEmpty(pm.NodeId)) { continue; }

                        MonitoredItem item = new(subscription.DefaultItem)
                        {
                            StartNodeId = pm.NodeId,
                            AttributeId = Attributes.Value,
                            DisplayName = pm.Name,
                            SamplingInterval = pm.SamplingInterval,
                            QueueSize = 1,
                            DiscardOldest = true,
                            MonitoringMode = MonitoringMode.Reporting
                        };
                        item.Notification += OnMonitoredItemNotification;

                        subscription.AddItem(item);
                    }
                    foreach (TemperatureMeasurement tm in pt.TemperatureFunction.Item)
                    {
                        if (string.IsNullOrEmpty(tm.NodeId)) { continue; }

                        MonitoredItem item = new(subscription.DefaultItem)
                        {
                            StartNodeId = tm.NodeId,
                            AttributeId = Attributes.Value,
                            DisplayName = tm.Name,
                            SamplingInterval = tm.SamplingInterval,
                            QueueSize = 1,
                            DiscardOldest = true,
                            MonitoringMode = MonitoringMode.Reporting
                        };
                        item.Notification += OnMonitoredItemNotification;

                        subscription.AddItem(item);
                    }
                }
            }

            subscription.ApplyChanges();
            subscription.StateChanged += null;
            logger.Info("MonitoredItems created for SubscriptionId = {0}", subscription.Id);
        }

        private void OnMonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            lock (WorkerLock)
            {
                try
                {
                    MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
                    logger.Debug(CultureInfo.InvariantCulture, "Subscription: {0}, Notification: {1} \"{2}\" Value = {3}", monitoredItem.Subscription.Id, notification.Message.SequenceNumber, monitoredItem.DisplayName, notification.Value);

                    if (notification != null)
                    {
                        foreach (var c in conf.ConfigList.Item)
                        {
                            foreach (Component comp in c.Composition.Item)
                            {
                                if (string.IsNullOrEmpty(comp.NodeId)) { continue; }

                                if (monitoredItem.StartNodeId.ToString() == comp.NodeId)
                                {
                                    comp.Value = Convert.ToDouble(notification.Value.Value);
                                }
                            }

                            foreach (PressureTemperature pt in c.PressureTemperatureList.Item)
                            {
                                foreach (PressureMeasurement pm in pt.PressureFunction.Item)
                                {
                                    if (string.IsNullOrEmpty(pm.NodeId)) { continue; }

                                    if (monitoredItem.StartNodeId.ToString() == pm.NodeId)
                                    {
                                        pm.Value = Convert.ToDouble(notification.Value.Value);
                                    }
                                }
                                foreach (TemperatureMeasurement tm in pt.TemperatureFunction.Item)
                                {
                                    if (string.IsNullOrEmpty(tm.NodeId)) { continue; }

                                    if (monitoredItem.StartNodeId.ToString() == tm.NodeId)
                                    {
                                        tm.Value = Convert.ToDouble(notification.Value.Value);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "OnMonitoredItemNotification error");
                }
            }
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

                subscription.Delete(true);
                _client.DisConnect();
            }
            logger.Info("Stopping service.");
        }

        private void GenerateNodeIdString(IEnumerable<Measurement> measurement)
        {
            BrowsePathCollection pathsToTranslate = [];
            List<string> paths = [];
            TypeTable typeTable = new(new NamespaceTable());

            foreach (var m in measurement)
            {
                if (string.IsNullOrEmpty(m.Identifier) && string.IsNullOrEmpty(m.RelativePath))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(m.Identifier) && !string.IsNullOrEmpty(m.RelativePath))
                {
                    logger.Warn(CultureInfo.InvariantCulture, "Identifier \"{0}\" and RelativePath \"{1}\" defined. Identifier will be used and RelativePath will be ignored.", m.Identifier, m.RelativePath);
                }

                string namespaceURI = conf.DefaultNamespaceURI;
                if (!string.IsNullOrEmpty(m.NamespaceURI)) { namespaceURI = m.NamespaceURI; }
                int namespaceIndex = Array.IndexOf(_client.OpcSession.NamespaceUris.ToArray(), namespaceURI);
                if (namespaceIndex < 0)
                {
                    logger.Error(CultureInfo.InvariantCulture, "Namespace URI \"{0}\" not found.", namespaceURI);
                    continue;
                }

                if (!string.IsNullOrEmpty(m.RelativePath) && string.IsNullOrEmpty(m.Identifier))
                {
                    BrowsePath pathToTranslate = new();
                    NodeId startNode;

                    if (!string.IsNullOrEmpty(m.StartIdentifier))
                    {
                        startNode = new NodeId(String.Format("ns={0};{1}", namespaceIndex, m.StartIdentifier));
                    }
                    else
                    {
                        startNode = new NodeId(ObjectIds.ObjectsFolder);
                    }

                    paths.Add(m.RelativePath);

                    RelativePath path = RelativePath.Parse(m.RelativePath, typeTable);
                    path.Elements.ForEach(e => { e.TargetName = new QualifiedName(e.TargetName.Name, (ushort)namespaceIndex); });

                    pathToTranslate.StartingNode = startNode;
                    pathToTranslate.RelativePath = path;
                    pathsToTranslate.Add(pathToTranslate);
                }

                if (!string.IsNullOrEmpty(m.Identifier))
                {
                    m.NodeId = String.Format("ns={0};{1}", namespaceIndex, m.Identifier);
                }
            }

            if (pathsToTranslate.Count > 0)
            {
                BrowsePathResultCollection results;
                try
                {
                    _client.OpcSession.TranslateBrowsePathsToNodeIds(null, pathsToTranslate, out results, out DiagnosticInfoCollection diagnosticInfos);
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "Failed to translate browse paths.");
                    throw;
                }

                foreach (var m in measurement)
                {
                    if (string.IsNullOrEmpty(m.Identifier) && !string.IsNullOrEmpty(m.RelativePath))
                    {
                        int index = Array.IndexOf([.. paths], m.RelativePath);
                        if (index >= 0 && StatusCode.IsGood(results[index].StatusCode))
                        {
                            if (results[index].Targets.Count > 1)
                            {
                                logger.Warn("Found multiple targets for path \"{0}\"", m.RelativePath);
                            }
                            m.NodeId = results[index].Targets[0].TargetId.ToString();
                            logger.Debug(CultureInfo.InvariantCulture, "RelativePath \"{0}\" translates to NodeId \"{1}\"", m.RelativePath, m.NodeId);
                        }
                        else
                        {
                            logger.Error(CultureInfo.InvariantCulture, "RelativePath \"{0}\" failed to translate: \"{1}\"", m.RelativePath, results[index].StatusCode);
                        }
                    }
                }
            }
        }

        private void GenerateNodeIds()
        {
            foreach (var c in conf.ConfigList.Item)
            {
                GenerateNodeIdString(c.Composition.Item);

                foreach (PressureTemperature pt in c.PressureTemperatureList.Item)
                {
                    GenerateNodeIdString(pt.PressureFunction.Item);
                    GenerateNodeIdString(pt.TemperatureFunction.Item);
                    GenerateNodeIdString(pt.Properties.Item);
                }
            }
        }
    }
}
