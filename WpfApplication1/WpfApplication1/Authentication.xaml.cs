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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Authentication.xaml
    /// </summary>
    public partial class Authentication : Page
    {
        Client0 mClient;
        int mSz;
        byte[] mBuffer;
        NetSttCode mState;
        bool bSrvrMsg;
        string mSrvrMsg;

        public Authentication()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            mSz = 1024 * 1024;
            mState = NetSttCode.PrepDate;
            mClient = Client0.Instance();
            //Connect(null, null);
            mSrvrMsg = String.Empty;
            bSrvrMsg = false;
        }

        private void Connect(Object source, System.Timers.ElapsedEventArgs e)
        {
            mClient.BeginConnect(CB);
        }

        private void CB(IAsyncResult ar)
        {
            if(ar == null)
            {
                Connect(null, null);
                return;
            }
            if (mState == NetSttCode.PrepDate)
            {
                TcpClient c = (TcpClient)ar.AsyncState;
                if (!c.Connected)
                {
                    //else: wait and connect again
                    System.Timers.Timer aTimer = new System.Timers.Timer(2000);
                    // Hook up the Elapsed event for the timer. 
                    aTimer.Elapsed += Connect;
                    aTimer.AutoReset = false;
                    aTimer.Enabled = true;
                    return;
                }
                NetworkStream s = c.GetStream();
                //char[] msg = new char[1];
                //msg[0] = (char)NetSttCode.Dating;
                //mBuffer = Encoding.UTF8.GetBytes(msg);
                mState = NetSttCode.Dating;
                mBuffer = BitConverter.GetBytes((Int32)mState);
                s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                return;
            }
            if (mState == NetSttCode.Dating)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                s.EndWrite(ar);
                mBuffer = new byte[mSz];
                mState = NetSttCode.Dated;
                s.BeginRead(mBuffer, 0, mSz, CB, s);
                return;
            }
            if (mState == NetSttCode.Dated)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                int r = s.EndRead(ar);
                int offs = 0;
                Date.ReadByteArr(mBuffer, ref offs);
                Dispatcher.Invoke(() => {
                    if(Date.sbArr != null)
                    txtDate.Text = Encoding.UTF32.GetString(Date.sbArr);
                });
                mState = NetSttCode.Dated;
                mClient.Close();//close conn
                return;
            }
            if (mState == NetSttCode.PrepAuth)
            {
                TcpClient c = (TcpClient)ar.AsyncState;
                if (!c.Connected)
                {
                    //else: wait and connect again
                    System.Timers.Timer aTimer = new System.Timers.Timer(2000);
                    // Hook up the Elapsed event for the timer. 
                    aTimer.Elapsed += Connect;
                    aTimer.AutoReset = false;
                    aTimer.Enabled = true;
                    return;
                }
                NetworkStream s = c.GetStream();
                mState = NetSttCode.Authenticating;
                mBuffer = BitConverter.GetBytes((Int32)mState);
                s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                return;
            }
            if (mState == NetSttCode.Authenticating)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                s.EndWrite(ar);
                mBuffer = new byte[mSz];
                mState = NetSttCode.Authenticated;
                s.BeginRead(mBuffer, 0, mSz, CB, s);
                return;
            }
            if (mState == NetSttCode.Authenticated)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                int r = s.EndRead(ar);
                bool auth = false;
                if(mBuffer.Length == 4)
                    auth = BitConverter.ToInt32(mBuffer, 0 ) == 1;
                if (auth)
                {
                    mState = NetSttCode.PrepExamRet;
                    mClient.Close();//close conn
                    mClient.BeginConnect(CB);
                }
                else
                {
                    mState = NetSttCode.Dated;
                    Dispatcher.Invoke(() => { txtMessage.Text += "fail to auth, retry"; });
                    mClient.Close();//close conn
                }
                return;
            }
            if (mState == NetSttCode.PrepExamRet)
            {
                TcpClient c = (TcpClient)ar.AsyncState;
                if (!c.Connected)
                {
                    //else: wait and connect again
                    System.Timers.Timer aTimer = new System.Timers.Timer(2000);
                    // Hook up the Elapsed event for the timer. 
                    aTimer.Elapsed += Connect;
                    aTimer.AutoReset = false;
                    aTimer.Enabled = true;
                    return;
                }
                NetworkStream s = c.GetStream();
                mState = NetSttCode.ExamRetrieving;
                mBuffer = BitConverter.GetBytes((Int32)mState);
                s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                return;
            }
            if (mState == NetSttCode.ExamRetrieving)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                s.EndWrite(ar);
                mBuffer = new byte[mSz];
                mState = NetSttCode.ExamRetrieved;
                s.BeginRead(mBuffer, 0, mSz, CB, s);
                return;
            }
            if (mState == NetSttCode.ExamRetrieved)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                int r = s.EndRead(ar);
                r = 0;
                Question.ReadByteArr(mBuffer, ref r);
                mClient.Close();
                NavigationService.Navigate(new Uri("TakeExam.xaml", UriKind.Relative));
                return;
            }
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            if (mState == NetSttCode.Dated)
            {
                mState = NetSttCode.PrepAuth;
                mClient.BeginConnect(CB);
            }
        }

        private void SignInCallback(IAsyncResult ar)
        {
            NetworkStream s = (NetworkStream)ar.AsyncState;
            s.EndWrite(ar);
            mClient.BeginRead(mBuffer, mSz, QuestReadCallback);
        }

        private void QuestReadCallback(IAsyncResult ar)
        {
            NetworkStream s = (NetworkStream)ar.AsyncState;
            s.EndRead(ar);
            //NavigationService.Navigate(new TakeExam());
            Dispatcher.Invoke(() =>
            {
                NavigationService.Navigate(new Uri("TakeExam.xaml", UriKind.Relative), mBuffer);//must have Urikind
            });
            //txMessage.Text += "\n" + txtUsername.Text + "\n" + txtPassword + "\n";
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = (Window)Parent;
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;

            FirewallHandler fwHndl = new FirewallHandler(3);
            txtMessage.Text += fwHndl.OpenFirewall();

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
            Connect(null, null);
        }
    }
}
