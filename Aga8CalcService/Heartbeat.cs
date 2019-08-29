using Opc.Ua;
using System;
using System.Globalization;
using System.Threading;
using System.Timers;

namespace Aga8CalcService
{
    public class Heartbeat
    {
        private readonly System.Timers.Timer _timer;
        private readonly Aga8OpcClient _client;
        private readonly ConfigFile conf;
        public Heartbeat()
        {
            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Tag_Config.xml";
            conf = ConfigFile.ReadConfig(TagConfFile);

            _timer = new System.Timers.Timer(conf.Interval) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;

            int stopTimeout = Timeout.Infinite;
            bool autoAccept = false;

            _client = new Aga8OpcClient(conf.OpcUrl, autoAccept, stopTimeout, conf.OpcUser, conf.OpcPassword);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (Config c in conf.ConfigList.Item)
                {
                    c.Pressure = Convert.ToDouble(_client.session.ReadValue(c.PressureTag).Value, CultureInfo.InvariantCulture) * 100.0;
                    Console.WriteLine("Pressure: {0} kPa", c.Pressure);
                    c.Temperature = Convert.ToDouble(_client.session.ReadValue(c.TemperatureTag).Value, CultureInfo.InvariantCulture) + 273.15;
                    Console.WriteLine("Temperature: {0} K", c.Temperature);
                    for (int i = 0; i < c.CompositionTag.Length; i++)
                    {
                        if (c.CompositionTag[i] != null)
                        {
                            c.GetComposition()[i] = Convert.ToDouble(_client.session.ReadValue(c.CompositionTag[i]).Value, CultureInfo.InvariantCulture) / 100.0;
                            Console.WriteLine("{0}: {1} mole fraction", c.CompositionTag[i], c.GetComposition()[i]);
                        }
                    }
                }

                foreach (Config c in conf.ConfigList.Item)
                {
                    c.Result = NativeMethods.Aga8_2017(c.GetComposition(), c.Pressure, c.Temperature, c.Calculation);
                    Console.WriteLine(c.Result);

                    WriteValue wv = new WriteValue();
                    wv.NodeId = c.ResultTag;
                    wv.AttributeId = Attributes.Value;
                    wv.Value.Value = c.Result;
                    wv.Value.StatusCode = StatusCodes.Good;

                    WriteValueCollection wvc = new WriteValueCollection();
                    wvc.Add(wv);

                    StatusCodeCollection results = null;
                    DiagnosticInfoCollection diagnosticInfos = null;

                    _client.session.Write(null, wvc, out results, out diagnosticInfos);
                }
            }
            catch
            {
                Console.WriteLine(@"Opc error!");
            }
        }

        public async void Start()
        {
            _timer.Start();
            await _client.Connect();
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
            _client.DisConnect();
        }
    }

}
