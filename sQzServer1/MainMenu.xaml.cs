﻿using System;
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
using sQzLib;

namespace sQzServer1
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        Client2 mClnt;
        string tPw;
        UICbMsg mCbMsg;
        int uRId;
        int uVer = 9;
        public MainMenu()
        {
            InitializeComponent();
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, true);
            tPw = "dummypwd";
            mCbMsg = new UICbMsg();

            uRId = 0;
        }

        private void GetRoomIDFromFile()
        {
            string first_line = Utils.GetFirstNonBlankLine("Room.txt");
            if (!int.TryParse(first_line, out uRId))
                uRId = 0;
        }

        public byte[] ClntBufPrep()
        {
            GetRoomIDFromFile();
            byte[] outMsg = new byte[16];
            Array.Copy(BitConverter.GetBytes((int)NetCode.Srvr1Auth), 0, outMsg, 0, 4);
            Array.Copy(BitConverter.GetBytes(uRId), 0, outMsg, 4, 4);
            Array.Copy(Encoding.ASCII.GetBytes(tPw), 0, outMsg, 8, 8);
            return outMsg;
        }

        public bool ClntBufHndl(byte[] buf)
        {
            int offs = 0;
            if (buf.Length - offs < 4)
            {
                MessageBox.Show("Error data!");
                return false;
            }
            int rs = BitConverter.ToInt32(buf, offs);
            offs += 4;
            if (rs != (int)TxI.OP_AUTH_OK)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    WPopup.s.ShowDialog(Txt.s._((int)TxI.OP_AUTH_NOK));
                });
                return false;
            }
            if(buf.Length - offs < sizeof(long))
            {
                MessageBox.Show("Error data!");
                return false;
            }
            TimeSpan testDuration = new TimeSpan(BitConverter.ToInt64(buf, offs));
            offs += sizeof(long);

            string subject = Utils.ReadBytesOfString(buf, ref offs);
            if (subject == null)
                subject = string.Empty;
            Dispatcher.InvokeAsync(() =>
            {
                Page op1 = new Operation1(testDuration, subject);
                NavigationService.Navigate(op1);
            });   
            return false;
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            txtLalgitc.Text = t._((int)TxI.LALGITC);
            txtsQz.Text = t._((int)TxI.SQZ) + " " + t._((int)TxI.UPPER_CASE_ROOM) + uRId;
            txtPw.Text = t._((int)TxI.OP_PW);
            btnAuth.Content = t._((int)TxI.OP_AUTH);
            btnExit.Content = t._((int)TxI.EXIT);
        }

        private void btnAuth_Click(object sender, RoutedEventArgs e)
        {
            if (tbxPw.Text.Length != 8)
            {
                WPopup.s.ShowDialog(Txt.s._((int)TxI.OP_PW_NOK));
                return;
            }
            tPw = tbxPw.Text;
            Task.Run(() => { mClnt.ConnectWR(ref mCbMsg); });
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Application.Current.MainWindow;
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 28;

            WPopup.s.owner = w;

            GetRoomIDFromFile();

            LoadTxt();
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WPopup.s.Exit();
        }
    }
}
