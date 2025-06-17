using ALVRAutoStart;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<HostOptions>(opts => opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
builder.Services.AddHostedService<Worker>();
//builder.Services.AddWindowsService(options =>
//{
//    options.ServiceName = "ALVRAutoStartService";
//});
var host = builder.Build();
host.Run();