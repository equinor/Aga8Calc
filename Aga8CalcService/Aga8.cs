using System;
using System.Runtime.InteropServices;

namespace Aga8CalcService
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Aga8Properties
    {
        public double d; // Molar concentration [mol/l]
        public double mm; // Molar mass (g/mol)
        public double z; // Compressibility factor
        public double dp_dd; // First derivative of pressure with respect to density at constant temperature [kPa/(mol/l)]
        public double d2p_dd2; // Second derivative of pressure with respect to density at constant temperature [kPa/(mol/l)^2]
        public double dp_dt; // First derivative of pressure with respect to temperature at constant density (kPa/K)
        public double u; // Internal energy (J/mol)
        public double h; // Enthalpy (J/mol)
        public double s; // Entropy [J/(mol-K)]
        public double cv; // Isochoric heat capacity [J/(mol-K)]
        public double cp; // Isobaric heat capacity [J/(mol-K)]
        public double w; // Speed of sound (m/s)
        public double g; // Gibbs energy (J/mol)
        public double jt; // Joule-Thomson coefficient (K/kPa)
        public double kappa; // Isentropic Exponent
    }

    internal class AGA8DetailHandle : SafeHandle
    {
        public AGA8DetailHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid
        {
            get { return false; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.Aga8Free(handle);
            return true;
        }
    }

    public sealed class AGA8Detail : IDisposable
    {
        private readonly AGA8DetailHandle aga8;
        private Aga8Properties ResultProperties;

        public AGA8Detail()
        {
            aga8 = NativeMethods.Aga8New();
        }

        public void Setup()
        {
            NativeMethods.Aga8Setup(aga8);
        }

        public void SetComposition(double[] composition)
        {
            if (composition is null)
            {
                throw new System.Exception("composition can not be null");
            }

            double[] comp = new double[21];

            for (int i = 0; i < composition.Length; i++)
            {
                comp[i] = composition[i];
            }

            NativeMethods.Aga8SetComposition(aga8, comp);
        }

        public void SetPressure(double pressure)
        {
            NativeMethods.Aga8SetPressure(aga8, pressure);
        }

        public void SetTemperature(double temperature)
        {
            NativeMethods.Aga8SetTemperature(aga8, temperature);
        }

        public double GetDensity()
        {
            return NativeMethods.Aga8GetDensity(aga8);
        }

        public void CalculateDensity()
        {
            NativeMethods.Aga8CalculateDensity(aga8);
        }

        public void CalculateProperties()
        {
            NativeMethods.Aga8CalculateProperties(aga8);
            ResultProperties = NativeMethods.Aga8GetProperties(aga8);
        }

        public double GetProperty(ConfigModel.Aga8ResultCode resultCode)
        {
            switch (resultCode)
            {
                case ConfigModel.Aga8ResultCode.MolarConcentration:
                    return ResultProperties.d;
                case ConfigModel.Aga8ResultCode.MolarMass:
                    return ResultProperties.mm;
                case ConfigModel.Aga8ResultCode.CompressibilityFactor:
                    return ResultProperties.z;
                case ConfigModel.Aga8ResultCode.InternalEnergy:
                    return ResultProperties.u;
                case ConfigModel.Aga8ResultCode.Enthalpy:
                    return ResultProperties.h;
                case ConfigModel.Aga8ResultCode.Entropy:
                    return ResultProperties.s;
                case ConfigModel.Aga8ResultCode.IsochoricHeatCapacity:
                    return ResultProperties.cv;
                case ConfigModel.Aga8ResultCode.IsobaricHeatCapacity:
                    return ResultProperties.cp;
                case ConfigModel.Aga8ResultCode.SpeedOfSound:
                    return ResultProperties.w;
                case ConfigModel.Aga8ResultCode.GibbsEnergy:
                    return ResultProperties.g;
                case ConfigModel.Aga8ResultCode.JouleThomsonCoefficient:
                    return ResultProperties.jt;
                case ConfigModel.Aga8ResultCode.IsentropicExponent:
                    return ResultProperties.u;
                case ConfigModel.Aga8ResultCode.Density:
                    return ResultProperties.d * ResultProperties.mm; // g/l = kg/m³
                default:
                    return ResultProperties.d;
            }
        }

        public void Dispose()
        {
            aga8.Dispose();
        }

    }

    internal class Gerg2008Handle : SafeHandle
    {
        public Gerg2008Handle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid
        {
            get { return false; }
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.GergFree(handle);
            return true;
        }
    }

    public sealed class Gerg2008 : IDisposable
    {
        private readonly Gerg2008Handle gerg;
        private Aga8Properties ResultProperties;

        public Gerg2008()
        {
            gerg = NativeMethods.GergNnew();
        }

        public void Setup()
        {
            NativeMethods.GergSetup(gerg);
        }

        public void SetComposition(double[] composition)
        {
            if (composition is null)
            {
                throw new System.Exception("composition can not be null");
            }

            double[] comp = new double[21];

            for (int i = 0; i < composition.Length; i++)
            {
                comp[i] = composition[i];
            }

            NativeMethods.GergSetComposition(gerg, composition);
        }

        public void SetPressure(double pressure)
        {
            NativeMethods.GergSetPressure(gerg, pressure);
        }

        public void SetTemperature(double temperature)
        {
            NativeMethods.GergSetTemperature(gerg, temperature);
        }

        public double GetDensity()
        {
            return NativeMethods.GergGetDensity(gerg);
        }

        public void CalculateDensity()
        {
            NativeMethods.GergCalculateDensity(gerg);
        }

        public void CalculateProperties()
        {
            NativeMethods.GergCalculateProperties(gerg);
            ResultProperties = NativeMethods.GergGetProperties(gerg);
        }

        public double GetProperty(ConfigModel.Aga8ResultCode resultCode)
        {
            switch (resultCode)
            {
                case ConfigModel.Aga8ResultCode.MolarConcentration:
                    return ResultProperties.d;
                case ConfigModel.Aga8ResultCode.MolarMass:
                    return ResultProperties.mm;
                case ConfigModel.Aga8ResultCode.CompressibilityFactor:
                    return ResultProperties.z;
                case ConfigModel.Aga8ResultCode.InternalEnergy:
                    return ResultProperties.u;
                case ConfigModel.Aga8ResultCode.Enthalpy:
                    return ResultProperties.h;
                case ConfigModel.Aga8ResultCode.Entropy:
                    return ResultProperties.s;
                case ConfigModel.Aga8ResultCode.IsochoricHeatCapacity:
                    return ResultProperties.cv;
                case ConfigModel.Aga8ResultCode.IsobaricHeatCapacity:
                    return ResultProperties.cp;
                case ConfigModel.Aga8ResultCode.SpeedOfSound:
                    return ResultProperties.w;
                case ConfigModel.Aga8ResultCode.GibbsEnergy:
                    return ResultProperties.g;
                case ConfigModel.Aga8ResultCode.JouleThomsonCoefficient:
                    return ResultProperties.jt;
                case ConfigModel.Aga8ResultCode.IsentropicExponent:
                    return ResultProperties.u;
                case ConfigModel.Aga8ResultCode.Density:
                    return ResultProperties.d * ResultProperties.mm;
                default:
                    return ResultProperties.d;
            }
        }

        public void Dispose()
        {
            gerg.Dispose();
        }
    }
}
