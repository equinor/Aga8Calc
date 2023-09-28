using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using static Aga8CalcService.ConfigModel;



namespace Aga8CalcService
{
    [XmlRoot("configuration")]
    public class ConfigModel
    {
        public enum Aga8Component
        {
            Methane,
            Nitrogen,
            CarbonDioxide,
            Ethane,
            Propane,
            IsoButane,
            NormalButane,
            IsoPentane,
            NormalPentane,
            Hexane,
            Heptane,
            Octane,
            Nonane,
            Decane,
            Hydrogen,
            Oxygen,
            CarbonMonoxide,
            Water,
            HydrogenSulfide,
            Helium,
            Argon
        }

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

        public enum Func : int
        {
            Min = 0,
            Max = 1,
            Average = 2,
            Median = 3
        }

        public enum Equation : int
        {
            AGA8Detail = 0,
            Gerg2008 = 1
        }


        [XmlElement]
        public string OpcUrl { get; set; }
        [XmlElement]
        public string OpcUser { get; set; }
        [XmlElement]
        public string OpcPassword { get; set; }

        [XmlElement]
        public string DefaultNamespaceURI { get; set; }

        [XmlElement]
        public double Interval { get; set; }

        [XmlElement]
        public Equation EquationOfState { get; set; }

        [XmlElement]
        public ConfigList ConfigList { get; set; } = new ConfigList();

        public static ConfigModel ReadConfig(string file)
        {
            XmlReaderSettings readerSettings = new()
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true
            };

            XmlReader configFileReader = XmlReader.Create(file, readerSettings);
            XmlSerializer configSerializer = new(typeof(ConfigModel));
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
        [XmlAttribute]
        public int SamplingInterval { get; set; } = 180000;
        [XmlAttribute]
        public bool Normalize { get; set; }
        [XmlElement("Component")]
        public List<Component> Item { get; }

        public StatusCode Quality { get; set; }

        public Aga8Composition GetScaledValues()
        {
            Aga8Composition comp = new();

            foreach (var component in Item)
            {
                switch (component.Name)
                {
                    case Aga8Component.Methane:
                        comp.Methane = component.GetScaledValue();
                        break;
                    case Aga8Component.Nitrogen:
                        comp.Nitrogen = component.GetScaledValue();
                        break;
                    case Aga8Component.CarbonDioxide:
                        comp.CarbonDioxide = component.GetScaledValue();
                        break;
                    case Aga8Component.Ethane:
                        comp.Ethane = component.GetScaledValue();
                        break;
                    case Aga8Component.Propane:
                        comp.Propane = component.GetScaledValue();
                        break;
                    case Aga8Component.IsoButane:
                        comp.IsoButane = component.GetScaledValue();
                        break;
                    case Aga8Component.NormalButane:
                        comp.NormalButane = component.GetScaledValue();
                        break;
                    case Aga8Component.IsoPentane:
                        comp.IsoPentane = component.GetScaledValue();
                        break;
                    case Aga8Component.NormalPentane:
                        comp.NormalPentane = component.GetScaledValue();
                        break;
                    case Aga8Component.Hexane:
                        comp.Hexane = component.GetScaledValue();
                        break;
                    case Aga8Component.Heptane:
                        comp.Heptane = component.GetScaledValue();
                        break;
                    case Aga8Component.Octane:
                        comp.Octane = component.GetScaledValue();
                        break;
                    case Aga8Component.Nonane:
                        comp.Nonane = component.GetScaledValue();
                        break;
                    case Aga8Component.Decane:
                        comp.Decane = component.GetScaledValue();
                        break;
                    case Aga8Component.Hydrogen:
                        comp.Hydrogen = component.GetScaledValue();
                        break;
                    case Aga8Component.Oxygen:
                        comp.Oxygen = component.GetScaledValue();
                        break;
                    case Aga8Component.CarbonMonoxide:
                        comp.CarbonMonoxide = component.GetScaledValue();
                        break;
                    case Aga8Component.Water:
                        comp.Water = component.GetScaledValue();
                        break;
                    case Aga8Component.HydrogenSulfide:
                        comp.HydrogenSulfide = component.GetScaledValue();
                        break;
                    case Aga8Component.Helium:
                        comp.Helium = component.GetScaledValue();
                        break;
                    case Aga8Component.Argon:
                        comp.Argon = component.GetScaledValue();
                        break;
                }
            }

            return comp;
        }
    }

    public class PTList
    {
        public PTList() { Item = new List<PressureTemperature>(); }
        [XmlElement("PressureTemperature")]
        public List<PressureTemperature> Item { get; }
    }

    public class PressureFunction
    {
        public PressureFunction() { Item = new List<PressureMeasurement>(); }
        [XmlElement("Pressure")]
        public List<PressureMeasurement> Item { get; }

        [XmlAttribute]
        public ConfigModel.Func MathFunction { get; set; }

        public StatusCode Quality { get; set; }

