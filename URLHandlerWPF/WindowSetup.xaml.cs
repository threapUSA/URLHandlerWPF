using System;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Win32;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Reflection;
using System.Windows.Documents;


namespace URLHandlerWPF
{
    /// <summary>
    /// Interaction logic for WindowSetup.xaml
    /// </summary>
    public partial class WindowSetup : Window
    {

        bool isLightTheme = true;
        System.Windows.Threading.DispatcherTimer dtHighlightMatches = new System.Windows.Threading.DispatcherTimer();

        [DllImport("Shell32.dll", SetLastError = false)]
        public static extern Int32 SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);
        
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        public enum SHSTOCKICONID : uint
        {
            SIID_SHIELD = 77,
            SIID_MAX_ICONS = 175

        }
        [Flags]
        public enum SHGSI : uint
        {
            SHGSI_ICONLOCATION = 0,
            SHGSI_ICON = 0x000000100,
            SHGSI_SYSICONINDEX = 0x000004000,
            SHGSI_LINKOVERLAY = 0x000008000,
            SHGSI_SELECTED = 0x000010000,
            SHGSI_LARGEICON = 0x000000000,
            SHGSI_SMALLICON = 0x000000001,
            SHGSI_SHELLICONSIZE = 0x000000004
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public UInt32 cbSize;
            public IntPtr hIcon;
            public Int32 iSysIconIndex;
            public Int32 iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }
        public Image shieldimage { get; set; } = new Image();

        public WindowSetup()
        {
            InitializeComponent();
            isLightTheme = SetColors(IsLightTheme());
            dtHighlightMatches.Tick += dtHighlightMatches_Tick;
            dtHighlightMatches.Interval = new TimeSpan(0, 0, 0, 0, 300);

            butFF.Tag = Variables.FFIcon;
            BitmapImage x = new BitmapImage(new Uri(@"pack://application:,,,/FF.png"));
            butGC.Tag = Variables.GCIcon;// MainWindow.getIconfromPath(MainWindow._Chromepath);//new BitmapImage(new Uri(@"pack://application:,,,/Google_Chrome.png"));
            butGC.IsChecked = !(butFF.IsChecked = MainWindow._Browserpath.Contains("firefox"));

            labVer.Text = String.Format("Version\n{0}",File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("d"));

            chkCloseDelay.IsChecked = Properties.Settings.Default.AutoClose;
            foreach(string s in Properties.Settings.Default.Filters)
            {
                tbURLs.AppendText(s+"\n");
            }


            if (Variables.sURL.Length > 5)
            {
                //labURL.Text = Variables.sURL;

                fTestAndUpdateURL();

                canvasCTT.Visibility = Visibility.Visible;
                canvasRegisterBrowser.Visibility = Visibility.Hidden;
                butFF.IsEnabled = MainWindow.IsFirefoxInstalled;
                butGC.IsEnabled = MainWindow.IsChromeInstalled;
            }
            else
            {
                if (!MainWindow.IsElevated)
                {
                    this.BorderThickness = new Thickness(10, 0, 0, 0);
                    this.BorderBrush = SystemParameters.WindowGlassBrush;
                    butRegisterRestart.Tag = GetUACShield();
                    this.Resources["RestartButtonText"] = "Click here to restart as Administrator";
                }
                else
                {
                    this.BorderThickness = new Thickness(10, 0, 0, 0);
                    this.BorderBrush = Brushes.Red;
                }

                canvasCTT.Visibility = Visibility.Hidden;
                canvasRegisterBrowser.Visibility = Visibility.Visible;

                tbURLs.IsEnabled = chkCloseDelay.IsEnabled = butFF.IsEnabled = butGC.IsEnabled = false;
                
            }
            
            Application.Current.Resources["ToggleOn"] = isLightTheme ? Brushes.LightGray : SystemParameters.WindowGlassBrush;
        }

