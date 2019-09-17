# Aga8Calc

An OPC UA client that reads gas composition, pressure and temperature
from an OPC UA server, calculates Aga8 parameters (like density) and
writes the results back to an OPC UA server.

## Using
Runs as a Windows service.
See [documentation](Aga8CalcService/Documentation/Aga8Calc.md).

## External dependencies
Aga8 calculation is performed by https://github.com/usnistgov/AGA8
