using System;
using System.Globalization;
using Topshelf;

namespace Aga8CalcService
{
    class Program
    {
        static void Main()
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<Aga8Calc>(s =>
                {
                    s.ConstructUsing(aga8Calc => new Aga8Calc());
                    s.WhenStarted(aga8Calc => aga8Calc.Start());
                    s.WhenStopped(aga8Calc => aga8Calc.Stop());
                });

                x.RunAsLocalSystem();

                x.SetServiceName("Aga8CalcService");
                x.SetDisplayName("Aga8 Calc Service");
                x.SetDescription("Calculates Aga8 parameters like density, molar mass, speed of sound.");
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode(), CultureInfo.InvariantCulture);
            Environment.ExitCode = exitCodeValue;
        }
    }
}
