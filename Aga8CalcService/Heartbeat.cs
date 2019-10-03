using Opc.Ua;
using System;
using System.Globalization;
using System.Timers;

namespace Aga8CalcService
{
    public class Heartbeat
    {
        private readonly System.Timers.Timer _timer;
        private readonly Aga8OpcClient _client;
        private readonly ConfigFile conf;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Heartbeat()
        {
            logger.Info("Initializing service.");
            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "Tag_Config.xml";
            try
            {
                conf = ConfigFile.ReadConfig(TagConfFile);

            }
            catch (Exception e)
            {
                logger.Fatal(e, "Failed to read tag configuration.");
                throw;
            }
            _timer = new System.Timers.Timer(conf.Interval) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;

            bool autoAccept = false;

            _client = new Aga8OpcClient(conf.OpcUrl, autoAccept, conf.OpcUser, conf.OpcPassword);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (Config c in conf.ConfigList.Item)
                {
                    c.Pressure = Convert.ToDouble(_client.OpcSession.ReadValue(c.PressureTag).Value, CultureInfo.InvariantCulture) * 100.0;
                    logger.Debug("Pressure: {0} kPa", c.Pressure);
                    c.Temperature = Convert.ToDouble(_client.OpcSession.ReadValue(c.TemperatureTag).Value, CultureInfo.InvariantCulture) + 273.15;
                    logger.Debug("Temperature: {0} K", c.Temperature);
                    for (int i = 0; i < c.CompositionTag.Length; i++)
                    {
                        if (c.CompositionTag[i] != null)
                        {
                            c.GetComposition()[i] = Convert.ToDouble(_client.OpcSession.ReadValue(c.CompositionTag[i]).Value, CultureInfo.InvariantCulture) / 100.0;
                            logger.Debug("{0}: {1} mole fraction", c.CompositionTag[i], c.GetComposition()[i]);
                        }
                    }
                }

                foreach (Config c in conf.ConfigList.Item)
                {
                    c.Result = NativeMethods.Aga8_2017(c.GetComposition(), c.Pressure, c.Temperature, c.Calculation);
                    logger.Debug("Result: {0}: {1}", c.Calculation.ToString(), c.Result);

                    WriteValue wv = new WriteValue();
                    wv.NodeId = c.ResultTag;
                    wv.AttributeId = Attributes.Value;
                    wv.Value.Value = c.Result;
                    wv.Value.StatusCode = StatusCodes.Good;

                    WriteValueCollection wvc = new WriteValueCollection();
                    wvc.Add(wv);

                    StatusCodeCollection results = null;
                    DiagnosticInfoCollection diagnosticInfos = null;

                    _client.OpcSession.Write(null, wvc, out results, out diagnosticInfos);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Opc error.");
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
            logger.Info("Stopping service.");
            _timer.Stop();
            _timer.Dispose();
            _client.DisConnect();
        }
    }

}
