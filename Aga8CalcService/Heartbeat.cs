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
        public ConfigFile conf;
        public Heartbeat()
        {
            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Tag_Config.xml";
            conf = Aga8Calc.ReadConfig(TagConfFile);

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
                foreach (Config c in conf.configList.Items)
                {
                    c.pressure = Convert.ToDouble(_client.session.ReadValue(c.pressure_tag).Value, CultureInfo.InvariantCulture) * 100.0;
                    Console.WriteLine("Pressure: {0} kPa", c.pressure);
                    c.temperature = Convert.ToDouble(_client.session.ReadValue(c.temperature_tag).Value, CultureInfo.InvariantCulture) + 273.15;
                    Console.WriteLine("Temperature: {0} K", c.temperature);
                    for (int i = 0; i < c.composition_tag.Length; i++)
                    {
                        if (c.composition_tag[i] != null)
                        {
                            c.composition[i] = Convert.ToDouble(_client.session.ReadValue(c.composition_tag[i]).Value, CultureInfo.InvariantCulture) / 100.0;
                            Console.WriteLine("{0}: {1} mole fraction", c.composition_tag[i], c.composition[i]);
                        }
                    }
                }

                foreach (Config c in conf.configList.Items)
                {
                    c.result = Aga8Calc.Aga8_2017(c.composition, c.pressure, c.temperature, c.calculation);
                    Console.WriteLine(c.result);

                    WriteValue wv = new WriteValue();
                    wv.NodeId = c.result_tag;
                    wv.AttributeId = Attributes.Value;
                    wv.Value.Value = c.result;
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
