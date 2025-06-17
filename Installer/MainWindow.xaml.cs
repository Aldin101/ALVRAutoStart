using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Installer
{
    public partial class MainWindow : Window
    {

        string dashboardPath = "none entered yet";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(dashboardPath))
            {
                MessageBox.Show("Please enter a valid ALVR Dashboard path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string programFilesPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ALVRAutoStart");
            if (!Directory.Exists(programFilesPath))
            {
                Directory.CreateDirectory(programFilesPath);
            }

            string adbZipUrl = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";
            string tempPath = System.IO.Path.GetTempPath();
            string adbZipPath = System.IO.Path.Combine(tempPath, "platform-tools-latest-windows.zip");
            using (var client = new System.Net.WebClient())
            {
                client.DownloadFile(adbZipUrl, adbZipPath);
            }
            System.IO.Compression.ZipFile.ExtractToDirectory(adbZipPath, programFilesPath, true);
            File.Delete(adbZipPath);

            string configPath = System.IO.Path.Combine(programFilesPath, "config.json");
            var config = new Dictionary<string, string>
            {
                { "adbPath", System.IO.Path.Combine(programFilesPath, "platform-tools", "adb.exe") },
                { "alvrPath", dashboardPath }
            };
            File.WriteAllText(configPath, System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            string programPath = System.IO.Path.Combine(programFilesPath, "ALVRAutoStart.exe");
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Installer.ALVRAutoStart.exe"))
            {
                if (stream != null)
                {
                    using (var fileStream = new FileStream(programPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }

            string startupPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "ALVRAutoStart.lnk");

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = programPath,
                UseShellExecute = true,
                CreateNoWindow = true
            });

            MessageBox.Show("ALVRAutoStart installed and started successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Application.Current.Shutdown();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DashboardPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DashboardPathTextBox.Text = DashboardPathTextBox.Text.Replace("\"", "").Replace("'", "");
            dashboardPath = DashboardPathTextBox.Text.Trim();
        }
    }
}