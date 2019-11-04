using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aga8CalcService;
using System.Threading.Tasks;

namespace Aga8Tests
{
    [TestClass]
    public class Aga8CalcTests
    {
        [TestMethod]
        public void ReadConfig_ReadsAConfigFile()
        {
            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "\\Tag_Config_Test.xml";
            ConfigFile conf = ConfigFile.ReadConfig(TagConfFile);

            Assert.AreEqual("opc.tcp://lt-103009:62548/Quickstarts/DataAccessServer", conf.OpcUrl);
            Assert.AreEqual("username", conf.OpcUser);
            Assert.AreEqual("password", conf.OpcPassword);
            Assert.AreEqual(42.0, conf.Interval, 1.0e-5);
            Assert.AreEqual("ns=2;s=ABB_800xA_Surrogate.S.24AI1234_A", conf.ConfigList.Item[0].CompositionTag[0]);
            Assert.AreEqual("ns=2;s=ABB_800xA_Surrogate.S.24AI1234_J", conf.ConfigList.Item[0].CompositionTag[1]);
            Assert.AreEqual("ns=2;s=ABB_800xA_Surrogate.S.24AI1234_K", conf.ConfigList.Item[0].CompositionTag[2]);
            Assert.AreEqual("ns=2;s=ABB_800xA_Surrogate.S.24AI1234_B", conf.ConfigList.Item[0].CompositionTag[3]);
            Assert.AreEqual("ns=2;s=ABB_800xA_Surrogate.S.24AI1234_C", conf.ConfigList.Item[0].CompositionTag[4]);
            Assert.AreEqual("ns=2;s=ABB_800xA_Surrogate.S.24AI1234_D", conf.ConfigList.Item[0].CompositionTag[5]);
            Assert.AreEqual("ns=2;s=ABB_800xA_Surrogate.S.24AI1234_E", conf.ConfigList.Item[0].CompositionTag[6]);
            Assert.AreEqual("ns=2;s=ABB_800xA_Surrogate.S.24AI1234_F", conf.ConfigList.Item[0].CompositionTag[7]);
            Assert.AreEqual("ns=2;s=ABB_800xA_Surrogate.S.24AI1234_G", conf.ConfigList.Item[0].CompositionTag[8]);
            Assert.AreEqual("ns=2;s=ABB_800xA_Surrogate.S.24AI1234_I", conf.ConfigList.Item[0].CompositionTag[9]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[10]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[11]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[12]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[13]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[14]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[15]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[16]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[17]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[18]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[19]);
            Assert.IsNull(conf.ConfigList.Item[0].CompositionTag[20]);
            Assert.AreEqual("24PI1234", conf.ConfigList.Item[0].PressureTag);
            Assert.AreEqual(Config.PressureUnits.barg, conf.ConfigList.Item[0].PressureUnit);
            Assert.AreEqual("24TI1234", conf.ConfigList.Item[0].TemperatureTag);
            Assert.AreEqual(Config.TemperatureUnits.C, conf.ConfigList.Item[0].TemperatureUnit);
            Assert.AreEqual("24DI1234", conf.ConfigList.Item[0].ResultTag);
            Assert.AreEqual(Config.Aga8ResultCode.Density, conf.ConfigList.Item[0].Calculation);
        }

        [TestMethod]
        public void Aga8_CalculatesDensity()
        {
            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "\\Tag_Config_Test.xml";
            ConfigFile conf = ConfigFile.ReadConfig(TagConfFile);

            double[] composition = {
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
            // Set pressure in barg
            double press = 498.98675;
            // Set temperature in C°
            double temp = 126.85;
            conf.ConfigList.Item[0].SetComposition(composition);
            conf.ConfigList.Item[0].Pressure = press;
            conf.ConfigList.Item[0].Temperature = temp;
            conf.ConfigList.Item[0].Calculation = Config.Aga8ResultCode.MolarConcentration;

            conf.ConfigList.Item[0].Result = Aga8CalcService.NativeMethods.Aga8(
                conf.ConfigList.Item[0].GetComposition(),
                conf.ConfigList.Item[0].GetConvertedPressure(conf.ConfigList.Item[0].PressureUnit),
                conf.ConfigList.Item[0].GetConvertedTemperature(conf.ConfigList.Item[0].TemperatureUnit),
                conf.ConfigList.Item[0].Calculation
            );

            Assert.AreEqual(12.807_924_036_488_01, conf.ConfigList.Item[0].Result, 1e-9);
        }

        // This test needs to have the OPC server mentioned
        // in the configuration file running.
        [TestMethod]
        public async Task Aga8OpcClient_Constructor()
        {
            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "\\Tag_Config_Test.xml";
            ConfigFile conf = ConfigFile.ReadConfig(TagConfFile);

            Aga8OpcClient client = new Aga8OpcClient(conf.OpcUrl, conf.OpcUser, conf.OpcPassword);
            Assert.IsNull(client.OpcSession);
            await client.Connect();
            Assert.IsNotNull(client.OpcSession);
            Assert.IsTrue(client.OpcSession.Connected);
            client.DisConnect();
        }
        [TestClass]
        public class ConfigTests
        {
            double accuracy= 0.00001;
            [TestMethod]
            public void GetConvertedTemperature_UnitIsCelsius_ReturnKelvin()
            {
              

                var config = new Config();
                double testTempCelsius = 446.30000;
                config.Temperature = 173.15000;
        
                double resultCelsius = config.GetConvertedTemperature(Config.TemperatureUnits.C);
                
               
              
                Assert.AreEqual(testTempCelsius, resultCelsius,accuracy);
                            
            }

            [TestMethod]
            public void GetConvertedTemperature_UnitIsKelvin_ReturnKelvin()
            {
               
                var config = new Config();
                double testTempKelvin = 73.15000;
                config.Temperature = testTempKelvin;

               
                double resultKelvin = config.GetConvertedTemperature(Config.TemperatureUnits.K);
               
               
                Assert.AreEqual(testTempKelvin, resultKelvin,accuracy);                
            }

            [TestMethod]
            public void GetConvertedPressure_UnitIsBarg_ReturnKPa()
            {
              
                var config = new Config();               
                config.Pressure = 111.700;
                double testPressureKPa = 11271.325;


                double resultKPa = config.GetConvertedPressure(Config.PressureUnits.barg);

                Assert.AreEqual(testPressureKPa, resultKPa, accuracy);


            }
            [TestMethod]
            public void GetConvertedPressure_UnitIsBara_ReturnKPa()
            {

                var config = new Config();
                config.Pressure = 157.7564;
                double testPressureKPa = 15775.64;


                double resultKPa = config.GetConvertedPressure(Config.PressureUnits.bara);

                Assert.AreEqual(testPressureKPa, resultKPa, accuracy);

            }
        }

       
    }
}
