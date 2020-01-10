using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aga8CalcService;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Aga8Tests
{
    [TestClass]
    public class Aga8CalcTests
    {
        [TestMethod]
        public void GenerateAndReadConfigModel()
        {
            // Stream 0
            ConfigModel config = new ConfigModel
            {
                OpcUrl = "opc.tcp://localhost:62548/Quickstarts/DataAccessServer",
                OpcUser = "user",
                OpcPassword = "password",
            };

            config.ConfigList.Item.Add(new Config { Name = "GC name" });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Methane", Tag = "ns=2;s=1:AI1001?A", Value = 0.778_240 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Nitrogen", Tag = "ns=2;s=1:AI1001?A", Value = 0.020_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Carbon dioxide", Tag = "ns=2;s=1:AI1001?A", Value = 0.060_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Ethane", Tag = "ns=2;s=1:AI1001?A", Value = 0.080_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Propane", Tag = "ns=2;s=1:AI1001?A", Value = 0.030_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Isobutane", Tag = "ns=2;s=1:AI1001?A", Value = 0.001_500 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "n-Butane", Tag = "ns=2;s=1:AI1001?A", Value = 0.003_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Isopentane", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_500 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "n-Pentane", Tag = "ns=2;s=1:AI1001?A", Value = 0.001_650 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Hexane", Tag = "ns=2;s=1:AI1001?A", Value = 0.002_150 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Heptane", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_880 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Octane", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_240 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Nonane", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_150 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Decane", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_090 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Hydrogen", Tag = "ns=2;s=1:AI1001?A", Value = 0.004_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Oxygen", Tag = "ns=2;s=1:AI1001?A", Value = 0.005_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Carbon monoxide", Tag = "ns=2;s=1:AI1001?A", Value = 0.002_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Water", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_100 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Hydrogen sulfide", Tag = "ns=2;s=1:AI1001?A", Value = 0.002_500 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Helium", Tag = "ns=2;s=1:AI1001?A", Value = 0.007_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Argon", Tag = "ns=2;s=1:AI1001?A", Value = 0.001_000 });

            config.ConfigList.Item[0].PressureTemperatureList.Item.Add(new PressureTemperature
            {
                Name = "Point 1",
                Pressure = new PressureMeasurement { Tag = "ns=2;s=1:AI1001?Pressure" },
                Temperature = new TemperatureMeasurement { Tag = "ns=2;s=1:AI1001?Temperature" }
            });

            config.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item.Add(new PropertyMeasurement
            {
                Property = ConfigModel.Aga8ResultCode.MolarConcentration
            });

            XmlWriterSettings writerSettings = new XmlWriterSettings
            {
                Indent = true,
            };
            XmlWriter writer = XmlWriter.Create("aga8calc.xml", writerSettings);
            XmlSerializer configSerializer = new XmlSerializer(typeof(ConfigModel));
            configSerializer.Serialize(writer, config);
            writer.Close();

            string file = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "\\aga8calc.xml";
            ConfigModel readConfig = ConfigModel.ReadConfig(file);
        }

        /*
        [TestMethod]
        public void ReadConfig_ReadsAConfigFile()
        {
            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "\\Tag_Config_Test.xml";
            ConfigModel conf = ConfigModel.ReadConfig(TagConfFile);

            Assert.AreEqual("opc.tcp://lt-103009:62548/Quickstarts/DataAccessServer", conf.OpcUrl);
            Assert.AreEqual("username", conf.OpcUser);
            Assert.AreEqual("password", conf.OpcPassword);
            Assert.AreEqual(42.0, conf.Interval, 1.0e-5);
            Assert.AreEqual("ns=2;s=1:AI1001?A", conf.ConfigList.Item[0].Composition.Item[0].Tag);
            Assert.AreEqual("24PI1234", conf.ConfigList.Item[0].PressureTag);
            Assert.AreEqual(Config.PressureUnits.barg, conf.ConfigList.Item[0].PressureUnit);
            Assert.AreEqual("24TI1234", conf.ConfigList.Item[0].TemperatureTag);
            Assert.AreEqual(Config.TemperatureUnits.C, conf.ConfigList.Item[0].TemperatureUnit);
            Assert.AreEqual("24DI1234", conf.ConfigList.Item[0].ResultTag);
            Assert.AreEqual(Config.Aga8ResultCode.Density, conf.ConfigList.Item[0].Calculation);
        }
        */
        [TestMethod]
        public void Aga8_CalculatesDensity()
        {
            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "\\Tag_Config_Test.xml";
            ConfigModel conf = new ConfigModel();

            conf.ConfigList.Item.Add(new Config());
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Methane", Tag = "ns=2;s=1:AI1001?A", Value = 0.778_240 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Nitrogen", Tag = "ns=2;s=1:AI1001?A", Value = 0.020_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Carbon dioxide", Tag = "ns=2;s=1:AI1001?A", Value = 0.060_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Ethane", Tag = "ns=2;s=1:AI1001?A", Value = 0.080_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Propane", Tag = "ns=2;s=1:AI1001?A", Value = 0.030_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Isobutane", Tag = "ns=2;s=1:AI1001?A", Value = 0.001_500 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "n-Butane", Tag = "ns=2;s=1:AI1001?A", Value = 0.003_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Isopentane", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_500 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "n-Pentane", Tag = "ns=2;s=1:AI1001?A", Value = 0.001_650 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Hexane", Tag = "ns=2;s=1:AI1001?A", Value = 0.002_150 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Heptane", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_880 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Octane", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_240 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Nonane", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_150 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Decane", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_090 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Hydrogen", Tag = "ns=2;s=1:AI1001?A", Value = 0.004_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Oxygen", Tag = "ns=2;s=1:AI1001?A", Value = 0.005_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Carbon monoxide", Tag = "ns=2;s=1:AI1001?A", Value = 0.002_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Water", Tag = "ns=2;s=1:AI1001?A", Value = 0.000_100 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Hydrogen sulfide", Tag = "ns=2;s=1:AI1001?A", Value = 0.002_500 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Helium", Tag = "ns=2;s=1:AI1001?A", Value = 0.007_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = "Argon", Tag = "ns=2;s=1:AI1001?A", Value = 0.001_000 });

            conf.ConfigList.Item[0].PressureTemperatureList.Item.Add(new PressureTemperature
            {
                Pressure = new PressureMeasurement { Value = 498.98675, Unit = ConfigModel.PressureUnit.barg },
                Temperature = new TemperatureMeasurement { Value = 126.85, Unit = ConfigModel.TemperatureUnit.C }
            });

            conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item.Add(new PropertyMeasurement
            {
                Property = ConfigModel.Aga8ResultCode.MolarConcentration
            });

            conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value = Aga8CalcService.NativeMethods.Aga8(
                conf.ConfigList.Item[0].Composition.GetValues(),
                conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Pressure.GetAGA8Converted(),
                conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Temperature.GetAGA8Converted(),
                conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Property
            );

            Assert.AreEqual(12.807_924_036_488_01, conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
        }
        
        // This test needs to have the OPC server mentioned
        // in the configuration file running.
        [TestMethod]
        public async Task Aga8OpcClient_Constructor()
        {
            string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "\\Tag_Config_Test.xml";
            ConfigModel conf = ConfigModel.ReadConfig(TagConfFile);

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
            [TestMethod]
            public void TemperatureMeasurement_GetAGA8Converted_UnitIsCelsius_ReturnKelvin()
            {
                var temperature = new TemperatureMeasurement
                {
                    Value = 173.15,
                    Unit = ConfigModel.TemperatureUnit.C
                };
                double testTempKelvin = 446.3;

                double resultKelvin = temperature.GetAGA8Converted();
                Assert.AreEqual(testTempKelvin, resultKelvin, 1.0e-10);
            }

            [TestMethod]
            public void TemperatureMeasurement_GetAGA8Converted_UnitIsKelvin_ReturnKelvin()
            {
                var temperature = new TemperatureMeasurement();
                double testTempKelvin = 73.15;
                temperature.Value = testTempKelvin;
                temperature.Unit = ConfigModel.TemperatureUnit.K;

                double resultKelvin = temperature.GetAGA8Converted();
                Assert.AreEqual(testTempKelvin, resultKelvin, 1.0e-10);
            }

            [TestMethod]
            public void TemperatureMeasurement_GetUnitConverted_UnitIsCelsius_ReturnCelsius()
            {
                var temperature = new TemperatureMeasurement();
                double testTempCelsius = -17.335;
                temperature.Value = 255.815;
                temperature.Unit = ConfigModel.TemperatureUnit.C;

                double resultCelsius = temperature.GetUnitConverted();
                Assert.AreEqual(testTempCelsius, resultCelsius, 1.0e-10);
            }

            [TestMethod]
            public void TemperatureMeasurement_GetUnitConverted_UnitIsKelvin_ReturnKelvin()
            {
                var temperature = new TemperatureMeasurement();
                double testTempKelvin = 255.815;
                temperature.Value = 255.815;
                temperature.Unit = ConfigModel.TemperatureUnit.K;

                double resultKelvin = temperature.GetUnitConverted();
                Assert.AreEqual(testTempKelvin, resultKelvin, 1.0e-10);
            }

            [TestMethod]
            public void PressureMeasurement_GetAGA8Converted_UnitIsBarg_Return_kPa()
            {
                var pressure = new PressureMeasurement();
                double testPressurekPa = 11_271.325;
                pressure.Value = 111.7;
                pressure.Unit = ConfigModel.PressureUnit.barg;

                double resultBara = pressure.GetAGA8Converted();
                Assert.AreEqual(testPressurekPa, resultBara, 1.0e-10);
            }

            [TestMethod]
            public void PressureMeasurement_GetAGA8Converted_UnitIsBara_Return_kPa()
            {
                var pressure = new PressureMeasurement();
                double testPressurekPa = 15_775.64;
                pressure.Value = 157.756_4;
                pressure.Unit = ConfigModel.PressureUnit.bara;

                double resultBara = pressure.GetAGA8Converted();
                Assert.AreEqual(testPressurekPa, resultBara, 1.0e-10);
            }

            [TestMethod]
            public void PressureMeasurement_GetUnitConverted_UnitIsBarg_ReturnBarg()
            {
                var pressure = new PressureMeasurement();
                double testPressureBarg = 111.7;
                pressure.Value = 11_271.325;
                pressure.Unit = ConfigModel.PressureUnit.barg;

                double resultBarg = pressure.GetUnitConverted();
                Assert.AreEqual(testPressureBarg, resultBarg, 1.0e-10);
            }

            [TestMethod]
            public void PressureMeasurement_GetUnitConverted_UnitIsBara_ReturnBara()
            {
                var pressure = new PressureMeasurement();
                double testPressureBara = 157.756_4;
                pressure.Value = 15_775.64;
                pressure.Unit = ConfigModel.PressureUnit.bara;

                double resultBara = pressure.GetUnitConverted();
                Assert.AreEqual(testPressureBara, resultBara, 1.0e-10);
            }
        }
    }
}
