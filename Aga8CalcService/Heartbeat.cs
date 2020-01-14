using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;

namespace Aga8CalcService
{
    public class Heartbeat : IDisposable
    {
        private readonly System.Timers.Timer _timer;
        private readonly Aga8OpcClient _client;
        private readonly ConfigModel conf;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly object WorkerLock = new object();

        public Heartbeat()
        {
            logger.Info("Initializing service.");
            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "Aga8Calc.config";
            try
            {
                conf = ConfigModel.ReadConfig(TagConfFile);

            }
            catch (Exception e)
            {
                logger.Fatal(e, "Failed to read tag configuration.");
                throw;
            }
            _timer = new System.Timers.Timer(conf.Interval) { AutoReset = true, SynchronizingObject = null };
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
            lock (WorkerLock)
            {
                ReadFromOPC();
                Calculate();
                WriteToOPC();
            }
        }

        private void ReadFromOPC()
        {
            NodeIdCollection nodes = new NodeIdCollection();
            List<Type> types = new List<Type>();
            List<object> result = new List<object>();
            List<ServiceResult> errors = new List<ServiceResult>();

            // Make a list of all the OPC items that we want to read
            foreach (var c in conf.ConfigList.Item)
            {
                foreach (Component comp in c.Composition.Item)
                {
                    nodes.Add(comp.Tag); types.Add(typeof(object));
                }

                foreach (PressureTemperature pt in c.PressureTemperatureList.Item)
                {
                    nodes.Add(pt.Pressure.Tag); types.Add(typeof(object));
                    nodes.Add(pt.Temperature.Tag); types.Add(typeof(object));
                }
            }

            foreach (var item in nodes)
            {
                logger.Debug(CultureInfo.InvariantCulture, "Item to read: \"{0}\"", item.ToString());
            }

            // Read all of the inputs
            try
            {
                _client.OpcSession.ReadValues(nodes, types, out result, out errors);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error reading values from OPC.");
            }

            int it = 0;
            foreach (var c in conf.ConfigList.Item)
            {
                foreach (var component in c.Composition.Item)
                {
                    component.Value = Convert.ToDouble(result[it++], CultureInfo.InvariantCulture);
                    logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Component Value: {1} Name: {2} Tag: \"{3}\"",
                        c.Name, component.GetScaledValue(), component.Name, component.Tag);
                }

                foreach (var pt in c.PressureTemperatureList.Item)
                {
                    pt.Pressure.Value = Convert.ToDouble(result[it++], CultureInfo.InvariantCulture);
                    logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Pressure Value: {1} Unit: \"{2}\" Tag: \"{3}\"",
                        pt.Name, pt.Pressure.Value, pt.Pressure.Unit, pt.Pressure.Tag);
                    pt.Temperature.Value = Convert.ToDouble(result[it++], CultureInfo.InvariantCulture);
                    logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Temperature Value: {1} Unit: \"{2}\" Tag: \"{3}\"",
                        pt.Name, pt.Temperature.Value, pt.Temperature.Unit, pt.Temperature.Tag);
                }
            }
        }

        private void Calculate()
        {
            var aga = new AGA8Detail();
            aga.Setup();

            foreach (var c in conf.ConfigList.Item)
            {
                aga.SetComposition(c.Composition.GetScaledValues());

                foreach (var pt in c.PressureTemperatureList.Item)
                {
                    aga.SetPressure(pt.Pressure.GetAGA8Converted());
                    aga.SetTemperature(pt.Temperature.GetAGA8Converted());
                    aga.CalculateDensity();
                    aga.CalculateProperties();

                    foreach (var property in pt.Properties.Item)
                    {
                        property.Value = aga.GetProperty(property.Property);
                        logger.Debug(CultureInfo.InvariantCulture, "\"{0}\" Property: \"{1}\" Value: {2}",
                            pt.Name, property.Property.ToString(), property.Value);
                    }
                }
            }

            aga.Dispose();
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
                            NodeId = property.Tag,
                            AttributeId = Attributes.Value,
                            Value = new DataValue { Value = property.GetTypedValue()}
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
    }
}
