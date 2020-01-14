using System;
using System.Runtime.InteropServices;

namespace Aga8CalcService
{
    internal class NativeMethods
    {
        [DllImport("aga8", EntryPoint = "aga8_new")]
        internal static extern AGA8DetailHandle Aga8New();
        [DllImport("aga8", EntryPoint = "aga8_free")]
        internal static extern void Aga8Free(IntPtr aga8);
        [DllImport("aga8", EntryPoint = "aga8_setup")]
        internal static extern void Aga8Setup(AGA8DetailHandle aga8);
        [DllImport("aga8", EntryPoint = "aga8_set_composition")]
        internal static extern void Aga8SetComposition(AGA8DetailHandle aga8, double[] composition);
        [DllImport("aga8", EntryPoint = "aga8_set_pressure")]
        internal static extern void Aga8SetPressure(AGA8DetailHandle aga8, double pressure);
        [DllImport("aga8", EntryPoint = "aga8_set_temperature")]
        internal static extern void Aga8SetTemperature(AGA8DetailHandle aga8, double temperature);
        [DllImport("aga8", EntryPoint = "aga8_calculate_density")]
        internal static extern void Aga8CalculateDensity(AGA8DetailHandle aga8);
        [DllImport("aga8", EntryPoint = "aga8_get_density")]
        internal static extern double Aga8GetDensity(AGA8DetailHandle aga8);
        [DllImport("aga8", EntryPoint = "aga8_calculate_properties")]
        internal static extern void Aga8CalculateProperties(AGA8DetailHandle aga8);
        [DllImport("aga8", EntryPoint = "aga8_get_properties")]
        internal static extern Aga8Properties Aga8GetProperties(AGA8DetailHandle aga8);

        [DllImport("aga8", EntryPoint = "aga8_2017")]
        internal static extern Aga8Properties Aga8_2017(double[] composition, double pressure,
            double temperature);

        [DllImport("aga8", EntryPoint = "gerg_new")]
        internal static extern GergHandle GergNnew();
        [DllImport("aga8", EntryPoint = "gerg_free")]
        internal static extern void GergFfree(IntPtr gerg);
        [DllImport("aga8", EntryPoint = "gerg_setup")]
        internal static extern void GergSetup(GergHandle gerg);
        [DllImport("aga8", EntryPoint = "gerg_set_composition")]
        internal static extern void GergSetComposition(GergHandle gerg, double[] composition);
        [DllImport("aga8", EntryPoint = "gerg_set_pressure")]
        internal static extern void GergSetPressure(GergHandle gerg, double pressure);
        [DllImport("aga8", EntryPoint = "gerg_set_temperature")]
        internal static extern void GergSetTemperature(GergHandle gerg, double temperature);
        [DllImport("aga8", EntryPoint = "gerg_calculate_density")]
        internal static extern void GergCalculateDensity(GergHandle gerg);
        [DllImport("aga8", EntryPoint = "gerg_get_density")]
        internal static extern double GergGetDensity(GergHandle aggerga8);
        [DllImport("aga8", EntryPoint = "gerg_calculate_properties")]
        internal static extern void GergCalculateProperties(GergHandle gerg);
        [DllImport("aga8", EntryPoint = "gerg_get_properties")]
        internal static extern Aga8Properties GergGetProperties(GergHandle gerg);

        [DllImport("aga8", EntryPoint = "gerg_2008")]
        internal static extern Aga8Properties Gerg2008(double[] composition, double pressure,
            double temperature);
    }
}
