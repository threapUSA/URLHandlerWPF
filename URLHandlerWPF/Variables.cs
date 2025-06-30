using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;


namespace URLHandlerWPF
{
    public static class Variables
    {
        public static string sURL = "";
        public static bool bAutoClose = false;
        public static bool bFavorEdge = false;
        //public static bool isLightTheme = true;
        public static List<string> lsURLPatterns;
        public static ImageSource FFIcon = new BitmapImage(new Uri(@"pack://application:,,,/FF.png"));
        public static ImageSource GCIcon = new BitmapImage(new Uri(@"pack://application:,,,/Google_Chrome.png"));
    }
}
