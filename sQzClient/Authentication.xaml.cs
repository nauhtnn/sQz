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
        int nBusy;//crash fixed: only call if not busy
        bool bToDispose;//crash fixed: flag to dispose
        bool bReconn;//reconnect after callback
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
            nBusy = 0;
            bToDispose = false;
            bReconn = false;
        }

        private void Connect(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (0 < nBusy)
                return;
            ++nBusy;
            mClient.BeginConnect(CB);
        }

        private void CB(IAsyncResult ar)
        {
            NetworkStream s = null;
            TcpClient c = null;
            int r = 0;
            switch (mState) {
                case NetSttCode.PrepDate:
                    c = (TcpClient)ar.AsyncState;
                    if (!c.Connected)
                        break;
                    s = c.GetStream();
                    mState = NetSttCode.Dating;
                    mBuffer = BitConverter.GetBytes((Int32)mState);
                    ++nBusy;
                    s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                    break;
                case NetSttCode.Dating:
                    s = (NetworkStream)ar.AsyncState;
                    s.EndWrite(ar);
                    mBuffer = new byte[mSz];
                    mState = NetSttCode.Dated;
                    ++nBusy;
                    s.BeginRead(mBuffer, 0, mSz, CB, s);
                    break;
                case NetSttCode.Dated:
                    s = (NetworkStream)ar.AsyncState;
                    r = s.EndRead(ar);
                    int offs = 0;
                    Date.ReadByteArr(mBuffer, ref offs);
                    Dispatcher.Invoke(() => {
                        if (Date.sbArr != null)
                            txtDate.Text = Encoding.UTF32.GetString(Date.sbArr);
                    });
                    //mState = NetSttCode.PrepAuth;
                    //mClient.Close();//close conn
                    //break;
                    //case NetSttCode.PrepAuth:
                    //    c = (TcpClient)ar.AsyncState;
                    //    if (!c.Connected)
                    //        break;
                    //    s = c.GetStream();
                    mState = NetSttCode.Authenticating;
                    mBuffer = BitConverter.GetBytes((Int32)mState);
                    ++nBusy;
                    s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                    break;
                case NetSttCode.Authenticating:
                    s = (NetworkStream)ar.AsyncState;
                    s.EndWrite(ar);
                    mBuffer = new byte[mSz];
                    mState = NetSttCode.Authenticated;
                    ++nBusy;
                    s.BeginRead(mBuffer, 0, mSz, CB, s);
                    break;
                case NetSttCode.Authenticated:
                    s = (NetworkStream)ar.AsyncState;
                    r = s.EndRead(ar);
                    bool auth = false;
                    if (mBuffer.Length == 4)
                        auth = BitConverter.ToInt32(mBuffer, 0) == 1;
                    if (auth)
                    {
                        //mState = NetSttCode.PrepExamRet;
                        //bReconn = true;
                    }
                    else
                    {
                        mState = NetSttCode.Dated;
                        Dispatcher.Invoke(() => { txtMessage.Text += "fail to auth, retry"; });
                        bToDispose = true;
                        break;
                    }
                //    break;
                //case NetSttCode.PrepExamRet:
                //    c = (TcpClient)ar.AsyncState;
                //    if (!c.Connected)
                //        break;
                //    s = c.GetStream();
                    mState = NetSttCode.ExamRetrieving;
                    mBuffer = BitConverter.GetBytes((Int32)mState);
                    ++nBusy;
                    s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                    break;
                case NetSttCode.ExamRetrieving:
                    s = (NetworkStream)ar.AsyncState;
                    s.EndWrite(ar);
                    mBuffer = new byte[mSz];
                    mState = NetSttCode.ExamRetrieved;
                    ++nBusy;
                    s.BeginRead(mBuffer, 0, mSz, CB, s);
                    break;
                case NetSttCode.ExamRetrieved:
                    s = (NetworkStream)ar.AsyncState;
                    r = s.EndRead(ar);
                    r = 0;
                    Question.ReadByteArr(mBuffer, ref r);
                    mClient.Close();
                    NavigationService.Navigate(new Uri("TakeExam.xaml", UriKind.Relative));
                    break;
            }

            --nBusy;
            if (bToDispose && nBusy == 0)
            {
                mClient.Close();
                bToDispose = false;
            }
            else if (bReconn && nBusy == 0)
            {
                ++nBusy;
                mClient.BeginConnect(CB);
            }
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            if (mState == NetSttCode.Dated)
            {
                //mState = NetSttCode.PrepAuth;
                mState = NetSttCode.Dated;
                ++nBusy;
                mClient.BeginConnect(CB);
            }
        }

        //private void SignInCallback(IAsyncResult ar)
        //{
        //    NetworkStream s = (NetworkStream)ar.AsyncState;
        //    s.EndWrite(ar);
        //    //fair -- ++
        //    mClient.BeginRead(mBuffer, mSz, QuestReadCallback);
        //}

        //private void QuestReadCallback(IAsyncResult ar)
        //{
        //    NetworkStream s = (NetworkStream)ar.AsyncState;
        //    s.EndRead(ar);
        //    //NavigationService.Navigate(new TakeExam());
        //    Dispatcher.Invoke(() =>
        //    {
        //        NavigationService.Navigate(new Uri("TakeExam.xaml", UriKind.Relative), mBuffer);//must have Urikind
        //    });
        //    //txMessage.Text += "\n" + txtUsername.Text + "\n" + txtPassword + "\n";
        //}

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
            if (0 < nBusy)
                return;
            ++nBusy;
            Connect(null, null);
        }
    }
}
