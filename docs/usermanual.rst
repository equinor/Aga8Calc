.. highlight:: none

#############
User's manual
#############

Introduction
------------

Aga8Calc is a Windows service that takes gas composition, pressure and temperature, and returns one of the Aga8 properties.
It can return the following properties:

    - CompressibilityFactor [-]
    - Density [kg/m³]
    - Enthalpy [J/mol]
    - Entropy [J/(mol-K)]
    - GibbsEnergy [J/mol]
    - InternalEnergy [J/mol]
    - IsentropicExponent [-]
    - IsobaricHeatCapacity [J/(mol-K)]
    - IsochoricHeatCapacity [J/(mol-K)]
    - JouleThomsonCoefficient [K/kPa]
    - MolarConcentration [mol/l]
    - MolarMass [g/mol]
    - SpeedOfSound [m/s]

The gas composition, pressure and temperature are read from an OPC-UA server.
The result is written back to the same OPC-UA server.

Getting Aga8Calc
----------------

The latest release version of Aga8Calc can be downloaded from:
https://github.com/equinor/Aga8Calc/releases

Installation
------------

Copy the Aga8Calc folder to `C:\\Program Files`.
Open cmd or Powershell and run the installation command::

    PS C:\Program Files\Aga8Calc> .\Aga8CalcService.exe install

This will install Aga8Calc as a service that should start automatically when the computer starts.

It is also possible to run Aga8Calc directly from the command line::

    PS C:\Program Files\Aga8Calc> .\Aga8CalcService.exe

Running from the command line could be useful for testing.

To uninstall the Windows service run the Aga8CalcService.exe with the uninstall command::

    PS C:\Program Files\Aga8Calc> .\Aga8CalcService.exe uninstall

This should be done before installing a new version of Aga8Calc.

There are several more options available to Aga8CalcService.exe.
They can be seen by running::

    PS C:\Program Files\Aga8Calc> .\Aga8CalcService.exe --help

This will produce this output:

::

    Command-Line Reference

    Aga8CalcService.exe [verb] [-option:value] [-switch]

        run                 Runs the service from the command line (default)
        help, --help        Displays help

        install             Installs the service

        --autostart       The service should start automatically (default)
        --disabled        The service should be set to disabled
        --manual          The service should be started manually
        --delayed         The service should start automatically (delayed)
        -instance         An instance name if registering the service
                            multiple times
        -username         The username to run the service
        -password         The password for the specified username
        --localsystem     Run the service with the local system account
        --localservice    Run the service with the local service account
        --networkservice  Run the service with the network service permission
        --interactive     The service will prompt the user at installation for
                            the service credentials
        start             Start the service after it has been installed
        --sudo            Prompts for UAC if running on Vista/W7/2008

        -servicename      The name that the service should use when
                            installing
        -description      The service description the service should use when
                            installing
        -displayname      The display name the the service should use when
                            installing

        start               Starts the service if it is not already running

        stop                Stops the service if it is running

        uninstall           Uninstalls the service

        -instance         An instance name if registering the service
                            multiple times
        --sudo            Prompts for UAC if running on Vista/W7/2008

    Examples:

        Aga8CalcService.exe install
            Installs the service into the service control manager

        Aga8CalcService.exe install -username:joe -password:bob --autostart
            Installs the service using the specified username/password and
            configures the service to start automatically at machine startup

        Aga8CalcService.exe uninstall
            Uninstalls the service

        Aga8CalcService.exe install -instance:001
            Installs the service, appending the instance name to the service name
            so that the service can be installed multiple times. You may need to
            tweak the log4net.config to make this play nicely with the log files.


Configuration
-------------

The configuration file is structured like the example below.

.. code-block:: xml

    <?xml version="1.0" encoding="utf-8"?>
    <configuration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <OpcUrl>opc.tcp://lt-103009:62548/Quickstarts/DataAccessServer</OpcUrl>
      <OpcUser>xxx</OpcUser>
      <OpcPassword>xxx</OpcPassword>
      <Interval>1000</Interval>
      <ConfigList>
        <Config>
        ...
        </Config>
        <Config>
        ...
        </Config>
        <Config>
        ...
        </Config>
      </ConfigList>
    </configuration>

