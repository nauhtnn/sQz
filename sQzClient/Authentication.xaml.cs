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
using System.Threading;
using System.Net.Sockets;
using sQzLib;

namespace sQzClient
{
    /// <summary>
    /// Interaction logic for Authentication.xaml
    /// </summary>
    public partial class Authentication : Page
    {
        Client2 mClient;
        int mSz;
        byte[] mBuffer;
        NetCode mState;
        UICbMsg mCbMsg;
        public Authentication()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            mSz = 1024 * 1024;
            //mState = NetCode.PrepDate;
            mState = NetCode.Dating;
            mClient = new Client2(CliBufHndl, CliBufPrep);
            mCbMsg = new UICbMsg();

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void Connect(Object source, System.Timers.ElapsedEventArgs e)
        {
            Thread th = new Thread(() => { mClient.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            //mState = NetCode.PrepAuth;
            Thread th = new Thread(() => { mClient.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = (Window)Parent;
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;

            double rt = w.RenderSize.Width / 640; //design size
            ScaleTransform st = new ScaleTransform(rt, rt);
            gMain.RenderTransform = st;

            FirewallHandler fwHndl = new FirewallHandler(3);
            lblStatus.Text += fwHndl.OpenFirewall();

            Connect(null, null);
        }

        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            tbxNeeId.Text = String.Empty;
            if (tbxNeeId.Text == "type Id" ||
                !System.Text.RegularExpressions.Regex.Match(tbxNeeId.Text, "[a-zA-Z0-9]").Success)
                tbxNeeId.Text = String.Empty;
        }

        private void btnReconn_Click(object sender, RoutedEventArgs e)
        {
        }

        public bool CliBufHndl(byte[] buf, int offs)
        {
            switch (mState)
            {
                //case NetCode.Dated:
                case NetCode.Dating:
                    Date.ReadByteArr(buf, ref offs, buf.Length);
                    Dispatcher.Invoke(() => {
                        if (Date.sbArr != null)
                            txtDate.Text = Encoding.UTF32.GetString(Date.sbArr);
                    });
                    //mState = NetCode.PrepAuth;
                    mState = NetCode.Authenticating;
                    return false;
                case NetCode.Authenticating:
                    bool auth = false;
                    if (buf.Length == 4)
                        auth = BitConverter.ToInt32(buf, 0) == 1;
                    if(!auth)
                    {
                        //mState = NetCode.Dated;
                        Dispatcher.Invoke(() => { lblStatus.Text += "fail to auth, retry"; });
                        return false;
                    }
                    //mState = NetCode.Authenticated;
                    mState = NetCode.ExamRetrieving;
                    break;
                //case NetCode.ExamRetrieved:
                case NetCode.ExamRetrieving:
                    offs = 0;
                    Question.ReadByteArr(buf, ref offs, buf.Length, false);
                    Dispatcher.Invoke(() =>
                    {
                        NavigationService.Navigate(new Uri("TakeExam.xaml", UriKind.Relative));
                    });
                    return false;
            }
            return true;
        }

        public bool CliBufPrep(ref byte[] outBuf)
        {
            switch (mState)
            {
                //case NetCode.PrepDate:
                //    mState = NetCode.Dating;
                case NetCode.Dating:
                    outBuf = BitConverter.GetBytes((int)mState);
                    //mState = NetCode.Dated;
                    break;
                //case NetCode.PrepAuth:
                //    mState = NetCode.Authenticating;
                case NetCode.Authenticating:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                //case NetCode.Authenticated:
                //    mState = NetCode.ExamRetrieving;
                case NetCode.ExamRetrieving:
                    outBuf = BitConverter.GetBytes((int)mState);
                    mState = NetCode.ExamRetrieved;
                    break;
            }
            return true;
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (mCbMsg.ToUp())
                Dispatcher.Invoke(() => {
                    lblStatus.Text += mCbMsg.txt;
                });
        }
    }
}
