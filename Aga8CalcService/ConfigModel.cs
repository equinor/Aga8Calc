using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;

namespace Aga8CalcService
{
    [XmlRoot("configuration")]
    public class ConfigModel
    {
        public enum Aga8ResultCode : Int32
        {
            MolarConcentration = 0,
            MolarMass = 1,
            CompressibilityFactor = 2,
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

        public enum PressureUnit : int
        {
            barg = 0,
            bara = 1,
        }

        public enum TemperatureUnit : int
        {
            C = 0,
            K = 1,
        }


        [XmlElement]
        public string OpcUrl { get; set; }
        [XmlElement]
        public string OpcUser { get; set; }
        [XmlElement]
        public string OpcPassword { get; set; }

        [XmlElement]
        public double Interval { get; set; }

        [XmlElement]
        public ConfigList ConfigList { get; set; } = new ConfigList();

        public static ConfigModel ReadConfig(string file)
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            };

            XmlReader configFileReader = XmlReader.Create(file, readerSettings);
            XmlSerializer configSerializer = new XmlSerializer(typeof(ConfigModel));
            ConfigModel result = (ConfigModel)configSerializer.Deserialize(configFileReader);
            configFileReader.Close();

            return result;
        }
    }

    public class ConfigList
    {
        public ConfigList() { Item = new List<Config>(); }
        [XmlElement("Config")]
        public List<Config> Item { get; }
    }

    public class Config
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlElement]
        public CompositionList Composition { get; set; } = new CompositionList();

        [XmlElement]
        public PTList PressureTemperatureList { get; set; } = new PTList();

        public Config()
        { }
    }

    public class CompositionList
    {
        public CompositionList() { Item = new List<Component>(); }
        [XmlElement("Component")]
        public List<Component> Item { get; }

        public double[] GetValues()
        {
            List<double> vs = new List<double>();

            foreach (var component in Item)
            {
                vs.Add(component.Value);
            }

            return vs.ToArray();
        }

        public double[] GetScaledValues()
        {
            List<double> vs = new List<double>();

            foreach (var component in Item)
            {
                vs.Add(component.GetScaledValue());
            }

            return vs.ToArray();
        }
    }

    public class PTList
    {
        public PTList() { Item = new List<PressureTemperature>(); }
        [XmlElement("PressureTemperature")]
        public List<PressureTemperature> Item { get; }
    }

    public enum Func : int
    {
        Min = 0,
        Max = 1,
        Average = 2,
        Median = 3
    }

    public class PressureFunction
    {
        public PressureFunction() { Item = new List<PressureMeasurement>(); }
        [XmlElement("Pressure")]
        public List<PressureMeasurement> Item { get; }

        [XmlAttribute]
        public Func MathFunction { get; set; }

        public double GetValue()
        {
            double value = 0.0;
            switch (MathFunction)
            {
                case Func.Max:
                    value = double.MinValue;
                    foreach (var it in Item)
                    {
                        if (it.GetAGA8Converted() > value)
                        { 
                            value = it.GetAGA8Converted();
                        }
                    }
                    break;
                case Func.Min:
                    value = double.MaxValue;
                    foreach (var it in Item)
                    {
                        if (it.GetAGA8Converted() < value)
                        {
                            value = it.GetAGA8Converted();
                        }
                    }
                    break;
                case Func.Average:
                    value = 0.0;
                    foreach (var it in Item)
                    {
                        value += it.GetAGA8Converted();
                    }
                    value /= (double)Item.Count;
                    break;
                case Func.Median:
                    value = 0.0;
                    List<double> v = new List<double>();
                    foreach (var it in Item)
                    {
                        v.Add(it.GetAGA8Converted());
                    }
                    v.Sort();

                    if (v.Count == 1)
                    {
                        value = v[0];
                    }
                    else if (v.Count % 2 == 0)
                    {
                        // An even number of values
                        value += v[v.Count / 2 - 1];
                        value += v[v.Count / 2];
                        value /= 2.0;
                    }
                    else
                    {
                        // An odd number of values
                        value = v[v.Count / 2];
                    }
                    break;
                default:
                    break;
            }

            return value;
        }
    }

    public class TemperatureFunction
    {
        public TemperatureFunction() { Item = new List<TemperatureMeasurement>(); }
        [XmlElement("Temperature")]
        public List<TemperatureMeasurement> Item { get; }

        [XmlAttribute]
        public Func MathFunction { get; set; }

        public double GetValue()
        {
            double value = 0.0;
            switch (MathFunction)
            {
                case Func.Max:
                    value = double.MinValue;
                    foreach (var it in Item)
                    {
                        if (it.GetAGA8Converted() > value)
                        {
                            value = it.GetAGA8Converted();
                        }
                    }
                    break;
                case Func.Min:
                    value = double.MaxValue;
                    foreach (var it in Item)
                    {
                        if (it.GetAGA8Converted() < value)
                        {
                            value = it.GetAGA8Converted();
                        }
                    }
                    break;
                case Func.Average:
                    value = 0.0;
                    foreach (var it in Item)
                    {
                        value += it.GetAGA8Converted();
                    }
                    value /= (double)Item.Count;
                    break;
                case Func.Median:
                    value = 0.0;
                    List<double> v = new List<double>();
                    foreach (var it in Item)
                    {
                        v.Add(it.GetAGA8Converted());
                    }
                    v.Sort();

                    if (v.Count == 1)
                    {
                        value = v[0];
                    }
                    else if (v.Count % 2 == 0)
                    {
                        // An even number of values
                        value += v[v.Count / 2 - 1];
                        value += v[v.Count / 2];
                        value /= 2.0;
                    }
                    else
                    {
                        // An odd number of values
                        value = v[v.Count / 2];
                    }
                    break;
                default:
                    break;
            }

            return value;
        }
    }


    public class PressureTemperature
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement]
        public PressureMeasurement Pressure { get; set; } = new PressureMeasurement();
        [XmlElement]
        public TemperatureMeasurement Temperature { get; set; } = new TemperatureMeasurement();

        [XmlElement]
        public PressureFunction PressureFunction { get; set; } = new PressureFunction();

        [XmlElement]
        public TemperatureFunction TemperatureFunction { get; set; } = new TemperatureFunction();

        [XmlElement]
        public PropertyList Properties { get; set; } = new PropertyList();

        public object GetTemperature()
        {
            if (Temperature.Type == "single")
            {
                return Convert.ToSingle(Temperature.GetUnitConverted());
            }
            else if (Temperature.Type == "double")
            {
                return Convert.ToDouble(Temperature.GetUnitConverted());
            }
            else
            {
                return Convert.ToDouble(Temperature.GetUnitConverted());
            }
        }

        public object GetPressure()
        {
            if (Pressure.Type == "single")
            {
                return Convert.ToSingle(Pressure.GetUnitConverted());
            }
            else if (Pressure.Type == "double")
            {
                return Convert.ToDouble(Pressure.GetUnitConverted());
            }
            else
            {
                return Convert.ToDouble(Pressure.GetUnitConverted());
            }
        }
    }

    public class PropertyList
    {
        public PropertyList() { Item = new List<PropertyMeasurement>(); }
        [XmlElement("Property")]
        public List<PropertyMeasurement> Item { get; }
    }

    public class Component
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Tag { get; set; }
        [XmlAttribute]
        public double ScaleFactor { get; set; }
        [XmlIgnore]
        public double Value { get; set; }

        public double GetScaledValue()
        {
            return Value * ScaleFactor;
        }
    }

    public class Measurement
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Tag { get; set; }
        [XmlAttribute]
        public string Type { get; set; }

        [XmlIgnore]
        public double Value { get; set; }
    }

    public class PressureMeasurement : Measurement
    {
        [XmlAttribute]
        public ConfigModel.PressureUnit Unit { get; set; }

        public double GetAGA8Converted()
        {
            // Convert from Unit to kPa absolute
            const double stdAtm = 1.01325;
            double result = 0.0;
            switch (Unit)
            {
                case ConfigModel.PressureUnit.barg:
                    result = (Value + stdAtm) * 100.0;
                    break;
                case ConfigModel.PressureUnit.bara:
                    result = Value * 100.0;
                    break;
                default:
                    break;
            }

            return (result);
        }

        public double GetUnitConverted()
        {
            // Convert from kPa absolute to Unit
            const double stdAtm = 1.01325;
            double result = 0.0;
            switch (Unit)
            {
                case ConfigModel.PressureUnit.barg:
                    result = Value / 100.0 - stdAtm;
                    break;
                case ConfigModel.PressureUnit.bara:
                    result = Value / 100.0;
                    break;
                default:
                    break;
            }

            return (result);
        }
    }

    public class TemperatureMeasurement : Measurement
    {
        [XmlAttribute]
        public ConfigModel.TemperatureUnit Unit { get; set; }

        public double GetAGA8Converted()
        {
            // Convert from Unit to K
            const double zeroCelsius = 273.15;
            double result = 0.0;
            switch (Unit)
            {
                case ConfigModel.TemperatureUnit.C:
                    result = Value + zeroCelsius;
                    break;
                case ConfigModel.TemperatureUnit.K:
                    result = Value;
                    break;
                default:
                    break;
            }

            return (result);
        }

        public double GetUnitConverted()
        {
            // Convert from K to Unit
            const double zeroCelsius = 273.15;
            double result = 0.0;
            switch (Unit)
            {
                case ConfigModel.TemperatureUnit.C:
                    result = Value - zeroCelsius;
                    break;
                case ConfigModel.TemperatureUnit.K:
                    result = Value;
                    break;
                default:
                    break;
            }

            return (result);
        }
    }

    public class PropertyMeasurement : Measurement
    {
        [XmlAttribute]
        public ConfigModel.Aga8ResultCode Property { get; set; }

        public object GetTypedValue()
        {
            if (Type == "single")
            {
                return Convert.ToSingle(Value);
            }
            else if (Type == "double")
            {
                return Convert.ToDouble(Value);
            }
            else
            {
                return Convert.ToDouble(Value);
            }

        }
    }

    public class TimeStampedMeasurement : Measurement
    {
        [XmlIgnore]
        public DateTime TimeStamp { get; set; }
    }
}
