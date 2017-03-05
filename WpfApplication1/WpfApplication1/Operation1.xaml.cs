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
        NetSttCode mState;
        byte[] zQuest;
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
            mState = NetSttCode.PrepDateStudent;
            mClient = Client0.Instance();
            mClient.SetSrvrPort(23820);
            mServer = new Server1(ResponseMsg);
            bSrvrMsg = false;
            mSrvrMsg = String.Empty;
            mSrvrConn = true;

            TakeExam.InitBrush();

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        public byte[] ResponseMsg(char code)
        {
            byte[] msg = null;
            switch (code)
            {
                case (char)NetSttCode.Dating:
                    msg = Date.sbArr;//check null
                    break;
                case (char)NetSttCode.Authenticating:
                    msg = zQuest;
                    break;
                case (char)NetSttCode.ExamRetrieving:
                    break;
                case (char)NetSttCode.Submiting:
                    break;
                default:
                    msg = BitConverter.GetBytes((char)NetSttCode.Unknown);
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
            if (mState == NetSttCode.PrepDateStudent)
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
                //char[] msg = new char[1];
                //msg[0] = (char)NetSttCode.DateStudentRetriving;
                //mBuffer = Encoding.UTF8.GetBytes(msg);
                mState = NetSttCode.DateStudentRetriving;
                mBuffer = BitConverter.GetBytes((Int32)mState);
                s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                return;
            }
            if (mState == NetSttCode.DateStudentRetriving)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                s.EndWrite(ar);
                mBuffer = new byte[mSz];
                mState = NetSttCode.DateStudentRetrieved;
                //s.BeginRead(mBuffer, 0, mSz, CB, s);
                s.BeginRead(mBuffer, 0, mSz, CB, s);
                return;
            }
            if (mState == NetSttCode.DateStudentRetrieved)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                int r = s.EndRead(ar);
                int offs = 0;
                Date.ReadByteArr(mBuffer, ref offs);
                Student.ReadByteArr(mBuffer, ref offs);
                Dispatcher.Invoke(() => {
                    if(Date.sbArr != null)
                        txtDate.Text = Encoding.UTF32.GetString(Date.sbArr);
                    foreach(Student st in Student.svStudent)
                    {
                        TextBlock x = new TextBlock();
                        x.FontSize = TakeExam.em;
                        x.Text = st.ToString();
                        spStudent.Children.Add(x);
                    }
                });
                //char[] msg = new char[1];
                //msg[0] = (char)NetSttCode.QuestAnsKeyRetrieving;
                //mBuffer = Encoding.UTF8.GetBytes(msg);
                mState = NetSttCode.PrepQuestAnsKey;
                mBuffer = BitConverter.GetBytes((Int32)mState);
                //reconnect
                btnDisconnect_Click(null, null);
                //mClient = new Client0();
                btnConnect_Click(null, null);
                return;
            }
            if (mState == NetSttCode.PrepQuestAnsKey)
            {
                TcpClient c = (TcpClient)ar.AsyncState;
                if (c.Connected)
                {
                    NetworkStream s = (NetworkStream)c.GetStream();
                    //char[] msg = new char[1];
                    //msg[0] = (char)NetSttCode.QuestAnsKeyRetrieving;
                    //mBuffer = Encoding.UTF8.GetBytes(msg);
                    mState = NetSttCode.QuestAnsKeyRetrieving;
                    s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                }
                return;
            }
            if (mState == NetSttCode.QuestAnsKeyRetrieving)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                s.EndWrite(ar);//bookmark
                mBuffer = new byte[mSz];
                mState = NetSttCode.QuestAnsKeyRetrieved;
                s.BeginRead(mBuffer, 0, mSz, CB, s);
                return;
            }
            if (mState == NetSttCode.QuestAnsKeyRetrieved)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                int r = s.EndRead(ar);
                int offs = 0;
                Question.ReadByteArr(mBuffer, ref offs);
                mState = NetSttCode.PrepMark;
            }
        }

        private string ReadAllNetStream(IAsyncResult ar)
        {
            NetworkStream stream = (NetworkStream)ar.AsyncState;
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
