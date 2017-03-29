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

namespace sQzLib
{
    /// <summary>
    /// Interaction logic for Theme.xaml
    /// </summary>
    public partial class Theme : UserControl
    {
        public static SolidColorBrush[] vBrush;
        public static SolidColorBrush[][] vTheme;

        static bool bBrushReady = false;

        public static void InitBrush()
        {
            if (bBrushReady)
                return;
            bBrushReady = true;
            vTheme = new SolidColorBrush[(int)ThemeId.Count][];
            //Harvard theme
            SolidColorBrush[] br = new SolidColorBrush[(int)BrushId.Count];
            Color c = new Color();
            c.A = 0xff;
            c.R = 0xc3;
            c.G = 0xd7;
            c.B = 0xa4;
            br[(int)BrushId.BG] = new SolidColorBrush(c);
            c.R = 0xa5;
            c.G = 0x1c;
            c.B = 0x30;
            br[(int)BrushId.QID_BG] = new SolidColorBrush(c);
            c.R = c.G = c.B = 0xff;
            br[(int)BrushId.QID_Color] = new SolidColorBrush(c);
            c.R = c.G = c.B = 0xee;
            br[(int)BrushId.Q_BG] = new SolidColorBrush(c);
            c.R = 0xd8;
            c.G = 0x70;
            c.B = 0xb8;
            br[(int)BrushId.Ans_TopLine] = new SolidColorBrush(c);
            c.R = 0x58;
            c.G = 0xa9;
            c.B = 0xb4;
            br[(int)BrushId.Ans_Highlight] = new SolidColorBrush(c);
            c.R = c.G = 0xff;
            c.B = 0xbb;
            br[(int)BrushId.LeftPanel_BG] = new SolidColorBrush(c);
            c.B = 0xdd;
            br[(int)BrushId.Sheet_BG] = new SolidColorBrush(c);
            c.R = 0xf1;
            c.G = 0x5a;
            c.B = 0x23;
            br[(int)BrushId.Button_Hover] = new SolidColorBrush(c);
            vTheme[(int)ThemeId.Harvard] = br;
            vBrush = vTheme[(int)ThemeId.Harvard];
        }

        public Theme()
        {
            InitializeComponent();
        }
    }

    public class WPopup
    {
        Window w;
        TextBlock t;
        public static WPopup _s;
        WPopup()
        {
            w = new Window();
            w.Title = Txt.s._[(int)TxI.POPUP_TIT];
            w.Closing += wPopup_Closing;
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            t = new TextBlock();
            
            t.VerticalAlignment = VerticalAlignment.Center;
            t.TextWrapping = TextWrapping.Wrap;
            t.TextAlignment = TextAlignment.Center;
            Color c = new Color();
            c.R = 0x58;
            c.G = 0xa9;
            c.B = 0xb4;
            t.Background = new SolidColorBrush(c);
            w.Content = t;
        }

        static WPopup s
        {
            get {
                if (_s == null)
                    _s = new WPopup();
                return _s;
            }
        }

        public static void Config(Window p, double fsz)
        {
            WPopup i = s;
            if (p != null)
            {
                i.w.Owner = p;
                s.w.Width = p.RenderSize.Width / 4;
                s.w.Height = p.RenderSize.Height / 4;
                s.w.ResizeMode = ResizeMode.NoResize;
            }
            if (0 < fsz)
                i.t.FontSize = fsz;
        }

        public static void ShowDialog(string msg)
        {
            s.t.Text = msg;
            s.w.ShowDialog();
        }

        private void wPopup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window w = sender as Window;
            w.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }
    }
}