        public double GetValue()
        {
            double value = 0.0;
            switch (MathFunction)
            {
                case ConfigModel.Func.Max:
                    value = double.MinValue;
                    foreach (var it in Item)
                    {
                        if (it.GetAGA8Converted() > value)
                        {
                            value = it.GetAGA8Converted();
                        }
                    }
                    break;
                case ConfigModel.Func.Min:
                    value = double.MaxValue;
                    foreach (var it in Item)
                    {
                        if (it.GetAGA8Converted() < value)
                        {
                            value = it.GetAGA8Converted();
                        }
                    }
                    break;
                case ConfigModel.Func.Average:
                    value = 0.0;
                    foreach (var it in Item)
                    {
                        value += it.GetAGA8Converted();
                    }
                    value /= (double)Item.Count;
                    break;
                case ConfigModel.Func.Median:
                    value = 0.0;
                    List<double> v = new();
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
        public ConfigModel.Func MathFunction { get; set; }

        public StatusCode Quality { get; set; }

        public double GetValue()
        {
            double value = 0.0;
            switch (MathFunction)
            {
                case ConfigModel.Func.Max:
                    value = double.MinValue;
                    foreach (var it in Item)
                    {
                        if (it.GetAGA8Converted() > value)
                        {
                            value = it.GetAGA8Converted();
                        }
                    }
                    break;
                case ConfigModel.Func.Min:
                    value = double.MaxValue;
                    foreach (var it in Item)
                    {
                        if (it.GetAGA8Converted() < value)
                        {
                            value = it.GetAGA8Converted();
                        }
                    }
                    break;
                case ConfigModel.Func.Average:
                    value = 0.0;
                    foreach (var it in Item)
                    {
                        value += it.GetAGA8Converted();
                    }
                    value /= (double)Item.Count;
                    break;
                case ConfigModel.Func.Median:
                    value = 0.0;
                    List<double> v = new();
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
        public PressureFunction PressureFunction { get; set; } = new PressureFunction();

        [XmlElement]
        public TemperatureFunction TemperatureFunction { get; set; } = new TemperatureFunction();

        [XmlElement]
        public PropertyList Properties { get; set; } = new PropertyList();
    }

    public class PropertyList
    {
        public PropertyList() { Item = new List<PropertyMeasurement>(); }
        [XmlElement("Property")]
        public List<PropertyMeasurement> Item { get; }
    }

    public class Component : Measurement
    {
        [XmlAttribute]
        public Aga8Component Name { get; set; }

        public Component()
        {
            ScaleFactor = 1.0;
            Value = 0.0;
        }
    }

    public class Measurement
    {
        [XmlAttribute]
        public string NamespaceURI { get; set; }
        [XmlAttribute]
        public string Identifier { get; set; }
        [XmlAttribute]
        public string StartIdentifier { get; set; }
        [XmlAttribute]
        public string RelativePath { get; set; }
        [XmlAttribute]
        public double ScaleFactor { get; set; }
        [XmlAttribute]
        public string Type { get; set; }
        [XmlAttribute]
        public double Value { get; set; }
        [XmlAttribute]
        public int SamplingInterval { get; set; } = -2;

        [XmlIgnore]
        public StatusCode Quality { get; set; }

        [XmlIgnore]
        public string NodeId { get; set; }

        public double GetScaledValue()
        {
            return Value * ScaleFactor;
        }
    }

    public class PressureMeasurement : Measurement
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public ConfigModel.PressureUnit Unit { get; set; }

        const double stdAtm = 1.01325;

        public PressureMeasurement()
        {
            ScaleFactor = 1.0;
        }

        public double GetAGA8Converted()
        {
            // Convert from Unit to kPa absolute
            double result = 0.0;
            switch (Unit)
            {
                case ConfigModel.PressureUnit.barg:
                    result = ((Value * ScaleFactor) + stdAtm) * 100.0;
                    break;
                case ConfigModel.PressureUnit.bara:
                    result = (Value * ScaleFactor) * 100.0;
                    break;
                default:
                    break;
            }

            return (result);
        }

        public double GetUnitConverted()
        {
            // Convert from kPa absolute to Unit
            double result = 0.0;
            switch (Unit)
            {
                case ConfigModel.PressureUnit.barg:
                    result = (Value * ScaleFactor) / 100.0 - stdAtm;
                    break;
                case ConfigModel.PressureUnit.bara:
                    result = (Value * ScaleFactor) / 100.0;
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
        public string Name { get; set; }

        [XmlAttribute]
        public ConfigModel.TemperatureUnit Unit { get; set; }

        const double zeroCelsius = 273.15;

        public TemperatureMeasurement()
        {
            ScaleFactor = 1.0;
        }

        public double GetAGA8Converted()
        {
            // Convert from Unit to K
            double result = 0.0;
            switch (Unit)
            {
                case ConfigModel.TemperatureUnit.C:
                    result = (Value * ScaleFactor) + zeroCelsius;
                    break;
                case ConfigModel.TemperatureUnit.K:
                    result = (Value * ScaleFactor);
                    break;
                default:
                    break;
            }

            return (result);
        }

        public double GetUnitConverted()
        {
            // Convert from K to Unit
            double result = 0.0;
            switch (Unit)
            {
                case ConfigModel.TemperatureUnit.C:
                    result = (Value * ScaleFactor) - zeroCelsius;
                    break;
                case ConfigModel.TemperatureUnit.K:
                    result = (Value * ScaleFactor);
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
