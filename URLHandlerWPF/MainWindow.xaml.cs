using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtensionMethods;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Security.Principal;
using Microsoft.Win32;

namespace URLHandlerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _Firefoxpath = @"C:\Program Files\Mozilla Firefox\firefox.exe";
        public string sURL = "";
        public bool bNiceClose = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ActivateCenteredToMouse();
            this.Opacity = 1.0F;
            
            foreach (string _arg in Environment.GetCommandLineArgs())
            {
                if ((_arg.ToLower().StartsWith("http")) && (_arg.Length < 8192)) // 
                {
                    sURL = _arg;
                }
            }
            if (sURL.Length < 8)
            {
                FF.IsEnabled = false; IE.IsEnabled = false;
                if (!IsElevated)
                {
                    Image MyContentImage = FindChild<Image>(Setup, "UACShield");
                    MyContentImage.Visibility = Visibility.Hidden;
                    
                    Setup.Content = "Restart as Administrator\nto register as a browser";
                    this.Opacity = 1.0F;
                }
                Setup.Visibility = Visibility.Visible;
                /*System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
                dispatcherTimer.Start();*/
            }
        }

        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null)
            {
                return null;
            }

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T childType = child as T;

                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);

                    if (foundChild != null) break;
                }
                else
                    if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;

                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                    else
                    {
                        foundChild = FindChild<T>(child, childName);

                        if (foundChild != null)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        static bool IsElevated
        {
            get
            {
                return WindowsIdentity.GetCurrent().Owner
                  .IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            bNiceClose = true;
            this.Close();
        }


        private void FF_Click(object sender, RoutedEventArgs e)
        {
            if (sURL.Length > 8)
            {
                Process process = new Process();
                process.StartInfo.FileName = _Firefoxpath;
                process.StartInfo.Arguments = sURL;// _URL;
                process.Start();
            }
            else
            {
                
            }
            bNiceClose = true;
            this.Close();
        }

        private void IE_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("microsoft-edge:" + sURL);
            bNiceClose = true;
            this.Close();
        }

        private void FF_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            bNiceClose = true;
            this.Close();
        }

        private void Setup_Click(object sender, RoutedEventArgs e)
        {
            string _exepath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            if (!IsElevated)
            {
                var psi = new ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.Verb = "runas";
                psi.FileName = _exepath;
                Process.Start(psi);
                this.Close();
            }
            else
            {
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", true);

                    key.CreateSubKey("URL-HandlerWPF");
                    key = key.OpenSubKey("URL-HandlerWPF", true);

                    key.CreateSubKey("Capabilities");
                    key = key.OpenSubKey("Capabilities", true);

                    key.SetValue("ApplicationDescription", "URL-HandlerWPF");
                    key.SetValue("ApplicationIcon", _exepath + ",0");
                    key.SetValue("ApplicationName", "URL-HandlerWPF");

                    key.CreateSubKey("URLAssociations");
                    key = key.OpenSubKey("URLAssociations", true);

                    key.SetValue("ftp", "URL-HandlerURL");
                    key.SetValue("http", "URL-HandlerURL");
                    key.SetValue("https", "URL-HandlerURL");

                    key = Registry.LocalMachine.OpenSubKey(@"Software\RegisteredApplications", true);
                    key.SetValue("URL-Handler", @"Software\URL-Handler\Capabilities");

                    key = Registry.LocalMachine.OpenSubKey(@"Software\Classes", true);
                    key.CreateSubKey("URL-HandlerWPFURL");
                    key = key.OpenSubKey("URL-HandlerWPFURL", true);

                    key.SetValue("", "URL-HandlerWPF Document");
                    key.SetValue("FriendlyTypeName", "URL-HandlerWPF http or https document");

                    key.CreateSubKey("shell");
                    key = key.OpenSubKey("shell", true);

                    key.CreateSubKey("open");
                    key = key.OpenSubKey("open", true);

                    key.CreateSubKey("command");
                    key = key.OpenSubKey("command", true);
                    key.SetValue("", "\"" + _exepath + "\" \"%1\"");

                    key.Close();
                    // Now launch the "Default Apps" Settings page, to allow the user to choose this tool as their default browser
                    var psi = new ProcessStartInfo();
                    psi.UseShellExecute = true;
                    psi.FileName = "ms-settings:defaultapps";
                    Process.Start(psi);
                    bNiceClose = true;
                    this.Close();
                }
                catch 
                {
                }
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (bNiceClose)
            {
                Closing -= Window_Closing;
                e.Cancel = true;
                var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromMilliseconds(500));
                anim.Completed += (s, _) => this.Close();
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            }
        }
    }
}
