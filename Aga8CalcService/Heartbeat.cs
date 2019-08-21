using Opc.Ua;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
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
            _timer = new System.Timers.Timer(10_000) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;

            int stopTimeout = Timeout.Infinite;
            bool autoAccept = false;

            conf = Aga8Calc.ReadConfig("Tag_Config.xml");

            string endpointURL = conf.opc_url;
            // use OPC UA .Net Sample server 
            //endpointURL = "opc.tcp://lt-103009:62548/Quickstarts/DataAccessServer";
            //endpointURL = "opc.tcp://127.0.0.1:49320";
            _client = new Aga8OpcClient(endpointURL, autoAccept, stopTimeout);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            double[] comp = {
                0.778_240, // Methane
                0.020_000, // Nitrogen
                0.060_000, // Carbon dioxide
                0.080_000, // Ethane
                0.030_000, // Propane
                0.001_500, // Isobutane
                0.003_000, // n-Butane
                0.000_500, // Isopentane
                0.001_650, // n-Pentane
                0.002_150, // Hexane
                0.000_880, // Heptane
                0.000_240, // Octane
                0.000_150, // Nonane
                0.000_090, // Decane
                0.004_000, // Hydrogen
                0.005_000, // Oxygen
                0.002_000, // Carbon monoxide
                0.000_100, // Water
                0.002_500, // Hydrogen sulfide
                0.007_000, // Helium
                0.001_000, // Argon
            };

            double press = 50_000.0;
            double tempr = 400.0;

            conf.configList.Items[0].pressure = 50_000.0;
            conf.configList.Items[0].temperature = 400.0;
            conf.configList.Items[0].composition = comp;

            Parallel.For(0, conf.configList.Items.Count, c =>
            {
                conf.configList.Items[c].result =
                Aga8Calc.aga8_2017(
                    conf.configList.Items[c].composition,
                    conf.configList.Items[c].pressure,
                    conf.configList.Items[c].temperature,
                    conf.configList.Items[c].calculation);

                Console.WriteLine("{0}: {1}", conf.configList.Items[c].calculation, conf.configList.Items[c].result);
            });

            Parallel.For(0, 10000, i =>
            {
                Aga8Calc.aga8_2017(comp, press, tempr, Config.Aga8ResultCode.SpeedOfSound);
            });

            foreach (Config c in conf.configList.Items)
            {
                c.result = Aga8Calc.aga8_2017(c.composition, c.pressure, c.temperature, c.calculation);
            }

            foreach (Config.Aga8ResultCode i in (Config.Aga8ResultCode[]) Enum.GetValues(typeof(Config.Aga8ResultCode)))
            {
                Console.WriteLine("{0}: {1}", Enum.GetName(typeof(Config.Aga8ResultCode), i), Aga8Calc.aga8_2017(comp, press, tempr, i));
            }

            try
            {
                DataValue[] meas = {
                    _client.session.ReadValue("ns=2;s=ABB_800xA_Surrogate.S.31AI0157A_A"),
                    _client.session.ReadValue("ns=2;s=ABB_800xA_Surrogate.S.31AI0157A_J"),
                    _client.session.ReadValue("ns=2;s=ABB_800xA_Surrogate.S.31AI0157A_K"),
                    _client.session.ReadValue("ns=2;s=ABB_800xA_Surrogate.S.31AI0157A_B"),
                    _client.session.ReadValue("ns=2;s=ABB_800xA_Surrogate.S.31AI0157A_C"),
                    _client.session.ReadValue("ns=2;s=ABB_800xA_Surrogate.S.31AI0157A_D"),
                    _client.session.ReadValue("ns=2;s=ABB_800xA_Surrogate.S.31AI0157A_E"),
                    _client.session.ReadValue("ns=2;s=ABB_800xA_Surrogate.S.31AI0157A_F"),
                    _client.session.ReadValue("ns=2;s=ABB_800xA_Surrogate.S.31AI0157A_G"),
                    _client.session.ReadValue("ns=2;s=ABB_800xA_Surrogate.S.31AI0157A_I"),
                };

                for (int i = 0; i < comp.Length; i++)
                {
                    if (i < meas.Length)
                    {
                        comp[i] = Convert.ToDouble(meas[i].Value, CultureInfo.InvariantCulture) / 100.0;
                    }
                    else
                    {
                        comp[i] = 0.0;
                    }
                    Console.WriteLine("Comp[{0}] = {1}", i, comp[i]);
                }

                Console.WriteLine(Aga8Calc.aga8_2017(comp, press, tempr, Config.Aga8ResultCode.Density));

                WriteValue wv = new WriteValue();
                wv.NodeId = "ns=2;s=ABB OPC Connect Server.PhaseOpt.31PY0161_A";
                wv.AttributeId = Attributes.Value;
                wv.Value.Value = (Variant)Aga8Calc.aga8_2017(comp, press, tempr, Config.Aga8ResultCode.Density);
                wv.Value.StatusCode = StatusCodes.Good;

                WriteValueCollection wvc = new WriteValueCollection();
                wvc.Add(wv);

                StatusCodeCollection results = null;
                DiagnosticInfoCollection diagnosticInfos = null;

                _client.session.Write(null, wvc, out results, out diagnosticInfos);

                Console.WriteLine("results:");
                Console.WriteLine(results[0].ToString());

            }
            catch
            {
                Console.WriteLine(@"Read error!");
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
