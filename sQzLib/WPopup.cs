using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace sQzLib
{
    public delegate void WPopupCb();

    public class WPopup
    {
        Window mW;
        TextBlock mT;
        TextBox mC;
        WPopupCb _wpCb;
        WPopupCb _wpCbCncl;
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
            get
            {
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

        public WPopupCb wpCbCncl
        {
            set { _wpCbCncl = value; }
        }

        public Window owner
        {
            set
            {
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

        public void Exit()
        {
            cncl = false;
            wpCb = wpCbCncl = null;
            //crash mW.Close();
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
            else
                _wpCbCncl?.Invoke();
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
