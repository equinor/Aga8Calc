using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aga8CalcService;


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
            Assert.AreEqual(42.0, conf.Interval);
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
            Assert.AreEqual("24TI1234", conf.ConfigList.Item[0].TemperatureTag);
            Assert.AreEqual("24DI1234", conf.ConfigList.Item[0].ResultTag);
            Assert.AreEqual(Config.Aga8ResultCode.Density, conf.ConfigList.Item[0].Calculation);
        }
    }
}
