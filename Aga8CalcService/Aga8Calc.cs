using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;

namespace Aga8CalcService
{
    [XmlRoot("configuration")]
    public class ConfigFile
    {
        [XmlElement("opc_url")]
        public string OpcUrl { get; set; }
        [XmlElement("opc_user")]
        public string OpcUser { get; set; }
        [XmlElement("opc_password")]
        public string OpcPassword { get; set; }

        [XmlElement("interval")]
        public double Interval { get; set; }

        [XmlElement("config_list")]
        public ConfigList ConfigList = new ConfigList();

        public static ConfigFile ReadConfig(string file)
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            };

            XmlReader configFileReader = XmlReader.Create(file, readerSettings);
            XmlSerializer configSerializer = new XmlSerializer(typeof(ConfigFile));
            ConfigFile result = (ConfigFile)configSerializer.Deserialize(configFileReader);
            configFileReader.Close();

            return result;
        }
    }

    public class ConfigList
    {
        public ConfigList() { Item = new List<Config>(); }
        [XmlElement("config")]
        public List<Config> Item { get; }
    }

    [XmlType("config")]
    public class Config
    {
        public enum Aga8ResultCode : Int32
        {
            MolarConcentration = 0,
            CompressibilityFactor = 1,
            MolarMass = 2,
            InternalEnergy = 6,
            Enthalpy = 7,
            Entropy = 8,
            IsochoricHeatCapacity = 9,
            IsobaricHeatCapacity = 10,
            SpeedOfSound = 11,
            GibbsEnergy = 12,
            JouleThomsonCoefficient = 13,
            IsentropicExponent = 14,
            Density = 15
        }

        private double[] Composition = (double[])Array.CreateInstance(typeof(double), 21);

        [XmlArray("composition_tag")]
        public string[] CompositionTag { get; set; } = (string[])Array.CreateInstance(typeof(string), 21);

        [XmlIgnore]
        public double Pressure { get; set; }
        [XmlIgnore]
        public double Temperature { get; set; }
        [XmlIgnore]
        public double Result { get; set; }

        public double[] GetComposition()
        {
            return Composition;
        }

        public void SetComposition(double[] value)
        {
            Composition = value;
        }

        [XmlElement("result_tag")]
        public string ResultTag { get; set; }
        [XmlElement("pressure_tag")]
        public string PressureTag { get; set; }
        [XmlElement("temperature_tag")]
        public string TemperatureTag { get; set; }
        [XmlElement("calculation")]
        public Aga8ResultCode Calculation { get; set; }

        public Config()
        { }
    }

    public static class NativeMethods
    {
        [DllImport(@"aga8_2017.dll", EntryPoint = "aga8_2017")]
        public static extern double Aga8(double[] composition, double pressure, double temperature, Config.Aga8ResultCode result);
    }
}
