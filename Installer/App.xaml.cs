using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;

namespace Installer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!IsRunAsAdministrator())
            {
                // Relaunch the application as administrator
                var exeName = Process.GetCurrentProcess().MainModule!.FileName!;
                var startInfo = new ProcessStartInfo(exeName)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = string.Join(" ", e.Args)
                };

                try
                {
                    Process.Start(startInfo);
                }
                catch
                {
                    // User refused the elevation
                }
                Application.Current.Shutdown();
                return;
            }
            base.OnStartup(e);
        }

        private static bool IsRunAsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
