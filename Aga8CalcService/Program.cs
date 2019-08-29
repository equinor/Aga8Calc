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
                x.Service<Heartbeat>(s =>
                {
                    s.ConstructUsing(heartbeat => new Heartbeat());
                    s.WhenStarted(heartbeat => heartbeat.Start());
                    s.WhenStopped(heartbeat => heartbeat.Stop());
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
