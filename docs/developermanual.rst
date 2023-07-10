##################
Developer's Manual
##################

Class Diagrams
==============

ConfigModel
-----------

.. uml::

    @startuml
    class ConfigModel {
        + OpcUrl : string <<get>> <<set>>
        + OpcUser : string <<get>> <<set>>
        + OpcPassword : string <<get>> <<set>>
        + DefaultNamespaceURI : string <<get>> <<set>>
        + Interval : double <<get>> <<set>>
        + {static} ReadConfig(file:string) : ConfigModel
    }
    class ConfigList {
        + ConfigList()
    }
    class Config {
        + Name : string <<get>> <<set>>
        + Config()
    }
    class CompositionList {
        + CompositionList()
        + GetScaledValues() : Aga8Composition
    }
    class PTList {
        + PTList()
    }
    class PressureFunction {
        + PressureFunction()
        + GetValue() : double
    }
    class TemperatureFunction {
        + TemperatureFunction()
        + GetValue() : double
    }
    class PressureTemperature {
        + Name : string <<get>> <<set>>
    }
    class PropertyList {
        + PropertyList()
    }
    class Component {
        + NamespaceURI : string <<get>> <<set>>
        + Identifier : string <<get>> <<set>>
        + ScaleFactor : double <<get>> <<set>>
        + Value : double <<get>> <<set>>
        + NodeId : string <<get>> <<set>>
        + Component()
        + GetScaledValue() : double
    }
    class Measurement {
        + Name : string <<get>> <<set>>
        + NamespaceURI : string <<get>> <<set>>
        + Identifier : string <<get>> <<set>>
        + ScaleFactor : double <<get>> <<set>>
        + Type : string <<get>> <<set>>
        + Value : double <<get>> <<set>>
        + NodeId : string <<get>> <<set>>
    }
    class PressureMeasurement {
        + PressureMeasurement()
        + GetAGA8Converted() : double
        + GetUnitConverted() : double
    }
    class TemperatureMeasurement {
        + TemperatureMeasurement()
        + GetAGA8Converted() : double
        + GetUnitConverted() : double
    }
    class PropertyMeasurement {
        + GetTypedValue() : object
    }
    class TimeStampedMeasurement {
    }
    enum Aga8Component {
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
        Argon,
    }
    enum Aga8ResultCode {
        MolarConcentration= 0,
        MolarMass= 1,
        CompressibilityFactor= 2,
        InternalEnergy= 6,
        Enthalpy= 7,
        Entropy= 8,
        IsochoricHeatCapacity= 9,
        IsobaricHeatCapacity= 10,
        SpeedOfSound= 11,
        GibbsEnergy= 12,
        JouleThomsonCoefficient= 13,
        IsentropicExponent= 14,
        Density= 15,
    }
    enum PressureUnit {
        barg= 0,
        bara= 1,
    }
    enum TemperatureUnit {
        C= 0,
        K= 1,
    }
    enum Func {
        Min= 0,
        Max= 1,
        Average= 2,
        Median= 3,
    }
    enum Equation {
        AGA8Detail= 0,
        Gerg2008= 1,
    }
    class "List`1"<T> {
    }
    ConfigModel --> "EquationOfState" Equation
    ConfigModel o-> "ConfigList" ConfigList
    ConfigList --> "Item<Config>" "List`1"
    Config o-> "Composition" CompositionList
    Config o-> "PressureTemperatureList" PTList
    CompositionList --> "Item<Component>" "List`1"
    PTList --> "Item<PressureTemperature>" "List`1"
    PressureFunction --> "Item<PressureMeasurement>" "List`1"
    TemperatureFunction --> "Item<TemperatureMeasurement>" "List`1"
    PressureTemperature o-> "PressureFunction" PressureFunction
    PressureTemperature o-> "TemperatureFunction" TemperatureFunction
    PressureTemperature o-> "Properties" PropertyList
    PropertyList --> "Item<PropertyMeasurement>" "List`1"
    Component --> "Name" Aga8Component
    Measurement <|-- PressureMeasurement
    Measurement <|-- TemperatureMeasurement
    Measurement <|-- PropertyMeasurement
    Measurement <|-- TimeStampedMeasurement
    TimeStampedMeasurement --> "TimeStamp" DateTime
    ConfigModel +-- Aga8Component
    ConfigModel +-- Aga8ResultCode
    ConfigModel +-- PressureUnit
    ConfigModel +-- TemperatureUnit
    ConfigModel +-- Func
    ConfigModel +-- Equation
    @enduml
