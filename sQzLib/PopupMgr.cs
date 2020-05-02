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
    public delegate void PopupMgrCb();

    public class PopupMgr
    {
        Window TheWindow;
        TextBlock Message;
        TextBox AdminSecretCode;
        public PopupMgrCb CbOK;
        public PopupMgrCb CbNOK;
        static PopupMgr _instance;
        Button OK;
        Button NOK;
        Grid TheGrid;
        public bool IsOK;
        bool IsShowed;
        //bool IsCollapse;
        bool DoNotClose;
        string mCode;
        PopupMgr()
        {
            TheWindow = new Window();
            TheWindow.Title = Txt.s._[(int)TxI.POPUP_TIT];
            TheWindow.Closing += wPopup_Closing;
            TheWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            TheWindow.ResizeMode = ResizeMode.NoResize;

            TheGrid = new Grid();
            RowDefinition rd = new RowDefinition();
            TheGrid.RowDefinitions.Add(rd);
            rd = new RowDefinition();
            TheGrid.RowDefinitions.Add(rd);
            rd = new RowDefinition();
            TheGrid.RowDefinitions.Add(rd);
            ColumnDefinition cd = new ColumnDefinition();
            TheGrid.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            TheGrid.ColumnDefinitions.Add(cd);

            Message = new TextBlock();
            Message.VerticalAlignment = VerticalAlignment.Center;
            Message.TextWrapping = TextWrapping.Wrap;
            Message.TextAlignment = TextAlignment.Center;
            Color c = new Color();
            c.R = 0x58;
            c.G = 0xa9;
            c.B = 0xb4;
            Message.Background = new SolidColorBrush(c);
            Grid.SetColumnSpan(Message, 2);
            TheGrid.Children.Add(Message);

            AdminSecretCode = new TextBox();
            AdminSecretCode.TextAlignment = TextAlignment.Center;
            Grid.SetRow(AdminSecretCode, 1);
            Grid.SetColumnSpan(AdminSecretCode, 2);
            TheGrid.Children.Add(AdminSecretCode);

            OK = new Button();
            OK.Click += BtnOk_Click;
            Grid.SetRow(OK, 2);
            TheGrid.Children.Add(OK);

            NOK = new Button();
            NOK.Click += BtnCncl_Click;
            Grid.SetRow(NOK, 2);
            Grid.SetColumn(NOK, 1);
            TheGrid.Children.Add(NOK);

            TheWindow.Content = TheGrid;

            //IsOK = false;
            IsShowed = false;
            //IsCollapse = true;
            DoNotClose = true;
            mCode = null;
        }

        private void BtnCncl_Click(object sender, RoutedEventArgs e)
        {
            CbNOK?.Invoke();
            TheWindow.Visibility = Visibility.Collapsed;
            //IsCollapse = true;
            IsShowed = false;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (mCode == null || mCode == AdminSecretCode.Text)
            {
                AdminSecretCode.Text = string.Empty;
                //IsOK = true;
                CbOK?.Invoke();
            }
        }

        public static PopupMgr Singleton
        {
            get
            {
                if (_instance == null)
                    _instance = new PopupMgr();
                return _instance;
            }
        }

        public static void SetParentWindow(Window owner)
        {
            _instance = new PopupMgr();
            _instance.ParentWindow = owner;
        }

        public Window ParentWindow
        {
            set
            {
                if (value != null)
                {
                    TheWindow.Owner = value;
                    TheWindow.Width = ((0.1 < value.RenderSize.Width) ? value.RenderSize.Width :
                        SystemParameters.PrimaryScreenWidth) / 2;
                    TheWindow.Height = ((0.1 < value.RenderSize.Height) ? value.RenderSize.Height :
                        SystemParameters.PrimaryScreenHeight) / 2;
                    TheGrid.RowDefinitions[2].Height = TheGrid.RowDefinitions[1].Height = new GridLength(TheWindow.Height / 6);
                    if (0 < value.FontSize)
                        Message.FontSize = value.FontSize * 1.2;
                }
            }
        }

        public void Exit()
        {
            DoNotClose = false;
            CbOK = CbNOK = null;
            TheWindow.Close();
        }

        public void ShowDialog(string msg)
        {
            Message.Text = msg;
            OK.Visibility = Visibility.Collapsed;
            NOK.Visibility = Visibility.Collapsed;
            //if (IsShowed)
            //{
            //    IsCollapse = false;
            //    return;
            //}
            //IsShowed = true;
            TheWindow.ShowDialog();
        }

        public void ShowDialog(string msg, string ok, string cncl, string code)
        {
            Message.Text = msg;
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
            TheWindow.ShowDialog();
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
