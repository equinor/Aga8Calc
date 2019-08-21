using System;
using System.Runtime.InteropServices;

namespace Aga8CalcService
{
    class Aga8Calc
    {
        [DllImport("aga8_2017.dll")]
        public static extern double aga8_2017(double[] composition, double pressure, double temperature, Aga8ResultCode result);
        public enum Aga8ResultCode : Int32
        {
            Density = 0,
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
            IsentropicExponent = 14
        }


    }
}
