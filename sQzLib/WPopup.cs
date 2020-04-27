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
        TextBox UserCode;
        public WPopupCb CbOK;
        public WPopupCb CbNOK;
        static WPopup _s;
        Button OK;
        Button NOK;
        Grid mG;
        public bool IsOK;
        bool IsShowed;
        //bool IsCollapse;
        bool DoNotClose;
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

            UserCode = new TextBox();
            UserCode.TextAlignment = TextAlignment.Center;
            Grid.SetRow(UserCode, 1);
            Grid.SetColumnSpan(UserCode, 2);
            mG.Children.Add(UserCode);

            OK = new Button();
            OK.Click += BtnOk_Click;
            Grid.SetRow(OK, 2);
            mG.Children.Add(OK);

            NOK = new Button();
            NOK.Click += BtnCncl_Click;
            Grid.SetRow(NOK, 2);
            Grid.SetColumn(NOK, 1);
            mG.Children.Add(NOK);

            mW.Content = mG;

            //IsOK = false;
            IsShowed = false;
            //IsCollapse = true;
            DoNotClose = true;
            mCode = null;
        }

        private void BtnCncl_Click(object sender, RoutedEventArgs e)
        {
            CbNOK?.Invoke();
            mW.Visibility = Visibility.Collapsed;
            //IsCollapse = true;
            IsShowed = false;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (mCode == null || mCode == UserCode.Text)
            {
                UserCode.Text = string.Empty;
                //IsOK = true;
                CbOK?.Invoke();
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
            DoNotClose = false;
            CbOK = CbNOK = null;
            mW.Close();
        }

        public void ShowDialog(string msg)
        {
            mT.Text = msg;
            OK.Visibility = Visibility.Collapsed;
            NOK.Visibility = Visibility.Collapsed;
            //if (IsShowed)
            //{
            //    IsCollapse = false;
            //    return;
            //}
            //IsShowed = true;
            mW.ShowDialog();
        }

        public void ShowDialog(string msg, string ok, string cncl, string code)
        {
            mT.Text = msg;
            OK.Content = ok;
            OK.Visibility = Visibility.Visible;
            NOK.Content = cncl;
            NOK.Visibility = Visibility.Visible;
            mCode = code;
            //if (IsShowed)
            //{
            //    IsCollapse = false;
            //    return;
            //}
            //IsShowed = true;
            mW.ShowDialog();
        }

        private void wPopup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DoNotClose)
            {
                e.Cancel = true;
                BtnCncl_Click(null, null);
            }
        }
    }
}
