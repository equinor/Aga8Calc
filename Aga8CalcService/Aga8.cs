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

    [StructLayout(LayoutKind.Sequential)]
    public struct Aga8Composition
    {
        public double Methane;
        public double Nitrogen;
        public double CarbonDioxide;
        public double Ethane;
        public double Propane;
        public double IsoButane;
        public double NormalButane;
        public double IsoPentane;
        public double NormalPentane;
        public double Hexane;
        public double Heptane;
        public double Octane;
        public double Nonane;
        public double Decane;
        public double Hydrogen;
        public double Oxygen;
        public double CarbonMonoxide;
        public double Water;
        public double HydrogenSulfide;
        public double Helium;
        public double Argon;
    }

    public enum CompositionError : int
    {
        Ok = 0,
        Empty = 1,
        BadSum = 2
    }

    public enum DensityError : int
    {
        Ok = 0,
        IterationFail = 1,
        PressureTooLow = 2,
    }

    interface IEquation : IDisposable
    {
        void SetComposition(Aga8Composition composition, ref CompositionError err);
        void SetPressure(double pressure);
        void SetTemperature(double temperature);
        void CalculateDensity(ref DensityError err);
        void CalculateProperties();
        double GetProperty(ConfigModel.Aga8ResultCode resultCode);
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

    public class AGA8Detail : IEquation
    {
        private readonly AGA8DetailHandle aga8;
        private Aga8Properties ResultProperties;
        private bool disposed = false;

        public AGA8Detail()
        {
            aga8 = NativeMethods.Aga8New();
        }

        ~AGA8Detail()
        {
            Dispose(false);
        }

        public void SetComposition(Aga8Composition composition, ref CompositionError err)
        {
            NativeMethods.Aga8SetComposition(aga8, composition, ref err);
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

        public void CalculateDensity(ref DensityError err)
        {
            NativeMethods.Aga8CalculateDensity(aga8, ref err);
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
                    return ResultProperties.kappa;
                case ConfigModel.Aga8ResultCode.Density:
                    return ResultProperties.d * ResultProperties.mm; // g/l = kg/m³
                default:
                    return ResultProperties.d;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // free managed objects
            }

            aga8.Close();
            disposed = true;
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

    public class Gerg2008 : IEquation
    {
        private readonly Gerg2008Handle gerg;
        private Aga8Properties ResultProperties;
        private bool disposed = false;

        public Gerg2008()
        {
            gerg = NativeMethods.GergNew();
        }

        ~Gerg2008()
        {
            Dispose(false);
        }

        public void SetComposition(Aga8Composition composition, ref CompositionError err)
        {
            NativeMethods.GergSetComposition(gerg, composition, ref err);
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

        public void CalculateDensity(ref DensityError err)
        {
            NativeMethods.GergCalculateDensity(gerg, ref err);
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
                    return ResultProperties.kappa;
                case ConfigModel.Aga8ResultCode.Density:
                    return ResultProperties.d * ResultProperties.mm;
                default:
                    return ResultProperties.d;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free managed objects
            }

            gerg.Close();
            disposed = true;
        }
    }

    internal class NativeMethods
    {
        // External dependency: aga8
        // URL: https://github.com/royvegard/aga8
        // Version: 0.4.0
        [DllImport("aga8", EntryPoint = "aga8_new")]
        internal static extern AGA8DetailHandle Aga8New();
        [DllImport("aga8", EntryPoint = "aga8_free")]
        internal static extern void Aga8Free(IntPtr aga8);
        [DllImport("aga8", EntryPoint = "aga8_set_composition")]
        internal static extern void Aga8SetComposition(AGA8DetailHandle aga8, Aga8Composition composition, ref CompositionError err);
        [DllImport("aga8", EntryPoint = "aga8_set_pressure")]
        internal static extern void Aga8SetPressure(AGA8DetailHandle aga8, double pressure);
        [DllImport("aga8", EntryPoint = "aga8_set_temperature")]
        internal static extern void Aga8SetTemperature(AGA8DetailHandle aga8, double temperature);
        [DllImport("aga8", EntryPoint = "aga8_calculate_density")]
        internal static extern void Aga8CalculateDensity(AGA8DetailHandle aga8, ref DensityError err);
        [DllImport("aga8", EntryPoint = "aga8_get_density")]
        internal static extern double Aga8GetDensity(AGA8DetailHandle aga8);
        [DllImport("aga8", EntryPoint = "aga8_calculate_properties")]
        internal static extern void Aga8CalculateProperties(AGA8DetailHandle aga8);
        [DllImport("aga8", EntryPoint = "aga8_get_properties")]
        internal static extern Aga8Properties Aga8GetProperties(AGA8DetailHandle aga8);

        [DllImport("aga8", EntryPoint = "gerg_new")]
        internal static extern Gerg2008Handle GergNew();
        [DllImport("aga8", EntryPoint = "gerg_free")]
        internal static extern void GergFree(IntPtr gerg);
        [DllImport("aga8", EntryPoint = "gerg_set_composition")]
        internal static extern void GergSetComposition(Gerg2008Handle gerg, Aga8Composition composition, ref CompositionError err);
        [DllImport("aga8", EntryPoint = "gerg_set_pressure")]
        internal static extern void GergSetPressure(Gerg2008Handle gerg, double pressure);
        [DllImport("aga8", EntryPoint = "gerg_set_temperature")]
        internal static extern void GergSetTemperature(Gerg2008Handle gerg, double temperature);
        [DllImport("aga8", EntryPoint = "gerg_calculate_density")]
        internal static extern void GergCalculateDensity(Gerg2008Handle gerg, ref DensityError err);
        [DllImport("aga8", EntryPoint = "gerg_get_density")]
        internal static extern double GergGetDensity(Gerg2008Handle aggerga8);
        [DllImport("aga8", EntryPoint = "gerg_calculate_properties")]
        internal static extern void GergCalculateProperties(Gerg2008Handle gerg);
        [DllImport("aga8", EntryPoint = "gerg_get_properties")]
        internal static extern Aga8Properties GergGetProperties(Gerg2008Handle gerg);
    }
}
