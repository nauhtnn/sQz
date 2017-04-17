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
        Window mW;
        TextBlock mT;
        TextBox mC;
        WPopupCb _wpCb;
        static WPopup _s;
        Button mBtnOk;
        Button mBtnCncl;
        Grid mG;
        bool bOk;
        bool bShowing;
        bool bCollapse;
        bool bCnclEvnt;
        string mCode;
        WPopup()
        {
            mW = new Window();
            mW.Title = Txt.s._[(int)TxI.POPUP_TIT];
            mW.Closing += wPopup_Closing;
            mW.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mW.ResizeMode = ResizeMode.NoResize;

            mG = new Grid();
            RowDefinition rd = new RowDefinition();
            mG.RowDefinitions.Add(rd);
            rd = new RowDefinition();
            mG.RowDefinitions.Add(rd);
            rd = new RowDefinition();
            mG.RowDefinitions.Add(rd);
            ColumnDefinition cd = new ColumnDefinition();
            mG.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            mG.ColumnDefinitions.Add(cd);

            mT = new TextBlock();
            mT.VerticalAlignment = VerticalAlignment.Center;
            mT.TextWrapping = TextWrapping.Wrap;
            mT.TextAlignment = TextAlignment.Center;
            Color c = new Color();
            c.R = 0x58;
            c.G = 0xa9;
            c.B = 0xb4;
            mT.Background = new SolidColorBrush(c);
            Grid.SetColumnSpan(mT, 2);
            mG.Children.Add(mT);

            mC = new TextBox();
            mC.TextAlignment = TextAlignment.Center;
            Grid.SetRow(mC, 1);
            Grid.SetColumnSpan(mC, 2);
            mG.Children.Add(mC);

            mBtnOk = new Button();
            mBtnOk.Click += BtnOk_Click;
            Grid.SetRow(mBtnOk, 2);
            mG.Children.Add(mBtnOk);

            mBtnCncl = new Button();
            mBtnCncl.Click += BtnCncl_Click;
            Grid.SetRow(mBtnCncl, 2);
            Grid.SetColumn(mBtnCncl, 1);
            mG.Children.Add(mBtnCncl);

            mW.Content = mG;

            bOk = false;
            bShowing = false;
            bCollapse = true;
            bCnclEvnt = true;
            mCode = null;

            _wpCb = null;
        }

        private void BtnCncl_Click(object sender, RoutedEventArgs e)
        {
            mW.Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (mCode == null || mCode == mC.Text)
            {
                mC.Text = string.Empty;
                bOk = true;
                mW.Close();
            }
        }

        public static WPopup s
        {
            get {
                if (_s == null)
                    _s = new WPopup();
                return _s;
            }
        }

        public static void nwIns(Window oner)
        {
            _s = new WPopup();
            _s.owner = oner;
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
                    mW.Owner = value;
                    mW.Width = ((0.1 < value.RenderSize.Width) ? value.RenderSize.Width :
                        SystemParameters.PrimaryScreenWidth) / 2;
                    mW.Height = ((0.1 < value.RenderSize.Height) ? value.RenderSize.Height :
                        SystemParameters.PrimaryScreenHeight) / 2;
                    mG.RowDefinitions[2].Height = mG.RowDefinitions[1].Height = new GridLength(mW.Height / 6);
                    if (0 < value.FontSize)
                        mT.FontSize = value.FontSize * 1.2;
                }
            }
        }

        public bool cncl { set { bCnclEvnt = value; } }

        public void ShowDialog(string msg)
        {
            mT.Text = msg;
            mBtnOk.Visibility = Visibility.Collapsed;
            mBtnCncl.Visibility = Visibility.Collapsed;
            bOk = true;
            if (bShowing)
            {
                bCollapse = false;
                return;
            }
            bShowing = true;
            mW.ShowDialog();
        }

        public void ShowDialog(string msg, string ok, string cncl, string code)
        {
            mT.Text = msg;
            mBtnOk.Content = ok;
            mBtnOk.Visibility = Visibility.Visible;
            mBtnCncl.Content = cncl;
            mBtnCncl.Visibility = Visibility.Visible;
            mCode = code;
            if (bShowing)
            {
                bCollapse = false;
                return;
            }
            bShowing = true;
            mW.ShowDialog();
        }

        private void wPopup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (bCnclEvnt)
                e.Cancel = true;
            if (bOk)
            {
                _wpCb?.Invoke();
                bOk = false;
            }
            if (bCollapse)
            {
                Window s = sender as Window;
                s.Visibility = Visibility.Collapsed;
                bShowing = false;
            }
            else
                bCollapse = true;
        }
    }
}
