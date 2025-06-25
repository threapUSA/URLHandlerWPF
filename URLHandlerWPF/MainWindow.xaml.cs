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
                System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
                dispatcherTimer.Start();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (bNiceClose)
            {
                Closing -= Window_Closing;
                e.Cancel = true;
                var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(1));
                anim.Completed += (s, _) => this.Close();
                this.BeginAnimation(UIElement.OpacityProperty, anim);
            }
        }
    }
}