        public string fCheckIfLinkIsOnWhitelist(string sLink, bool bTmp)
        {
            string sReturn = "";
            //if (Variables.lsURLPatterns.Any(x => sLink.ToLower().Contains(x.ToLower()))) { bReturn = true;  }

            foreach (string sTmp in tbURLs.Text.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                if ((sTmp.Length >2) && (sLink.ToLower().Contains(sTmp.ToLower())))
                {
                    sReturn = sTmp;
                }
            }

            return (sReturn);
        }
        private void fTestAndUpdateURL()
        {
            string sTest = fCheckIfLinkIsOnWhitelist(Variables.sURL, true);
            
            richTextBox1.Document.Blocks.Clear();

            if (sTest.Length > 0)
            {
                int iTmp = Variables.sURL.IndexOf(sTest);

                TextRange trBefore = new TextRange(richTextBox1.Document.ContentEnd, richTextBox1.Document.ContentEnd);
                trBefore.Text = Variables.sURL.Substring(0, iTmp);
                trBefore.ApplyPropertyValue(TextElement.ForegroundProperty, isLightTheme ? new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44)):Brushes.Silver );
                trBefore.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);

                TextRange trMatch = new TextRange(richTextBox1.Document.ContentEnd, richTextBox1.Document.ContentEnd);
                trMatch.Text = sTest;
                trMatch.ApplyPropertyValue(TextElement.ForegroundProperty, isLightTheme? Brushes.Black : Brushes.White);
                trMatch.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                trMatch.ApplyPropertyValue(TextElement.FontFamilyProperty, "Segoe UI");

                if (iTmp + sTest.Length <= Variables.sURL.Length)
                {
                    TextRange trAfter = new TextRange(richTextBox1.Document.ContentEnd, richTextBox1.Document.ContentEnd);
                    trAfter.Text = Variables.sURL.Substring(iTmp + sTest.Length);
                    trAfter.ApplyPropertyValue(TextElement.ForegroundProperty, isLightTheme ? new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44)) : Brushes.Silver);
                    trAfter.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);

                }
            }
            else // no match, just display string as-is
            {
                TextRange trURL = new TextRange(richTextBox1.Document.ContentEnd, richTextBox1.Document.ContentEnd);
                trURL.Text = Variables.sURL;
                trURL.ApplyPropertyValue(TextElement.ForegroundProperty, isLightTheme? new SolidColorBrush(Color.FromArgb(0xFF,0x44,0x44,0x44)) : Brushes.Silver);
                trURL.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
                //richTextBox1.AppendText(Variables.sURL);
            }
        }

        private void dtHighlightMatches_Tick(object sender, EventArgs e)
        {
            dtHighlightMatches.Stop();
            fTestAndUpdateURL();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Detect when the theme changed
            HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
            {
                const int WM_SETTINGCHANGE = 0x001A;
                if (msg == WM_SETTINGCHANGE)
                {
                    if (wParam == IntPtr.Zero && Marshal.PtrToStringUni(lParam) == "ImmersiveColorSet")
                    {
                        if (IsLightTheme() != isLightTheme)
                        {
                            SetColors(IsLightTheme());
                            isLightTheme = !isLightTheme;
                        }
                        
                    }
                }

                return IntPtr.Zero;
            });
        }

        public SolidColorBrush brDark { get; set; }
        public SolidColorBrush brDarkLighter { get; set; }
        public SolidColorBrush brLight { get; set; }
        public SolidColorBrush brLightDarker { get; set; }

        public bool SetColors(bool isLight)
        {
            Color colDark = Color.FromArgb(0xff, 0x22, 0x22, 0x22);
            brDark = new SolidColorBrush(colDark);
            brDarkLighter = new SolidColorBrush(Color.FromArgb(0xff, 32, 32, 32));
            Color colLight = Color.FromArgb(0xff, 240, 240, 240);
            brLight = new SolidColorBrush(colLight);
            brLightDarker = new SolidColorBrush(Color.FromArgb(0xff, 200, 200, 200));
            chkCloseDelay.Tag = SystemParameters.WindowGlassBrush;
            this.Background = isLight ? new SolidColorBrush(colLight) : new SolidColorBrush(colDark);
            tbURLs.Foreground = this.Foreground = isLight ? brDarkLighter : brLightDarker;
            this.Resources["CheckFillBrush"] = isLight ? brLight : SystemParameters.WindowGlassBrush;
            tbURLs.Background = isLight ? Brushes.Gainsboro : brDarkLighter;
            //this.Resources["CheckFillBrushTint"] = isLight ? brLight : new Brush(Color.FromArgb(SystemParameters.WindowGlassBrush;
            //labExpLink.Foreground = labExpOO.Foreground = labExpPB.Foreground = isLight ? brDarkLighter : brLightDarker;
            labRegister.Foreground = richTextBox1.Foreground = labCTT.Foreground = labOtherOpts.Foreground = chkCloseDelay.Foreground = label_Browser.Foreground = isLight ? brDark : brLight;

            return isLight;
        }


        private static bool IsLightTheme()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return (value is int i && i > 0);
        }

        private BitmapSource GetUACShield()
        {
            BitmapSource shieldSource = null;

            if (Environment.OSVersion.Version.Major >= 6)
            {
                SHSTOCKICONINFO sii = new SHSTOCKICONINFO();
                sii.cbSize = (UInt32)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

                Marshal.ThrowExceptionForHR(SHGetStockIconInfo(SHSTOCKICONID.SIID_SHIELD,
                    SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON,
                    ref sii));

                shieldSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    sii.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DestroyIcon(sii.hIcon);
            }
            else
            {
                shieldSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    System.Drawing.SystemIcons.Shield.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            return shieldSource;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }

        private void butRegisterRestart_Click(object sender, RoutedEventArgs e)
        {
            string _exepath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            if (!MainWindow.IsElevated)
            {
                var psi = new ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.Verb = "runas";
                psi.FileName = _exepath;
                Process.Start(psi);
                this.DialogResult = false;
                System.Windows.Application.Current.Shutdown();
                //this.Close();
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

                    key.SetValue("ftp", "URL-HandlerWPFURL");
                    key.SetValue("http", "URL-HandlerWPFURL");
                    key.SetValue("https", "URL-HandlerWPFURL");

                    key = Registry.LocalMachine.OpenSubKey(@"Software\RegisteredApplications", true);
                    key.SetValue("URL-HandlerWPF", @"Software\URL-HandlerWPF\Capabilities");

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
                    this.DialogResult = false;
                    System.Windows.Application.Current.Shutdown();
                }
                catch
                {
                }
            }

            butRegisterRestart.Tag = null;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // write out the list of URL filters to the internal variable, and to the Settings.
            Variables.lsURLPatterns.Clear();
            foreach (string s in tbURLs.Text.Split(new String[] { "\r\n","\n" }, StringSplitOptions.None))
            { if (s.Length > 2) { Variables.lsURLPatterns.Add(s); } }

            Properties.Settings.Default.Filters.Clear();
            Properties.Settings.Default.Filters.AddRange(Variables.lsURLPatterns.ToArray());
            
            // write out the value of bAutoClose
            Variables.bAutoClose = Properties.Settings.Default.AutoClose = (true ==chkCloseDelay.IsChecked);

            // write out the default left button browser setting.
            Properties.Settings.Default.Browser = (butFF.IsChecked == true) ? "firefox" : "chrome";

            Properties.Settings.Default.Save();

            Variables.sURL = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd).Text;
            
            if (Variables.sURL.Length <=5) { this.DialogResult = false; } else { this.DialogResult = true; }
        }

        private void chromeClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void labURL_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void butCert_Click(object sender, RoutedEventArgs e)
        {
            if (Variables.sURL.ToLower().StartsWith("https://"))
            {
                try
                {
                    // Do webrequest to get info on secure site
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Variables.sURL);
                    request.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 51.0.2704.103 Safari / 537.36";
                    request.Method = "GET";
                    request.Accept = "text/html";
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    //MessageBox.Show(response);
                    response.Close();

                    // retrieve the ssl cert and assign it to an X509Certificate object
                    X509Certificate cert = request.ServicePoint.Certificate;

                    // convert the X509Certificate to an X509Certificate2 object by passing it into the constructor
                    X509Certificate2 cert2 = new X509Certificate2(cert);

                    string cn = cert2.GetIssuerName();
                    string cedate = cert2.GetExpirationDateString();
                    string cpub = cert2.GetPublicKeyString();

                    // display the cert dialog box
                    X509Certificate2UI.DisplayCertificate(cert2);
                }
                catch { }
            }
        }

        private void butFF_Click(object sender, RoutedEventArgs e)
        {
            RadioButton bTmp = (RadioButton)sender;
            if (bTmp.Name == "butFF")
            {
                MainWindow._Browserpath = MainWindow._Firefoxpath; 
                butGC.IsChecked = false;
                //Application.Current.Resources["leftbuttonimage"] = new BitmapImage(new Uri(@"pack://application:,,,/FF.png"));
            }
            else
            {
                MainWindow._Browserpath = MainWindow._Chromepath;
                butFF.IsChecked = false;
                //Application.Current.Resources["leftbuttonimage"] = new BitmapImage(new Uri(@"pack://application:,,,/Google_Chrome.png"));
            }
            Application.Current.Resources["leftbuttonimage"] = MainWindow._Browserpath == MainWindow._Firefoxpath ? Variables.FFIcon : Variables.GCIcon;
        }

        private void butGC_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbURLs_TextChanged(object sender, TextChangedEventArgs e)
        {
           dtHighlightMatches.Start(); 
        }

        private void richTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            dtHighlightMatches.Stop();
        }

        private void richTextBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            dtHighlightMatches.Stop();
            string sTmp = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd).Text;
            richTextBox1.Document.Blocks.Clear();
            richTextBox1.AppendText(sTmp);
        }
    }
}
