##################
Developer's Manual
##################

Project Structure
=================

Class Diagrams
--------------


.. uml::

    @startuml
    class Measurement {
        string Name
        string NamespaceURI
        string Identifier
        double ScaleFactor
        string Type
        double Value
        string NodeId
    }

    class PressureMeasurement {
        PressureUnit Unit
        PressureMeasurement()
        double GetAGA8Converted()
        double GetUnitConverted()
    }

    class TemperatureMeasurement {
        TemperatureUnit Unit
        TemperatureMeasurement()
        double GetAGA8Converted()
        double GetUnitConverted()
    }

    Measurement <|-- PressureMeasurement
    Measurement <|-- TemperatureMeasurement

    @enduml
