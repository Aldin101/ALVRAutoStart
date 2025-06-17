using ALVRAutoStart;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

void Uninstall()
{
    // 1. Remove from Program Files
    string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
    string programFilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ALVRAutoStart");
    try
    {
        if (exePath.StartsWith(programFilesPath, StringComparison.OrdinalIgnoreCase))
        {
            // Try to delete the directory
            Directory.Delete(programFilesPath, true);
        }
    }
    catch { /* Ignore errors */ }

    // 2. Remove auto start key
    try
    {
        using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
        {
            key?.DeleteValue("ALVRAutoStart", false);
        }
    }
    catch { /* Ignore errors */ }

    // 3. Remove uninstall info
    try
    {
        using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
        {
            key?.DeleteSubKeyTree("ALVRAutoStart", false);
        }
    }
    catch { /* Ignore errors */ }
}

if (args.Contains("--uninstall", StringComparer.OrdinalIgnoreCase))
{
    Uninstall();
    return;
}

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<HostOptions>(opts => opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
builder.Services.AddHostedService<Worker>();
//builder.Services.AddWindowsService(options =>
//{
//    options.ServiceName = "ALVRAutoStartService";
//});
var host = builder.Build();
host.Run();