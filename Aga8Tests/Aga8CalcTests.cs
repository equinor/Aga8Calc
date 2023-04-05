using Aga8CalcService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static Aga8CalcService.ConfigModel;

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
                Interval = 1000,
                EquationOfState = ConfigModel.Equation.Gerg2008
            };

            config.ConfigList.Item.Add(new Config { Name = "GC name" });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Methane,         Tag = "ns=2;s=1:AI1001?A", Value = 0.778_240 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Nitrogen,        Tag = "ns=2;s=1:AI1001?A", Value = 0.020_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.CarbonDioxide,   Tag = "ns=2;s=1:AI1001?A", Value = 0.060_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Ethane,          Tag = "ns=2;s=1:AI1001?A", Value = 0.080_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Propane, Tag = "ns=2;s=1:AI1001?A", Value = 0.030_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.IsoButane, Tag = "ns=2;s=1:AI1001?A", Value = 0.001_500 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.NormalButane, Tag = "ns=2;s=1:AI1001?A", Value = 0.003_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.IsoPentane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_500 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.NormalPentane, Tag = "ns=2;s=1:AI1001?A", Value = 0.001_650 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Hexane, Tag = "ns=2;s=1:AI1001?A", Value = 0.002_150 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Heptane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_880 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Octane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_240 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Nonane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_150 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Decane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_090 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Hydrogen, Tag = "ns=2;s=1:AI1001?A", Value = 0.004_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Oxygen, Tag = "ns=2;s=1:AI1001?A", Value = 0.005_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.CarbonMonoxide, Tag = "ns=2;s=1:AI1001?A", Value = 0.002_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Water, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_100 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.HydrogenSulfide, Tag = "ns=2;s=1:AI1001?A", Value = 0.002_500 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Helium, Tag = "ns=2;s=1:AI1001?A", Value = 0.007_000 });
            config.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Argon, Tag = "ns=2;s=1:AI1001?A", Value = 0.001_000 });

            config.ConfigList.Item[0].PressureTemperatureList.Item.Add(new PressureTemperature
            {
                Name = "Point 1",
            });


            config.ConfigList.Item[0].PressureTemperatureList.Item[0].PressureFunction.Item.Add(new PressureMeasurement
            {
                Name = "PF 1",
                Tag = "ns=2;s=1:AI1001?Pressure",
            });

            config.ConfigList.Item[0].PressureTemperatureList.Item[0].PressureFunction.Item.Add(new PressureMeasurement
            {
                Name = "PF 2",
                Tag = "ns=2;s=1:AI1002?Pressure",
            });

            config.ConfigList.Item[0].PressureTemperatureList.Item[0].TemperatureFunction.Item.Add(new TemperatureMeasurement
            {
                Name = "PF 1",
                Tag = "ns=2;s=1:AI1001?Temperature",
            });

            config.ConfigList.Item[0].PressureTemperatureList.Item[0].TemperatureFunction.Item.Add(new TemperatureMeasurement
            {
                Name = "PF 2",
                Tag = "ns=2;s=1:AI1002?Temperature",
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
            ConfigModel.ReadConfig(file);
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
        public void Aga8_CalculatesProperties()
        {
            ConfigModel conf = new ConfigModel();

            conf.ConfigList.Item.Add(new Config());
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Methane, Tag = "ns=2;s=1:AI1001?A", Value = 0.778_240 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Nitrogen, Tag = "ns=2;s=1:AI1001?A", Value = 0.020_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.CarbonDioxide, Tag = "ns=2;s=1:AI1001?A", Value = 0.060_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Ethane, Tag = "ns=2;s=1:AI1001?A", Value = 0.080_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Propane, Tag = "ns=2;s=1:AI1001?A", Value = 0.030_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.IsoButane, Tag = "ns=2;s=1:AI1001?A", Value = 0.001_500 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.NormalButane, Tag = "ns=2;s=1:AI1001?A", Value = 0.003_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.IsoPentane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_500 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.NormalPentane, Tag = "ns=2;s=1:AI1001?A", Value = 0.001_650 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Hexane, Tag = "ns=2;s=1:AI1001?A", Value = 0.002_150 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Heptane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_880 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Octane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_240 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Nonane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_150 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Decane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_090 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Hydrogen, Tag = "ns=2;s=1:AI1001?A", Value = 0.004_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Oxygen, Tag = "ns=2;s=1:AI1001?A", Value = 0.005_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.CarbonMonoxide, Tag = "ns=2;s=1:AI1001?A", Value = 0.002_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Water, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_100 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.HydrogenSulfide, Tag = "ns=2;s=1:AI1001?A", Value = 0.002_500 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Helium, Tag = "ns=2;s=1:AI1001?A", Value = 0.007_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Argon, Tag = "ns=2;s=1:AI1001?A", Value = 0.001_000 });

            conf.ConfigList.Item[0].PressureTemperatureList.Item.Add(new PressureTemperature
            {
                Name = "Point 1",
            });

            conf.ConfigList.Item[0].PressureTemperatureList.Item[0].PressureFunction.Item.Add(new PressureMeasurement
            {
                Name = "PF 1",
                Tag = "ns=2;s=1:AI1001?Pressure",
                Value = 498.98675,
                Unit = ConfigModel.PressureUnit.barg
            });

            conf.ConfigList.Item[0].PressureTemperatureList.Item[0].TemperatureFunction.Item.Add(new TemperatureMeasurement
            {
                Name = "PF 1",
                Tag = "ns=2;s=1:AI1001?Temperature",
                Value = 126.85,
                Unit = ConfigModel.TemperatureUnit.C
            });

            conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item.Add(new PropertyMeasurement
            {
                Property = ConfigModel.Aga8ResultCode.MolarConcentration
            });

            var compositionError = new CompositionError();
            using (var aga = new AGA8Detail())
            {
                aga.SetComposition(conf.ConfigList.Item[0].Composition.GetScaledValues(), ref compositionError);
                aga.SetPressure(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].PressureFunction.GetValue());
                aga.SetTemperature(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].TemperatureFunction.GetValue());
                aga.CalculateDensity();
                aga.CalculateProperties();

                conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value = aga.GetProperty(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Property);
                Assert.AreEqual(12.807_924_036_488_01, conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
                Assert.AreEqual(12.807_924_036_488_01, aga.GetDensity(), 1e-9);

                conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value = aga.GetProperty(ConfigModel.Aga8ResultCode.Density);
                Assert.AreEqual(263.117_416_628_546, conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);

                Assert.AreEqual(12.807_924_036_488_01, aga.GetProperty(ConfigModel.Aga8ResultCode.MolarConcentration), 1.0e-9);
                Assert.AreEqual(20.543_330_51, aga.GetProperty(ConfigModel.Aga8ResultCode.MolarMass), 1.0e-9);
                Assert.AreEqual(1.173_801_364_147_326, aga.GetProperty(ConfigModel.Aga8ResultCode.CompressibilityFactor), 1.0e-9);
                //Assert.AreEqual(6_971.387_690_924_090, aga.GetProperty(ConfigModel.Aga8ResultCode.dp_dd), 1.0e-9);
                //Assert.AreEqual(1_118.803_636_639_520, aga.GetProperty(ConfigModel.Aga8ResultCode.d2p_dd2), 1.0e-9);
                //Assert.AreEqual(235.664_149_306_821_2, aga.GetProperty(ConfigModel.Aga8ResultCode.dp_dt), 1.0e-9);
                Assert.AreEqual(-2_739.134_175_817_231, aga.GetProperty(ConfigModel.Aga8ResultCode.InternalEnergy), 1.0e-9);
                Assert.AreEqual(1_164.699_096_269_404, aga.GetProperty(ConfigModel.Aga8ResultCode.Enthalpy), 1.0e-9);
                Assert.AreEqual(-38.548_826_846_771_11, aga.GetProperty(ConfigModel.Aga8ResultCode.Entropy), 1.0e-9);
                Assert.AreEqual(39.120_761_544_303_32, aga.GetProperty(ConfigModel.Aga8ResultCode.IsochoricHeatCapacity), 1.0e-9);
                Assert.AreEqual(58.546_176_723_806_67, aga.GetProperty(ConfigModel.Aga8ResultCode.IsobaricHeatCapacity), 1.0e-9);
                Assert.AreEqual(712.639_368_405_790_3, aga.GetProperty(ConfigModel.Aga8ResultCode.SpeedOfSound), 1.0e-9);
                Assert.AreEqual(16_584.229_834_977_85, aga.GetProperty(ConfigModel.Aga8ResultCode.GibbsEnergy), 1.0e-9);
                Assert.AreEqual(7.432_969_304_794_577E-5, aga.GetProperty(ConfigModel.Aga8ResultCode.JouleThomsonCoefficient), 1.0e-9);
                Assert.AreEqual(2.672_509_225_184_606, aga.GetProperty(ConfigModel.Aga8ResultCode.IsentropicExponent), 1.0e-9);
            }

            using (var gerg = new Gerg2008())
            {
                gerg.SetComposition(conf.ConfigList.Item[0].Composition.GetScaledValues(), ref compositionError);
                gerg.SetPressure(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].PressureFunction.GetValue());
                gerg.SetTemperature(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].TemperatureFunction.GetValue());
                gerg.CalculateDensity();
                gerg.CalculateProperties();

                conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value = gerg.GetProperty(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Property);
                Assert.AreEqual(12.798_286_260_820_6, conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
                Assert.AreEqual(12.798_286_260_820_6, gerg.GetDensity(), 1e-9);

                conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value = gerg.GetProperty(ConfigModel.Aga8ResultCode.Density);
                Assert.AreEqual(262.911_924_714_376, conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);

                Assert.AreEqual(12.798_286_260_820_62, gerg.GetProperty(ConfigModel.Aga8ResultCode.MolarConcentration), 1.0e-9);
                Assert.AreEqual(20.542_744_501_6, gerg.GetProperty(ConfigModel.Aga8ResultCode.MolarMass), 1.0e-9);
                Assert.AreEqual(1.174_690_666_383_717, gerg.GetProperty(ConfigModel.Aga8ResultCode.CompressibilityFactor), 1.0e-9);
                //Assert.AreEqual(7_000.694_030_193_327, gerg.GetProperty(ConfigModel.Aga8ResultCode.dp_dd), 1.0e-9);
                //Assert.AreEqual(1_129.526_655_214_841, gerg.GetProperty(ConfigModel.Aga8ResultCode.d2p_dd2), 1.0e-9);
                //Assert.AreEqual(235.983_229_259_309_6, gerg.GetProperty(ConfigModel.Aga8ResultCode.dp_dt), 1.0e-9);
                Assert.AreEqual(-2_746.492_901_212_53, gerg.GetProperty(ConfigModel.Aga8ResultCode.InternalEnergy), 1.0e-9);
                Assert.AreEqual(1_160.280_160_510_973, gerg.GetProperty(ConfigModel.Aga8ResultCode.Enthalpy), 1.0e-9);
                Assert.AreEqual(-38.575_903_924_090_89, gerg.GetProperty(ConfigModel.Aga8ResultCode.Entropy), 1.0e-9);
                Assert.AreEqual(39.029_482_181_563_72, gerg.GetProperty(ConfigModel.Aga8ResultCode.IsochoricHeatCapacity), 1.0e-9);
                Assert.AreEqual(58.455_220_510_003_66, gerg.GetProperty(ConfigModel.Aga8ResultCode.IsobaricHeatCapacity), 1.0e-9);
                Assert.AreEqual(714.424_884_059_602_4, gerg.GetProperty(ConfigModel.Aga8ResultCode.SpeedOfSound), 1.0e-9);
                Assert.AreEqual(16_590.641_730_147_33, gerg.GetProperty(ConfigModel.Aga8ResultCode.GibbsEnergy), 1.0e-9);
                Assert.AreEqual(7.155_629_581_480_913E-5, gerg.GetProperty(ConfigModel.Aga8ResultCode.JouleThomsonCoefficient), 1.0e-9);
                Assert.AreEqual(2.683_820_255_058_032, gerg.GetProperty(ConfigModel.Aga8ResultCode.IsentropicExponent), 1.0e-9);
            }

            conf.ConfigList.Item.Add(new Config());
            conf.ConfigList.Item[1].Composition.Item.Add(new Component { Name = Aga8Component.Methane, Tag = "ns=2;s=1:AI1001?A", Value = 96.5, ScaleFactor = 0.01 });
            conf.ConfigList.Item[1].Composition.Item.Add(new Component { Name = Aga8Component.Nitrogen, Tag = "ns=2;s=1:AI1001?A", Value = 0.3, ScaleFactor = 0.01 });
            conf.ConfigList.Item[1].Composition.Item.Add(new Component { Name = Aga8Component.CarbonDioxide, Tag = "ns=2;s=1:AI1001?A", Value = 0.6, ScaleFactor = 0.01 });
            conf.ConfigList.Item[1].Composition.Item.Add(new Component { Name = Aga8Component.Ethane, Tag = "ns=2;s=1:AI1001?A", Value = 1.8, ScaleFactor = 0.01 });
            conf.ConfigList.Item[1].Composition.Item.Add(new Component { Name = Aga8Component.Propane, Tag = "ns=2;s=1:AI1001?A", Value = 0.45, ScaleFactor = 0.01 });
            conf.ConfigList.Item[1].Composition.Item.Add(new Component { Name = Aga8Component.IsoButane, Tag = "ns=2;s=1:AI1001?A", Value = 0.1, ScaleFactor = 0.01 });
            conf.ConfigList.Item[1].Composition.Item.Add(new Component { Name = Aga8Component.NormalButane, Tag = "ns=2;s=1:AI1001?A", Value = 0.1, ScaleFactor = 0.01 });
            conf.ConfigList.Item[1].Composition.Item.Add(new Component { Name = Aga8Component.IsoPentane, Tag = "ns=2;s=1:AI1001?A", Value = 0.05, ScaleFactor = 0.01 });
            conf.ConfigList.Item[1].Composition.Item.Add(new Component { Name = Aga8Component.NormalPentane, Tag = "ns=2;s=1:AI1001?A", Value = 0.03, ScaleFactor = 0.01 });
            conf.ConfigList.Item[1].Composition.Item.Add(new Component { Name = Aga8Component.Hexane, Tag = "ns=2;s=1:AI1001?A", Value = 0.07, ScaleFactor = 0.01 });

            conf.ConfigList.Item[1].PressureTemperatureList.Item.Add(new PressureTemperature
            {
                Name = "Point 1",
            });

            conf.ConfigList.Item[1].PressureTemperatureList.Item[0].PressureFunction.Item.Add(new PressureMeasurement
            {
                Name = "PF 1",
                Tag = "ns=2;s=1:AI1001?Pressure",
                Value = 145,
                Unit = ConfigModel.PressureUnit.barg
            });

            conf.ConfigList.Item[1].PressureTemperatureList.Item[0].TemperatureFunction.Item.Add(new TemperatureMeasurement
            {
                Name = "PF 1",
                Tag = "ns=2;s=1:AI1001?Temperature",
                Value = 18,
                Unit = ConfigModel.TemperatureUnit.C
            });

            conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item.Add(new PropertyMeasurement
            {
                Property = ConfigModel.Aga8ResultCode.MolarConcentration
            });

            using (var aga = new AGA8Detail())
            {
                aga.SetComposition(conf.ConfigList.Item[1].Composition.GetScaledValues(), ref compositionError);
                aga.SetPressure(conf.ConfigList.Item[1].PressureTemperatureList.Item[0].PressureFunction.GetValue());
                aga.SetTemperature(conf.ConfigList.Item[1].PressureTemperatureList.Item[0].TemperatureFunction.GetValue());
                aga.CalculateDensity();
                aga.CalculateProperties();

                conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item[0].Value = aga.GetProperty(conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item[0].Property);
                Assert.AreEqual(7.731_358_744_220_41, conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
                Assert.AreEqual(7.731_358_744_220_41, aga.GetDensity(), 1e-9);

                conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item[0].Value = aga.GetProperty(ConfigModel.Aga8ResultCode.Density);
                Assert.AreEqual(129.914_519_856_789, conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
            }

            using (var gerg = new Gerg2008())
            {
                gerg.SetComposition(conf.ConfigList.Item[1].Composition.GetScaledValues(), ref compositionError);
                gerg.SetPressure(conf.ConfigList.Item[1].PressureTemperatureList.Item[0].PressureFunction.GetValue());
                gerg.SetTemperature(conf.ConfigList.Item[1].PressureTemperatureList.Item[0].TemperatureFunction.GetValue());
                gerg.CalculateDensity();
                gerg.CalculateProperties();

                conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item[0].Value = gerg.GetProperty(conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item[0].Property);
                Assert.AreEqual(7.730_483_295_277_39, conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
                Assert.AreEqual(7.730_483_295_277_39, gerg.GetDensity(), 1e-9);

                conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item[0].Value = gerg.GetProperty(ConfigModel.Aga8ResultCode.Density);
                Assert.AreEqual(129.895_544_935_963, conf.ConfigList.Item[1].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
            }
        }

        [TestMethod]
        public void Aga8_Test_Memory()
        {
            ConfigModel conf = new ConfigModel();

            conf.ConfigList.Item.Add(new Config());
            conf.ConfigList.Item[0].Name = "Test config";
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Methane, Tag = "ns=2;s=1:AI1001?A", Value = 0.778_240 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Nitrogen, Tag = "ns=2;s=1:AI1001?A", Value = 0.020_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.CarbonDioxide, Tag = "ns=2;s=1:AI1001?A", Value = 0.060_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Ethane, Tag = "ns=2;s=1:AI1001?A", Value = 0.080_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Propane, Tag = "ns=2;s=1:AI1001?A", Value = 0.030_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.IsoButane, Tag = "ns=2;s=1:AI1001?A", Value = 0.001_500 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.NormalButane, Tag = "ns=2;s=1:AI1001?A", Value = 0.003_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.IsoPentane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_500 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.NormalPentane, Tag = "ns=2;s=1:AI1001?A", Value = 0.001_650 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Hexane, Tag = "ns=2;s=1:AI1001?A", Value = 0.002_150 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Heptane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_880 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Octane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_240 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Nonane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_150 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Decane, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_090 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Hydrogen, Tag = "ns=2;s=1:AI1001?A", Value = 0.004_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Oxygen, Tag = "ns=2;s=1:AI1001?A", Value = 0.005_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.CarbonMonoxide, Tag = "ns=2;s=1:AI1001?A", Value = 0.002_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Water, Tag = "ns=2;s=1:AI1001?A", Value = 0.000_100 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.HydrogenSulfide, Tag = "ns=2;s=1:AI1001?A", Value = 0.002_500 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Helium, Tag = "ns=2;s=1:AI1001?A", Value = 0.007_000 });
            conf.ConfigList.Item[0].Composition.Item.Add(new Component { Name = Aga8Component.Argon, Tag = "ns=2;s=1:AI1001?A", Value = 0.001_000 });

            conf.ConfigList.Item[0].PressureTemperatureList.Item.Add(new PressureTemperature
            {
                Name = "Point 1",
            });

            conf.ConfigList.Item[0].PressureTemperatureList.Item[0].PressureFunction.Item.Add(new PressureMeasurement
            {
                Name = "PF 1",
                Tag = "ns=2;s=1:AI1001?Pressure",
                Value = 498.98675,
                Unit = ConfigModel.PressureUnit.barg
            });

            conf.ConfigList.Item[0].PressureTemperatureList.Item[0].TemperatureFunction.Item.Add(new TemperatureMeasurement
            {
                Name = "PF 1",
                Tag = "ns=2;s=1:AI1001?Temperature",
                Value = 126.85,
                Unit = ConfigModel.TemperatureUnit.C
            });

            conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item.Add(new PropertyMeasurement
            {
                Property = ConfigModel.Aga8ResultCode.MolarConcentration
            });

            var compositionError = new CompositionError();
            for (int i = 0; i < 1000; i++)
            {
                using (var aga = new AGA8Detail())
                {
                    aga.SetComposition(conf.ConfigList.Item[0].Composition.GetScaledValues(), ref compositionError);
                    aga.SetPressure(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].PressureFunction.GetValue());
                    aga.SetTemperature(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].TemperatureFunction.GetValue());
                    aga.CalculateDensity();
                    aga.CalculateProperties();

                    conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value = aga.GetProperty(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Property);
                    Assert.AreEqual(12.807_924_036_488_01, conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
                    Assert.AreEqual(12.807_924_036_488_01, aga.GetDensity(), 1e-9);

                    conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value = aga.GetProperty(ConfigModel.Aga8ResultCode.Density);
                    Assert.AreEqual(263.117_416_628_546, conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
                }

                using (var gerg = new Gerg2008())
                {
                    gerg.SetComposition(conf.ConfigList.Item[0].Composition.GetScaledValues(),ref compositionError);
                    gerg.SetPressure(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].PressureFunction.GetValue());
                    gerg.SetTemperature(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].TemperatureFunction.GetValue());
                    gerg.CalculateDensity();
                    gerg.CalculateProperties();

                    conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value = gerg.GetProperty(conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Property);
                    Assert.AreEqual(12.798_286_260_820_6, conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
                    Assert.AreEqual(12.798_286_260_820_6, gerg.GetDensity(), 1e-9);

                    conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value = gerg.GetProperty(ConfigModel.Aga8ResultCode.Density);
                    Assert.AreEqual(262.911_924_714_376, conf.ConfigList.Item[0].PressureTemperatureList.Item[0].Properties.Item[0].Value, 1e-9);
                }
            }
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
            public void ConfigModel_ReadConfig()
            {
                string TagConfFile = AppDomain.CurrentDomain.BaseDirectory.ToString(CultureInfo.InvariantCulture) + "\\Tag_Config_Test.xml";
                ConfigModel conf = ConfigModel.ReadConfig(TagConfFile);
            }

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

            [TestMethod]
            public void PressureFunction_GetValue_Min()
            {
                var pressure = new PressureFunction { MathFunction = ConfigModel.Func.Min };
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 3.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 4.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 5.14
                });

                Assert.AreEqual(415.325, pressure.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void PressureFunction_GetValue_Max()
            {
                var pressure = new PressureFunction { MathFunction = ConfigModel.Func.Max };
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 3.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 4.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 5.14
                });

                Assert.AreEqual(615.325, pressure.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void PressureFunction_GetValue_Average()
            {
                var pressure = new PressureFunction { MathFunction = ConfigModel.Func.Average };
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 3.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 4.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 5.14
                });

                Assert.AreEqual(515.325, pressure.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void PressureFunction_GetValue_Median_Even()
            {
                var pressure = new PressureFunction { MathFunction = ConfigModel.Func.Median };
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 3.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 4.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 5.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 6.14
                });

                Assert.AreEqual(565.325, pressure.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void PressureFunction_GetValue_Median_Odd()
            {
                var pressure = new PressureFunction { MathFunction = ConfigModel.Func.Median };
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 3.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 4.14
                });
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 6.14
                });

                Assert.AreEqual(515.325, pressure.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void PressureFunction_GetValue_Median_Single()
            {
                var pressure = new PressureFunction { MathFunction = ConfigModel.Func.Median };
                pressure.Item.Add(new PressureMeasurement
                {
                    Unit = ConfigModel.PressureUnit.barg,
                    Value = 3.14
                });

                Assert.AreEqual(415.325, pressure.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void TemperatureFunction_GetValue_Min()
            {
                var temperature = new TemperatureFunction { MathFunction = ConfigModel.Func.Min };
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 3.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 4.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 5.14
                });

                Assert.AreEqual(3.14, temperature.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void TemperatureFunction_GetValue_Max()
            {
                var temperature = new TemperatureFunction { MathFunction = ConfigModel.Func.Max };
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 3.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 4.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 5.14
                });

                Assert.AreEqual(5.14, temperature.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void TemperatureFunction_GetValue_Average()
            {
                var temperature = new TemperatureFunction { MathFunction = ConfigModel.Func.Average };
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 3.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 4.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 5.14
                });

                Assert.AreEqual(4.14, temperature.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void TemperatureFunction_GetValue_Median_Even()
            {
                var temperature = new TemperatureFunction { MathFunction = ConfigModel.Func.Median };
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 3.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 4.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 5.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 7.14
                });

                Assert.AreEqual(4.64, temperature.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void TemperatureFunction_GetValue_Median_Odd()
            {
                var temperature = new TemperatureFunction { MathFunction = ConfigModel.Func.Median };
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 3.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 4.14
                });
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 5.14
                });

                Assert.AreEqual(4.14, temperature.GetValue(), 1.0e-10);
            }

            [TestMethod]
            public void TemperatureFunction_GetValue_Median_Single()
            {
                var temperature = new TemperatureFunction { MathFunction = ConfigModel.Func.Median };
                temperature.Item.Add(new TemperatureMeasurement
                {
                    Unit = ConfigModel.TemperatureUnit.K,
                    Value = 3.14
                });

                Assert.AreEqual(3.14, temperature.GetValue(), 1.0e-10);
            }
        }
    }
}
