.. highlight:: none

#############
User's manual
#############

Getting Aga8Calc
----------------

The latest release version of Aga8Calc can be downloaded from:
https://github.com/equinor/Aga8Calc/releases

Installation
------------

Copy the Aga8Calc folder to `C:\Program Files`.
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
      <opc_url>opc.tcp://lt-103009:62548/Quickstarts/DataAccessServer</opc_url>
      <opc_user>xxx</opc_user>
      <opc_password>xxx</opc_password>
      <interval>1000</interval>
      <config_list>
        <config>
        ...
        </config>
        <config>
        ...
        </config>
        <config>
        ...
        </config>
      </config_list>
    </configuration>

-   `<configuration>` is the root element.
    All other elements live inside this one.

-   `<opc_url>` is used to select what OPC server to connect to.

-   `<opc_user>` and `<opc_password>` are used to select what user name and password to use to connect to the OPC server.

-   `<interval>` is used to set the update interval of the calculation task.
    The interval is set in milli seconds, so 1000 would be 1 second.

-   `<config_list>` can contain one or more `<config>` elements.

Every `<config>` element is structured like below.

.. code-block:: xml

    <config>
      <!-- Sort order is significant -->
      <composition_tag>
        <!-- Methane -->
        <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_A</string>
        <!-- Nitrogen -->
        <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_J</string>
        <!-- Carbon dioxide -->
        <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_K</string>
        <!-- Ethane -->
        <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_B</string>
        <!-- Propane -->
        <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_C</string>
        <!-- Isobutane -->
        <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_D</string>
        <!-- n-Butane -->
        <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_E</string>
        <!-- Isopentane -->
        <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_F</string>
        <!-- n-Pentane -->
        <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_G</string>
        <!-- Hexane -->
        <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_I</string>
        <!-- Heptane -->
        <string xsi:nil="true" />
        <!-- Octane -->
        <string xsi:nil="true" />
        <!-- Nonane -->
        <string xsi:nil="true" />
        <!-- Decane -->
        <string xsi:nil="true" />
        <!-- Hydrogen -->
        <string xsi:nil="true" />
        <!-- Oxygen -->
        <string xsi:nil="true" />
        <!-- Carbon monoxide -->
        <string xsi:nil="true" />
        <!-- Water -->
        <string xsi:nil="true" />
        <!-- Hydrogen sulfide -->
        <string xsi:nil="true" />
        <!-- Helium -->
        <string xsi:nil="true" />
        <!-- Argon -->
        <string xsi:nil="true" />
      </composition_tag>
      <pressure_tag>24PI1234</pressure_tag>
      <temperature_tag>24TI1234</temperature_tag>
      <calculation>Density</calculation>
      <result_tag>24DI1234</result_tag>
    </config>

This holds the values that is read from, and the result written back to the OPC server.

-   `<composition_tag>` contains several `<string>` elements where each one contains the OPC item for one gas component.
    The sort order and number of components is significant.
    The number of components shall be 21.
    No more. No less.
    21 shall be the number of components, and the number of components shall be 21.
    It shall not be 22, nor shall it be 20.
    23 is right out.

-   `<pressure_tag>` is the OPC item for the pressure.
    The value of this item is read from the OPC server.

-   `<temperature_tag>` is the OPC item for the temperature.
    The value is read from the OPC server.

-   `<calculation>` lets you select what type of result that will be put into the `<result_tag>` element.
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

-   `<result_tag>` is the OPC item for the calculation result.
    The result value will be written to this item on the OPC server.

A complete, minimal configuration file could look like this.

.. code-block:: xml

    <?xml version="1.0" encoding="utf-8"?>
    <configuration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <opc_url>opc.tcp://lt-103009:62548/Quickstarts/DataAccessServer</opc_url>
      <opc_user>xxx</opc_user>
      <opc_password>xxx</opc_password>
      <interval>1000</interval>
      <config_list>
        <config>
        <!-- Sort order is significant -->
          <composition_tag>
            <!-- Methane -->
            <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_A</string>
            <!-- Nitrogen -->
            <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_J</string>
            <!-- Carbon dioxide -->
            <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_K</string>
            <!-- Ethane -->
            <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_B</string>
            <!-- Propane -->
            <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_C</string>
            <!-- Isobutane -->
            <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_D</string>
            <!-- n-Butane -->
            <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_E</string>
            <!-- Isopentane -->
            <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_F</string>
            <!-- n-Pentane -->
            <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_G</string>
            <!-- Hexane -->
            <string>ns=2;s=ABB_800xA_Surrogate.S.24AI1234_I</string>
            <!-- Heptane -->
            <string xsi:nil="true" />
            <!-- Octane -->
            <string xsi:nil="true" />
            <!-- Nonane -->
            <string xsi:nil="true" />
            <!-- Decane -->
            <string xsi:nil="true" />
            <!-- Hydrogen -->
            <string xsi:nil="true" />
            <!-- Oxygen -->
            <string xsi:nil="true" />
            <!-- Carbon monoxide -->
            <string xsi:nil="true" />
            <!-- Water -->
            <string xsi:nil="true" />
            <!-- Hydrogen sulfide -->
            <string xsi:nil="true" />
            <!-- Helium -->
            <string xsi:nil="true" />
            <!-- Argon -->
            <string xsi:nil="true" />
          </composition_tag>
          <pressure_tag>24PI1234</pressure_tag>
          <temperature_tag>24TI1234</temperature_tag>
          <calculation>Density</calculation>
          <result_tag>24DI1234</result_tag>
        </config>
      </config_list>
    </configuration>

.. note:: Not every component of the composition needs to have an item,
    but the number of components must be exactly 21.
    And they must be in the same order as shown here.

Files
-----

-   **aga8_2017.dll** Library that implements Aga8 Part 1 Detail equation of state.

-   **Aga8_Calc_Client.Config.xml** Config file for the OPC client.

-   **Aga8CalcService.exe** Main program.

-   **NLog.config** Configuration file for logging system.

-   **Tag_Config.xml** Main configuration file.


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
