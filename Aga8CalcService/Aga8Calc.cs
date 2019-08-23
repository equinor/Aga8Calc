using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        public ConfigList configList = new ConfigList();
    }

    public class ConfigList
    {
        public ConfigList() { Items = new List<Config>(); }
        [XmlElement("config")]
        public List<Config> Items { get; set; }
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

        public double[] composition = (double[])Array.CreateInstance(typeof(double), 21);
        public string[] composition_tag = (string[])Array.CreateInstance(typeof(string), 21);
        [XmlIgnore]
        public double pressure;
        public string pressure_tag;
        [XmlIgnore]
        public double temperature;
        public string temperature_tag;
        public Aga8ResultCode calculation;
        [XmlIgnore]
        public double result;
        public string result_tag;

        public Config()
        { }
    }

    class Aga8Calc
    {
        [DllImport(@"aga8_2017.dll", EntryPoint = "aga8_2017")]
        public static extern double Aga8_2017(double[] composition, double pressure, double temperature, Config.Aga8ResultCode result);

        public static ConfigFile ReadConfig(string file)
        {
            try
            {
                string filename = file;
                FileStream configFileStream = new FileStream(filename, FileMode.Open);
                XmlSerializer configSerializer = new XmlSerializer(typeof(ConfigFile));
                ConfigFile result = (ConfigFile)configSerializer.Deserialize(configFileStream);
                configFileStream.Close();

                return result;
            }
            catch
            {
                return new ConfigFile();
            }
        }
    }
}