-   `<configuration>` is the root element.
    All other elements live inside this one.

-   `<OpcUrl>` is used to select what OPC server to connect to.

-   `<OpcUser>` and `<OpcPassword>` are used to select what user name and password to use to connect to the OPC server.

-   `<Interval>` is used to set the update interval of the calculation task.
    The interval is set in milli seconds, so 1000 would be 1 second.

-   `<ConfigList>` can contain one or more `<Config>` elements.

Every `<Config>` element is structured like below.

.. code-block:: xml

    <Config Name="GC 1">
      <Composition>
        <Component Name="Methane" Tag="ns=2;s=1:AI1001?A" ScaleFactor="0.01" />
        <Component Name="Nitrogen" Tag="ns=2;s=1:AI1001?J" ScaleFactor="0.01" />
        <Component Name="Carbon dioxide" Tag="ns=2;s=1:AI1001?K" ScaleFactor="0.01" />
        <Component Name="Ethane" Tag="ns=2;s=1:AI1001?B" ScaleFactor="0.01" />
        <Component Name="Propane" Tag="ns=2;s=1:AI1001?C" ScaleFactor="0.01" />
        <Component Name="Isobutane" Tag="ns=2;s=1:AI1001?D" ScaleFactor="0.01" />
        <Component Name="n-Butane" Tag="ns=2;s=1:AI1001?E" ScaleFactor="0.01" />
        <Component Name="Isopentane" Tag="ns=2;s=1:AI1001?F" ScaleFactor="0.01" />
        <Component Name="n-Pentane" Tag="ns=2;s=1:AI1001?G" ScaleFactor="0.01" />
        <Component Name="Hexane" Tag="ns=2;s=1:AI1001?I" ScaleFactor="0.01" />
      </Composition>
      <PressureTemperatureList>
        <PressureTemperature Name="Point 1">
          <Pressure Tag="ns=2;s=1:AI1001?Pressure" Unit="barg" />
          <Temperature Tag="ns=2;s=1:AI1001?Temperature" Unit="C" />
          <Properties>
            <Property Tag="ns=2;s=1:AI1001?Result" Property="MolarConcentration" Type="single" />
          </Properties>
        </PressureTemperature>
      </PressureTemperatureList>
    </Config>


This holds the values that is read from, and the result written back to the OPC server.

-   `<Composition>` contains up to 21 `<Component>` elements where each one contains attributes for the component.
    Attributes:

    - `Name` is used to identify the component in the log files.
    - `Tag` is the OPC item to read the value from.
    - `ScaleFactor` is used to scale the individual component values into the mol fraction range from 0-1.

    The sort order of the `<Component>` elements is significant.
    They must be in this order:
      - Methane
      - Nitrogen
      - Carbon dioxide
      - Ethane
      - Propane
      - Isobutane
      - n-Butane
      - Isopentane
      - n-Pentane
      - Hexane
      - Heptane
      - Octane
      - Nonane
      - Decane
      - Hydrogen
      - Oxygen
      - Carbon monoxide
      - Water
      - Hydrogen sulfide
      - Helium
      - Argon

-   `<PressureTemperatureList>` can contain several `<PressureTemperature>` elements.
    Every `<PressureTemperature>` element contains the pressure and temperature to read, and one or more properties that is to be written to the OPC server.

-   `<Pressure>` is the pressure to be read.
    Attributes:

    - `Tag` is the OPC item to read.
    - `Unit` is the expected engineering unit of the pressure value.
      This is used to convert the pressure value to the unit needed for the Aga8 equation of state, namely [kPa].
      The possible units are:

      - barg (bar gauge)
      - bara (bar absolute)

-   `<Temperature>` is the temperature to read.
    Attributes:

    - `Tag` is the OPC item to read.
    - `Unit` is the expected engineering unit of the temperature value.
      This is used to convert the temperature to the proper unit - [K].
      The possible temperature units are:

      - C (degree Celsius)
      - K (Kelvin)

