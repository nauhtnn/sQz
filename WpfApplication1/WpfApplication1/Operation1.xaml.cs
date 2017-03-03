using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Operation1.xaml
    /// </summary>
    public partial class Operation1 : Page
    {
        Client0 mClient;
        int mSz;
        byte[] mBuffer;
        RequestCode mState;
        string mDate;
        string zQuest;
        bool mSrvrConn;
        Server1 mServer;
        bool bSrvrMsg;
        string mSrvrMsg;

        public Operation1()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            //FirewallHandler fwHndl = new FirewallHandler(0);
            //fwHndl.OpenFirewall();
            mSz = 1024 * 1024;
            mState = RequestCode.PrepDateStudent;
            mClient = Client0.Instance();
            mClient.SetSrvrPort(23820);
            mServer = new Server1(ResponseMsg);
            bSrvrMsg = false;
            mSrvrMsg = String.Empty;

            mDate = String.Empty;
            mSrvrConn = true;

            TakeExam.InitBrush();

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        public string ResponseMsg(char code)
        {
            string msg = null;
            switch (code)
            {
                case (char)RequestCode.Dating:
                    msg = mDate;
                    break;
                case (char)RequestCode.Authenticating:
                    msg = zQuest;
                    break;
                case (char)RequestCode.ExamRetrieving:
                    break;
                case (char)RequestCode.Submiting:
                    break;
                default:
                    msg = "unknown";
                    break;
            }
            return msg;
        }

        private void ScaleScreen(double r)
        {
            lblStatus.Height = lblStatus.Height * r;
            lblStatus.Width = lblStatus.Width * r;
            svwrStudent.Height = svwrStudent.Height * r;
            svwrStudent.Background = new SolidColorBrush(Colors.AliceBlue);

            txtDate.FontSize = TakeExam.em;
        }

        private void spMain_Loaded(object sender, RoutedEventArgs e)
        {
            spMain.Background = TakeExam.vBrush[(int)BrushId.BG];
            Window w = (Window)Parent;
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.Closing += W_Closing;

            double scaleW = spMain.RenderSize.Width / 640; //d:DesignWidth
            //double scaleH = spMain.RenderSize.Height / 360; //d:DesignHeight
            ScaleScreen(scaleW);

            FirewallHandler fwHndl = new FirewallHandler(1);
            string msg = fwHndl.OpenFirewall();
            lblStatus.Text = msg;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            mSrvrConn = true;
            mClient.BeginConnect(CB);
        }

        private void CB(IAsyncResult ar)
        {
            if (ar == null)
            {
                //btnConnect_Click(null, null);
                return;
            }
            if (mState == RequestCode.PrepDateStudent)
            {
                TcpClient c = (TcpClient)ar.AsyncState;
                //exception: c.EndConnect(ar);
                if (!c.Connected)
                {
                    //else: wait and connect again
                    //System.Timers.Timer aTimer = new System.Timers.Timer(2000);
                    //// Hook up the Elapsed event for the timer. 
                    //aTimer.Elapsed += Connect;
                    //aTimer.AutoReset = false;
                    //aTimer.Enabled = true;
                    return;
                }
                NetworkStream s = c.GetStream();
                char[] msg = new char[1];
                msg[0] = (char)RequestCode.DateStudentRetriving;
                mBuffer = Encoding.UTF8.GetBytes(msg);
                mState = RequestCode.DateStudentRetriving;
                s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                return;
            }
            if (mState == RequestCode.DateStudentRetriving)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                mBuffer = new byte[mSz];
                mState = RequestCode.DateStudentRetrieved;
                s.BeginRead(mBuffer, 0, mSz, CB, s);
                return;
            }
            if (mState == RequestCode.DateStudentRetrieved)
            {
                int nullIdx = Array.IndexOf(mBuffer, 0);
                nullIdx = nullIdx >= 0 ? nullIdx : mBuffer.Length;
                string dat = UTF8Encoding.UTF8.GetString(mBuffer, 0, nullIdx);
                dat = dat.Substring(0, dat.IndexOf('\0'));
                Dispatcher.Invoke(() => {
                    int idx1 = dat.IndexOf('\n');
                    mDate = dat.Substring(0, idx1++);
                    txtDate.Text = mDate;
                    int idx2 = dat.IndexOf('\n', idx1);
                    while (idx2 != -1) //not check ends with '\n' here
                    {
                        string t = dat.Substring(idx1, idx2 - idx1);
                        TextBlock x = new TextBlock();
                        x.FontSize = TakeExam.em;
                        x.Text = t;
                        spStudent.Children.Add(x);
                        idx1 = ++idx2;
                        idx2 = dat.IndexOf('\n', idx2);
                    }
                });
                char[] msg = new char[1];
                msg[0] = (char)RequestCode.QuestAnsKeyRetrieving;
                mBuffer = Encoding.UTF8.GetBytes(msg);
                mState = RequestCode.PrepQuestAnsKey;
                //reconnect
                btnDisconnect_Click(null, null);
                btnConnect_Click(null, null);
                return;
            }
            if (mState == RequestCode.PrepQuestAnsKey)
            {
                TcpClient c = (TcpClient)ar.AsyncState;
                if (c.Connected)
                {
                    NetworkStream s = (NetworkStream)c.GetStream();
                    char[] msg = new char[1];
                    msg[0] = (char)RequestCode.QuestAnsKeyRetrieving;
                    mBuffer = Encoding.UTF8.GetBytes(msg);
                    mState = RequestCode.QuestAnsKeyRetrieving;
                    s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                }
                return;
            }
            if (mState == RequestCode.QuestAnsKeyRetrieving)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                mBuffer = new byte[mSz];
                mState = RequestCode.QuestAnsKeyRetrieved;
                s.BeginRead(mBuffer, 0, mSz, CB, s);
                return;
            }
            if (mState == RequestCode.QuestAnsKeyRetrieved)
            {
                int nullIdx = Array.IndexOf(mBuffer, 0);
                nullIdx = nullIdx >= 0 ? nullIdx : mBuffer.Length;
                zQuest = UTF8Encoding.UTF8.GetString(mBuffer, 0, nullIdx);
                zQuest = zQuest.Substring(0, zQuest.IndexOf('\0'));
                Dispatcher.Invoke(() => {
                    TextBlock t = new TextBlock();
                    t.Text = "QuestAnsKey recv len = " + zQuest.Length + " last = " +
                        zQuest.Substring((int)(zQuest.Length * 0.9));
                    spStudent.Children.Add(t);
                });
                mState = RequestCode.PrepMark;
            }
        }

        string ReadAllNetStream(NetworkStream stream)
        {
            if (stream.CanRead)
            {
                byte[] buf = new byte[1024];
                StringBuilder recvMsg = new StringBuilder();
                int nByte = 0;

                // Incoming message may be larger than the buffer size.
                do
                {
                    nByte = stream.Read(buf, 0, buf.Length);

                    recvMsg.AppendFormat("{0}", Encoding.UTF8.GetString(buf, 0, nByte));

                }
                while (mSrvrConn && stream.DataAvailable);
                return recvMsg.ToString();
            }
            return String.Empty;
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            mSrvrConn = false;
            mClient.Close();
        }

        private void StartSrvr_Click(object sender, RoutedEventArgs e)
        {

            Thread th = new Thread(() => { mServer.Start(ref bSrvrMsg, ref mSrvrMsg); /*StartSrvr(ref bSrvrMsg, ref mSrvrMsg); */});
            th.Start();
        }

        private void StartSrvr(ref bool bUpdate, ref string msg)
        {
            mServer.Start(ref bUpdate, ref msg);
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (bSrvrMsg)
                Dispatcher.Invoke(() => {
                    lblStatus.Text += mSrvrMsg; bSrvrMsg = false; mSrvrMsg = String.Empty;
                });
        }

        private void StopSrvr_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref bSrvrMsg, ref mSrvrMsg);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window w = (Window)Parent;
            w.Close();
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool dummy1 = false;
            string dummy2 = null;
            mServer.Stop(ref dummy1, ref dummy2);
        }
    }
}
