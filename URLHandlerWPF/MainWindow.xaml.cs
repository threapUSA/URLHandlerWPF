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
//using ExtensionMethods;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Security.Principal;
using Microsoft.Win32;
using System.IO;

namespace URLHandlerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string _Firefoxpath = @"C:\Program Files\Mozilla Firefox\firefox.exe";
        public static string _Chromepath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
        public static string _Browserpath = "";

        public bool bNiceClose = false;
        public System.Windows.Threading.DispatcherTimer dispatcherTimer;
        public Window wSetup;

        public MainWindow()
        {
            InitializeComponent();
            Variables.lsURLPatterns = new List<string>();
            Variables.bAutoClose = Properties.Settings.Default.AutoClose;

            Variables.lsURLPatterns = Properties.Settings.Default.Filters.Cast<string>().ToList();
                        
            if (Properties.Settings.Default.Browser.Contains("firefox"))
            {
                _Browserpath = _Firefoxpath;  
                //Application.Current.Resources["leftbuttonimage"] = System.Drawing.Icon.ExtractAssociatedIcon(_Firefoxpath);// new BitmapImage(new Uri(@"pack://application:,,,/FF.png"));
            }
            else
            {
                _Browserpath = _Chromepath;
            }
            if (IsFirefoxInstalled) { Variables.FFIcon = (ImageSource)getIconfromPath(_Firefoxpath); }
            if (IsChromeInstalled) { Variables.GCIcon = (ImageSource)getIconfromPath(_Chromepath); }

            Application.Current.Resources["leftbuttonimage"] = _Browserpath == _Firefoxpath ? Variables.FFIcon : Variables.GCIcon;
            
            FF.IsEnabled = File.Exists(_Browserpath);

            this.Opacity = 0.9F;
        }

        public static object getIconfromPath(string browserpath)
        {
            var icons0 = IconInfo.LoadIconsFromBinary(browserpath, 0);

            // get the jumbo icon
            var iconId_256 = icons0.First(i => i.WithIcon(it => it.Size.Width == 256));
            BitmapSource _is = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
            iconId_256.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
            icons0.ForEach(i => i.Dispose());
            return (_is);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            CenterWindowOnMousePosition();
        }

        private void CenterWindowOnMousePosition()
        {
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(GetMousePosition());
            Left = mouse.X - (int)(ActualWidth / 2.0F);
            Top = mouse.Y - (int)(ActualHeight / 2.0F);
        }

        public System.Windows.Point GetMousePosition()
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            return new System.Windows.Point(point.X, point.Y);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.ActivateCenteredToMouse();
            this.Opacity = 1.0F;
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 4);

            foreach (string _arg in Environment.GetCommandLineArgs())
            {
                if ((_arg.ToLower().StartsWith("http")) && (_arg.Length < 8192)) // 
                {
                    Variables.sURL = _arg;
                }
            }
            if (Variables.sURL.Length < 8)
            {
                //FF.IsEnabled = false; IE.IsEnabled = false;
                fShowSetup(true);
                if (!IsElevated)
                {
                    // no url, and not elevated
                    
                    this.Opacity = 1.0F;
                }
            } else
            {
                if (fCheckIfLinkIsOnWhitelist(Variables.sURL))
                {
                    Variables.bFavorEdge = true;
                    IE.IsDefault = true;
                }
            }
            
            if (Variables.bAutoClose) { dispatcherTimer.Start(); }
        }

        public static bool IsElevated
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
            if (Variables.bFavorEdge)
            {
                IE_Click(null, null);
            }
            else
            {
                this.Close();
            }
        }

        public static bool fCheckIfLinkIsOnWhitelist(string sLink)
        {
            bool bReturn = false;
            //if (Variables.lsURLPatterns.Any(x => sLink.ToLower().Contains(x.ToLower()))) { bReturn = true;  }

            foreach (string sTmp in Variables.lsURLPatterns)
            {
                if (sLink.ToLower().Contains(sTmp.ToLower()))
                {
                    bReturn = true;
                    
                }
            }
            
            return (bReturn);
        }

        
        private void FF_Click(object sender, RoutedEventArgs e)
        {
            if (Variables.sURL.Length > 8)
            {
                Process process = new Process();
                process.StartInfo.FileName = _Browserpath;
                process.StartInfo.Arguments = Variables.sURL;// _URL;
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
            Process.Start("microsoft-edge:" + Variables.sURL);
            bNiceClose = true;
            this.Close();
        }

        private void FF_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            fShowSetup(false);
        }

        private void fShowSetup(bool bInitialConfig)
        {
            dispatcherTimer.Stop();
            this.Topmost = false;
            this.Visibility = Visibility.Hidden;
            FF.InvalidateVisual();
            wSetup = new WindowSetup();
            if (false == wSetup.ShowDialog())
            {
                this.Close();
            }
            else
            {
                FF.IsEnabled = File.Exists(_Browserpath);
                this.Visibility = Visibility.Visible;
                this.Topmost = true;

                Variables.bFavorEdge = IE.IsDefault = fCheckIfLinkIsOnWhitelist(Variables.sURL);
                
                if (Variables.bAutoClose) { dispatcherTimer.Start(); }
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
                {
                case Key.D1: // 1
                    FF_Click(null, null);
                    break;
                case Key.D2: // 2
                    IE_Click(null, null);
                    break;
                case Key.Escape: // escape
                    this.Close();
                    break;
                default:
                    break;
            }
        }

        public static bool IsChromeInstalled
        {
            get
            {
                string sGoogle = "\\Google\\Chrome\\Application\\chrome.exe";
                return ((File.Exists(Environment.ExpandEnvironmentVariables("%ProgramW6432%") + sGoogle)) || (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + sGoogle)) || (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + sGoogle)));
            }
        }

        public static bool IsFirefoxInstalled
        {
            get
            {
                string sFirefox = "\\Mozilla Firefox\\firefox.exe";
                return ((File.Exists(Environment.ExpandEnvironmentVariables("%ProgramW6432%") + sFirefox)) || (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + sFirefox)));
            }
        }
    }
}