-   `<Properties>` contains one or more `<Property>` elements.
    These are the results that will be written to the OPC server.
    The Attributes of the `<Property>` element are:

    - `Tag` is the OPC item to write to.
    - `Property` is the result that will be written to the OPC item.
      The possible options are:

      - CompressibilityFactor
      - Density
      - Enthalpy
      - Entropy
      - GibbsEnergy
      - InternalEnergy
      - IsentropicExponent
      - IsobaricHeatCapacity
      - IsochoricHeatCapacity
      - JouleThomsonCoefficient
      - MolarConcentration
      - MolarMass
      - SpeedOfSound

    - `Type` is the datatype that the OPC server expects for the item.
      Possible types are:

      - `single` a 32-bit floating point type.
      - `double` a 64-bit floating point type.

A complete, minimal configuration file could look like this.

.. code-block:: xml

    <?xml version="1.0" encoding="utf-8"?>
    <configuration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <OpcUrl>opc.tcp://lt-103009:62548/Quickstarts/DataAccessServer</OpcUrl>
      <OpcUser>username</OpcUser>
      <OpcPassword>password</OpcPassword>
      <Interval>10000.0</Interval>
      <ConfigList>
        <Config Name="GC 1">
          <Composition>
            <Component Name="Methane" Tag="ns=2;s=1:AI1001?A" ScaleFactor="0.01" />
            <Component Name="Nitrogen" Tag="ns=2;s=1:AI1001?J" ScaleFactor="0.01" />
            <Component Name="Carbon dioxide" Tag="ns=2;s=1:AI1001?K" ScaleFactor="0.01" />
            <Component Name="Ethane" Tag="ns=2;s=1:AI1001?B" ScaleFactor="0.01" />
            <Component Name="Propane" Tag="ns=2;s=1:AI1001?C" ScaleFactor="0.01" />
            <Component Name="Isobutane" Tag="ns=2;s=1:AI1001?D" ScaleFactor="0.01" />
            <Component Name="n-Butane" Tag="ns=2;s=1:AI1001?E" ScaleFactor="0.01" />
            <Component Name="Isopentane" Tag="ns=2;s=1:AI1001?F" ScaleFactor="0.01" />
            <Component Name="n-Pentane" Tag="ns=2;s=1:AI1001?G" ScaleFactor="0.01" />
            <Component Name="Hexane" Tag="ns=2;s=1:AI1001?I" ScaleFactor="0.01" />
          </Composition>
          <PressureTemperatureList>
            <PressureTemperature Name="Point 1">
              <Pressure Tag="ns=2;s=1:AI1001?Pressure" Unit="barg" />
              <Temperature Tag="ns=2;s=1:AI1001?Temperature" Unit="C" />
              <Properties>
                <Property Tag="ns=2;s=1:AI1001?Result" Property="MolarConcentration" Type="single" />
              </Properties>
            </PressureTemperature>
          </PressureTemperatureList>
        </Config>
      </ConfigList>
    </configuration>

Files
-----

-   **aga8.dll** Library that implements Aga8 Part 1 Detail equation of state.

-   **Aga8_Calc_Client.Config.xml** Config file for the OPC client.

-   **Aga8CalcService.exe** Main program.

-   **NLog.config** Configuration file for logging system.

-   **Aga8Calc.config** Main configuration file.


Sequence Diagram
----------------

.. uml::

    @startuml
    scale 1

    == Init ==
    Aga8Calc -> OpcServer : Connect request
    OpcServer --> Aga8Calc : Connect granted

    == Main loop ==
    loop forever
        Aga8Calc -> OpcServer : Poll pressure, temperature and composition
        OpcServer --> Aga8Calc : Return pressure, temperature, composition

        hnote over Aga8Calc : Calculate results

        Aga8Calc -> OpcServer : Write results

        hnote over Aga8Calc : Wait <interval> ms
    end
    @enduml
