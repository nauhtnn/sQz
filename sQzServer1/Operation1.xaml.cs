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

namespace sQzServer1
{
    /// <summary>
    /// Interaction logic for Operation1.xaml
    /// </summary>
    public partial class Operation1 : Page
    {
        Client2 mClient2;
        NetCode mState;
        Server2 mServer;
        UICbMsg mCbMsg;

        public Operation1()
        {
            InitializeComponent();
            ShowsNavigationUI = false;

            mState = NetCode.PrepDateStudent;
            mClient2 = new Client2(CliBufHndl, CliBufPrep);
            mClient2.SrvrPort = 23820;
            mServer = new Server2(SrvrCodeHndl);
            mServer.SrvrPort = 23821;
            mCbMsg = new UICbMsg();

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
                case (char)NetCode.Dating:
                    msg = Date.sbArr;//check null
                    break;
                case (char)NetCode.Authenticating:
                    msg = null;
                    break;
                case (char)NetCode.ExamRetrieving:
                    break;
                case (char)NetCode.Submiting:
                    break;
                default:
                    msg = BitConverter.GetBytes((char)NetCode.Unknown);
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
            //todo: check th state to return
            Thread th = new Thread(() => { mClient2.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
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

        private void btnCli_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                NavigationService.Navigate(new Uri("Authentication.xaml", UriKind.Relative)); });
        }

        public bool SrvrCodeHndl(NetCode c, byte[] dat, int offs, ref byte[] outMsg)
        {
            switch (c)
            {
                case NetCode.Dating:
                    outMsg = Date.sbArr;
                    break;
                case NetCode.Authenticating:
                    outMsg = BitConverter.GetBytes((Int32)1);
                    break;
                case NetCode.ExamRetrieving:
                    outMsg = Question.sbArrwKey;
                    //outMsg = Question.sbArr;
                    break;
                case NetCode.Submiting:
                    outMsg = BitConverter.GetBytes((Int32)NetCode.Unknown);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public bool CliBufHndl(byte[] buf, int offs)
        {
            switch (mState)
            {
                case NetCode.DateStudentRetrieved:
                    int r = buf.Length;
                    Date.ReadByteArr(buf, ref offs, r);
                    r -= offs;
                    Student.ReadByteArr(buf, ref offs, r);
                    Dispatcher.Invoke(() => {
                        if (Date.sbArr != null)
                            txtDate.Text = Encoding.UTF32.GetString(Date.sbArr);
                        foreach (Student st in Student.svStudent)
                        {
                            TextBlock x = new TextBlock();
                            x.FontSize = Theme.em;
                            x.Text = st.ToString();
                            spStudent.Children.Add(x);
                        }
                    });
                    mState = NetCode.QuestAnsKeyRetrieving;
                    break;
                case NetCode.QuestAnsKeyRetrieved:
                    offs = 0;
                    Question.ReadByteArr(buf, ref offs, buf.Length, true);
                    Question.ToByteArr(true);
                    mState = NetCode.PrepMark;
                    return false;
            }
            return true;
        }

        public bool CliBufPrep(ref byte[] outBuf)
        {
            switch (mState)
            {
                case NetCode.PrepDateStudent:
                    mState = NetCode.DateStudentRetriving;
                    outBuf = BitConverter.GetBytes((Int32)mState);
                    mState = NetCode.DateStudentRetrieved;
                    break;
                case NetCode.QuestAnsKeyRetrieving:
                    outBuf = BitConverter.GetBytes((Int32)mState);
                    mState = NetCode.QuestAnsKeyRetrieved;
                    break;
            }
            return true;
        }
    }
}
