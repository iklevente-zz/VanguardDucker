using System.Windows;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Security.Principal;

namespace VanguardToggler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // This bool is required to fix a bug. Without it, it would start the ToggleButton_Checked/Unchecked void after initialization even though the user did not cick on it.
        public bool isInitializationComplete = false;

        public static string vanguardPath = @"C:\Program Files\Riot Vanguard";

        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            ServiceController vgk = new ServiceController("vgk");
            if (vgk.Status == ServiceControllerStatus.Stopped)
            {
                toggler.IsChecked = false;
                infoText.Text = "Vanguard  is disabled.";
            }
            else
            {
                toggler.IsChecked = true;
                infoText.Text = "Vanguard is enabled.";
            }

            if (File.Exists(vanguardPath + "\\vgtray.exe"))
            {
                trayToggler.IsChecked = true;
            }
            else if (File.Exists(vanguardPath + "\\vgtray1.exe"))
            {
                trayToggler.IsChecked = false;
            }
            else
            {
                infoText.Text= "Vanguard is not installed.";
            }

            if (!IsAdministrator())
            {
                infoText.Text = "Please run the software with administrator privileges!";
                toggler.IsEnabled = false;
                trayToggler.IsEnabled = false;
                killVanguardButton.IsEnabled = false;
                restartButton.IsEnabled = false;
            }

            isInitializationComplete = true;
        }

        private void toggler_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitializationComplete == true)
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c sc config vgc start= demand") { CreateNoWindow = true });
                Process.Start(new ProcessStartInfo("cmd", $"/c sc config vgk start= system") { CreateNoWindow = true });

                if (trayToggler.IsChecked == false)
                {
                    trayToggler.IsChecked = true;
                }

                infoText.Text = "Vanguard enabled, restart is required.";
            }
        }

        private void toggler_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isInitializationComplete == true)
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c sc config vgc start= disabled") { CreateNoWindow = true });
                Process.Start(new ProcessStartInfo("cmd", $"/c sc config vgk start= disabled") { CreateNoWindow = true });
                infoText.Text = "Vanguard disabled. It will not start at boot.";
            }
        }

        private void trayToggler_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitializationComplete == true)
            {
                File.Move(@"C:\Program Files\Riot Vanguard\vgtray1.exe", @"C:\Program Files\Riot Vanguard\vgtray.exe");
                infoText.Text = "Vanguard tray enabled.";
            }
        }
        private void trayToggler_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isInitializationComplete == true)
            {
                File.Move(@"C:\Program Files\Riot Vanguard\vgtray.exe", @"C:\Program Files\Riot Vanguard\vgtray1.exe");

                if (toggler.IsChecked == true)
                {
                    toggler.IsChecked = false;
                }

                infoText.Text = "Vanguard tray disabled.";
            }
        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
        }

        private void killVanguardButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c net stop vgc") { CreateNoWindow = true });
            Process.Start(new ProcessStartInfo("cmd", $"/c net stop vgk") { CreateNoWindow = true });
            Process.Start(new ProcessStartInfo("cmd", $"/c taskkill /IM vgtray.exe") { CreateNoWindow = true });
            infoText.Text = "Vanguard killed.";
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/iklevente";
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
    }
}
