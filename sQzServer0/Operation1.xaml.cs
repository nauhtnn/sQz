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
using sQzLib;

namespace sQzServer0
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
        int nBusy;//crash fixed: only call if not busy
        bool bToDispose;//crash fixed: flag to dispose
        bool bReconn;//reconnect after callback
        Server1 mServer;
        UICbMsg mCbMsg;

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
            mServer = new Server1();
            mCbMsg = new UICbMsg();
            nBusy = 0;
            bToDispose = false;
            bReconn = false;

            Theme.InitBrush();

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

            txtDate.FontSize = Theme.em;
        }

        private void spMain_Loaded(object sender, RoutedEventArgs e)
        {
            spMain.Background = Theme.vBrush[(int)BrushId.BG];
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
            if (0 < nBusy)
                return;
            ++nBusy;
            mClient.BeginConnect(CB);
        }

        private void CB(IAsyncResult ar)
        {
            NetworkStream s = null;
            TcpClient c = null;
            int r = 0, offs = 0;
            switch (mState)
            {
                case NetSttCode.PrepDateStudent:
                    c = (TcpClient)ar.AsyncState;
                    if (c.Connected)
                    {
                        s = c.GetStream();
                        mState = NetSttCode.DateStudentRetriving;
                        mBuffer = BitConverter.GetBytes((Int32)mState);
                        ++nBusy;
                        s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                    }
                    break;
                case NetSttCode.DateStudentRetriving:
                    s = (NetworkStream)ar.AsyncState;
                    s.EndWrite(ar);
                    mBuffer = new byte[mSz];
                    mState = NetSttCode.DateStudentRetrieved;
                    ++nBusy;
                    s.BeginRead(mBuffer, 0, mSz, CB, s);
                    break;
                case NetSttCode.DateStudentRetrieved:
                    s = (NetworkStream)ar.AsyncState;
                    r = s.EndRead(ar);
                    offs = 0;
                    Date.ReadByteArr(mBuffer, ref offs, r);
                    r -= offs;
                    Student.ReadByteArr(mBuffer, ref offs, r);
                    Dispatcher.Invoke(() => {
                        if(Date.sbArr != null)
                            txtDate.Text = Encoding.UTF32.GetString(Date.sbArr);
                        foreach(Student st in Student.svStudent)
                        {
                            TextBlock x = new TextBlock();
                            x.FontSize = Theme.em;
                            x.Text = st.ToString();
                            spStudent.Children.Add(x);
                        }
                    });
                //    mState = NetSttCode.PrepQuestAnsKey;
                //    mBuffer = BitConverter.GetBytes((Int32)mState);
                //    //reconnect
                //    bReconn = true;
                //    break;
                //case NetSttCode.PrepQuestAnsKey:
                //    c = (TcpClient)ar.AsyncState;
                //    if (c.Connected)
                //    {
                //        s = (NetworkStream)c.GetStream();
                    mState = NetSttCode.QuestAnsKeyRetrieving;
                    mBuffer = BitConverter.GetBytes((Int32)mState);
                    ++nBusy;
                    s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                    //}
                    break;
                case NetSttCode.QuestAnsKeyRetrieving:
                    s = (NetworkStream)ar.AsyncState;
                    s.EndWrite(ar);
                    mBuffer = new byte[mSz];
                    mState = NetSttCode.QuestAnsKeyRetrieved;
                    ++nBusy;
                    s.BeginRead(mBuffer, 0, mSz, CB, s);
                    break;
                case NetSttCode.QuestAnsKeyRetrieved:
                    s = (NetworkStream)ar.AsyncState;
                    r = s.EndRead(ar);
                    offs = 0;
                    Question.ReadByteArr(mBuffer, ref offs, r);
                    mState = NetSttCode.PrepMark;
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

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (nBusy == 0)
                mClient.Close();
            else
                bToDispose = true;
        }

        private void StartSrvr_Click(object sender, RoutedEventArgs e)
        {

            Thread th = new Thread(() => { mServer.Start(ref mCbMsg); });
            th.Start();
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (mCbMsg.ToUp())
                Dispatcher.Invoke(() => {
                    lblStatus.Text += mCbMsg.txt; });
        }

        private void StopSrvr_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref mCbMsg);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window w = (Window)Parent;
            w.Close();
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UICbMsg dummy = new UICbMsg();
            mServer.Stop(ref dummy);
        }
    }
}
