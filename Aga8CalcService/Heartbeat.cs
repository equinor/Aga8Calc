using Opc.Ua;
using System;
using System.Globalization;
using System.Timers;

namespace Aga8CalcService
{
    public class Heartbeat : IDisposable
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
            _timer = new System.Timers.Timer(conf.Interval) { AutoReset = true, SynchronizingObject = null };
            _timer.Elapsed += TimerElapsed;

            _client = new Aga8OpcClient(conf.OpcUrl, conf.OpcUser, conf.OpcPassword);
        }

        public void Dispose()
        {
            _client.Dispose();
            _timer.Dispose();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (Config c in conf.ConfigList.Item)
                {
                    c.Pressure = Convert.ToDouble(_client.OpcSession.ReadValue(c.PressureTag).Value, CultureInfo.InvariantCulture);
                    logger.Debug(CultureInfo.InvariantCulture, "Pressure: {0} kPa", c.GetConvertedPressure(c.PressureUnit));
                    c.Temperature = Convert.ToDouble(_client.OpcSession.ReadValue(c.TemperatureTag).Value, CultureInfo.InvariantCulture);
                    logger.Debug(CultureInfo.InvariantCulture, "Temperature: {0} K", c.GetConvertedTemperature(c.TemperatureUnit));
                    for (int i = 0; i < c.CompositionTag.Length; i++)
                    {
                        if (c.CompositionTag[i] != null)
                        {
                            c.GetComposition()[i] = Convert.ToDouble(_client.OpcSession.ReadValue(c.CompositionTag[i]).Value, CultureInfo.InvariantCulture);
                            logger.Debug(CultureInfo.InvariantCulture, "{0}: {1} mole fraction", c.CompositionTag[i], c.GetScaledComposition()[i]);
                        }
                    }
                }

                foreach (Config c in conf.ConfigList.Item)
                {
                    c.Result = NativeMethods.Aga8(c.GetScaledComposition(),
                        c.GetConvertedPressure(c.PressureUnit),
                        c.GetConvertedTemperature(c.TemperatureUnit),
                        c.Calculation);
                    logger.Debug(CultureInfo.InvariantCulture, "Result: {0}: {1}", c.Calculation.ToString(), c.Result);

                    var resultType = _client.OpcSession.ReadValue(c.ResultTag).Value.GetType();
                    WriteValue wv = new WriteValue
                    {
                        NodeId = c.ResultTag,
                        AttributeId = Attributes.Value
                    };
                    if (resultType == typeof(float))
                    {
                        wv.Value.Value = Convert.ToSingle(c.Result);
                    }
                    else if (resultType == typeof(double))
                    {
                        wv.Value.Value = Convert.ToDouble(c.Result);
                    }
                    
                    wv.Value.StatusCode = StatusCodes.Good;

                    WriteValueCollection wvc = new WriteValueCollection
                    {
                        wv
                    };

                    _client.OpcSession.Write(null, wvc, out StatusCodeCollection results, out DiagnosticInfoCollection diagnosticInfos);
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
