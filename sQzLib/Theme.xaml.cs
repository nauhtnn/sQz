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
    //from left to right
    public enum BrushId
    {
        BG = 0,
        LeftPanel_BG,
        Sheet_BG,
        Button_Hover,
        Q_BG,
        QID_BG,
        QID_Color,
        Ans_TopLine,
        Ans_Highlight,
        Count
    }

    public enum ThicknessId
    {
        LT = 0,
        MT,
        RT,
        LB,
        MB,
        RB,
        Count
    }

    public enum ThemeId
    {
        Harvard = 0,
        Berkeley,
        Count
    }
    /// <summary>
    /// Interaction logic for Theme.xaml
    /// </summary>
    public partial class Theme : UserControl
    {
        public SolidColorBrush[] _;
        public Thickness[] l;
        SolidColorBrush[][] vTheme;
        static Theme _s;
        public static Theme s
        {
            get {
                if (_s == null)
                    _s = new Theme();
                return _s;
            }
        }

        public Theme()
        {
            //InitializeComponent();
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
            _ = vTheme[(int)ThemeId.Harvard];
            
            l = new Thickness[(int)ThicknessId.Count];
            l[(int)ThicknessId.LT] = new Thickness(0, 0, 0, 0);
            l[(int)ThicknessId.MT] = new Thickness(1, 1, 0, 0);
            l[(int)ThicknessId.RT] = new Thickness(1, 1, 1, 0);
            l[(int)ThicknessId.LB] = new Thickness(1, 1, 0, 1);
            l[(int)ThicknessId.MB] = new Thickness(1, 1, 0, 1);
            l[(int)ThicknessId.RB] = new Thickness(1, 1, 1, 1);
        }
    }

    public delegate void WPopupCb();

    public class WPopup
    {
        Window w;
        TextBlock t;
        WPopupCb _wpCb;
        bool bCncl;
        static WPopup _s;
        WPopup()
        {
            w = new Window();
            w.Title = Txt.s._[(int)TxI.POPUP_TIT];
            w.Closing += wPopup_Closing;
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.ResizeMode = ResizeMode.NoResize;
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
            bCncl = true;

            _wpCb = null;
        }

        public static WPopup s
        {
            get {
                if (_s == null)
                    _s = new WPopup();
                return _s;
            }
        }

        public WPopupCb wpCb
        {
            set { _wpCb = value; }
        }

        public Window owner
        {
            set {
                if (value != null)
                {
                    w.Owner = value;
                    w.Width = ((0.1 < value.RenderSize.Width) ? value.RenderSize.Width :
                        SystemParameters.PrimaryScreenWidth) / 3;
                    w.Height = ((0.1 < value.RenderSize.Height) ? value.RenderSize.Height :
                        SystemParameters.PrimaryScreenHeight) / 3;
                    if (0 < value.FontSize)
                        t.FontSize = value.FontSize;
                }
            }
        }

        public bool cncl { set { bCncl = value; } }

        public void ShowDialog(string msg)
        {
            t.Text = msg;
            w.UpdateLayout();
            w.ShowDialog();
        }

        private void wPopup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window s = sender as Window;
            s.Visibility = Visibility.Collapsed;
            if (bCncl)
            {
                e.Cancel = true;
                _wpCb?.Invoke();
            }
        }
    }
}
