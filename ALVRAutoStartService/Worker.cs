using System.Diagnostics;

namespace ALVRAutoStart
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        const bool DEBUG = false;
        bool hasStartedOnThisWakeSession = false;
        string adbPath = "C:\\ALVR\\platform-tools-latest-windows\\platform-tools\\adb.exe";
        string alvrPath = "C:\\ALVR\\installations\\v20.13.0\\ALVR Dashboard.exe";
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            var existingProcesses = Process.GetProcessesByName("ALVRAutoStart");
            if (existingProcesses.Length > 1)
            {
                _logger.LogError("ALVRAutoStart is already running. Exiting to prevent multiple instances.");
                throw new InvalidOperationException("ALVRAutoStart is already running.");
            }

            if (!DEBUG)
            {
                string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ALVRAutoStart", "config.json");
                if (File.Exists(configPath))
                {
                    try
                    {
                        var config = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(configPath));
                        if (config != null)
                        {
                            adbPath = config.GetValueOrDefault("adbPath", adbPath);
                            alvrPath = config.GetValueOrDefault("alvrPath", alvrPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to read configuration file. Exiting.");
                        throw new InvalidOperationException("Failed to read configuration file.", ex);
                    }
                }
            }

            if (!File.Exists(adbPath))
            {
                throw new FileNotFoundException("ADB executable not found at specified path.", adbPath);
            }

            if (!File.Exists(alvrPath))
            {
                throw new FileNotFoundException("ALVR Dashboard executable not found at specified path.", alvrPath);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                var adbProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = adbPath,
                        Arguments = "shell dumpsys power",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                adbProcess.Start();
                string output = await adbProcess.StandardOutput.ReadToEndAsync();
                adbProcess.WaitForExit();

                if (output.Contains("mWakefulness=Awake"))
                {
                    //prevent alvr from starting again if you take off you headset and close the streamer before the headset sleeps, it was annoying
                    if (hasStartedOnThisWakeSession)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Device is awake but it hasn't gone to sleep since ALVR was started");
                        }
                        continue;
                    }

                    var alvrProcesses = Process.GetProcessesByName("ALVR Dashboard");
                    if (alvrProcesses.Length > 0)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Device is awake but ALVR is already running");
                        }
                        continue;
                    }

                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Device is awake, starting ALVR");
                        hasStartedOnThisWakeSession = true;
                    }

                    var alvrProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = alvrPath,
                            UseShellExecute = true
                        }
                    };
                    alvrProcess.Start();
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Device is asleep");
                        hasStartedOnThisWakeSession = false;
                    }
                }
            }
        }
    }
}