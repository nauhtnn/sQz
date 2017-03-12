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
        Dictionary<string, TextBlock> vMark;

        public Operation1()
        {
            InitializeComponent();
            ShowsNavigationUI = false;

            mState = NetCode.DateStudentRetriving;
            mClient2 = new Client2(CliBufHndl, CliBufPrep);
            mServer = new Server2(SrvrCodeHndl);
            mServer.SrvrPort = 23821;
            mCbMsg = new UICbMsg();

            vMark = new Dictionary<string, TextBlock>();

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

            txtDate.FontSize = Theme.em;
        }

        private void spMain_Loaded(object sender, RoutedEventArgs e)
        {
            spMain.Background = Theme.vBrush[(int)BrushId.BG];
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;

            double rt = spMain.RenderSize.Width / 1280; //d:DesignWidth
            //double scaleH = spMain.RenderSize.Height / 360; //d:DesignHeight
            //ScaleScreen(scaleW);
            ScaleTransform st = new ScaleTransform(rt, rt);
            spMain.RenderTransform = st;

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
            Window.GetWindow(this).Close();
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
                    int l = dat.Length - offs;
                    if (l < 4)
                        break;
                    ExamLvl lv = (ExamLvl)BitConverter.ToInt32(dat, offs);
                    l -= 4;
                    offs += 4;
                    if (l < 2)
                        break;
                    short id = BitConverter.ToInt16(dat, offs);
                    l -= 2;
                    offs += 2;
                    if (l < 10)
                        break;
                    string date = Encoding.UTF32.GetString(dat, offs, dat.Length - offs);
                    outMsg = null;
                    foreach (Examinee st in Examinee.svExaminee)
                    {
                        if (st.mId == id && st.mLvl == lv && st.mBirthdate == date)
                        {
                            outMsg = BitConverter.GetBytes(1);
                            break;
                        }
                    }
                    if(outMsg == null)
                        outMsg = BitConverter.GetBytes(0);
                    break;
                case NetCode.ExamRetrieving:
                    outMsg = Question.sbArrwKey;
                    //outMsg = Question.sbArr;
                    break;
                case NetCode.Submiting:
                    int mark = 0;
                    offs = 0;
                    if (dat.Length < sizeof(int)) {
                        outMsg = BitConverter.GetBytes((int)101);
                        break;
                    }
                    int len = BitConverter.ToInt32(dat, offs);
                    offs += 4;
                    len = (len > dat.Length) ? dat.Length : len;
                    //List<byte[]> l = new List<byte[]>();
                    for(int i = 0, j = offs, m = len, k = 0,
                        n = Question.svQuest.Count; i < n && j < m; ++i)
                    {
                        for (k = 0; k < Question.svQuest[i].vKeys.Length && j < m; ++k, ++j)
                            if (dat[j] != Convert.ToByte(Question.svQuest[i].vKeys[k]))
                                break;
                        if (k == Question.svQuest[i].vKeys.Length)
                            ++mark;
                    }
                    Dispatcher.Invoke(() => {
                        TextBlock t = null;
                        if (vMark.TryGetValue("" + 1 + "" + 1, out t))
                            t.Text = mark.ToString();
                    });
                    outMsg = BitConverter.GetBytes(mark);
                    //if (dat.Length == Question.sbArr.Length)
                    //{
                    //    for (int i = 0; i < dat.Length;)
                    //    {
                    //        int j = i + 4;
                    //        for (; i < j; ++i)
                    //            if (dat[i] != Question.sbArr[i])
                    //                break;
                    //        if (i == j)
                    //            ++mark;
                    //        i = j;
                    //    }
                    //    outMsg = BitConverter.GetBytes((Int32)mark);
                    //}
                    //else
                    //    outMsg = BitConverter.GetBytes((Int32)NetCode.Resubmit);
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
                case NetCode.DateStudentRetriving:
                    int r = buf.Length;
                    Date.ReadByteArr(buf, ref offs, r);
                    r -= offs;
                    Examinee.ReadByteArr(buf, ref offs, r);
                    Dispatcher.Invoke(() => {
                        if (Date.sbArr != null)
                            txtDate.Text = Encoding.UTF32.GetString(Date.sbArr);
                        int rid = 0;
                        foreach (Examinee st in Examinee.svExaminee)
                        {
                            RowDefinition rd = new RowDefinition();
                            rd.Height = new GridLength(20);
                            gStudent.RowDefinitions.Add(rd);
                            TextBlock t = new TextBlock();
                            t.Text = st.ToString();
                            Grid.SetRow(t, rid);
                            gStudent.Children.Add(t);
                            t = new TextBlock();
                            t.Text = "waiting";
                            Grid.SetRow(t, rid);
                            Grid.SetColumn(t, 1);
                            gStudent.Children.Add(t);
                            t = new TextBlock();
                            t.Text = "doing";
                            vMark.Add("" + (int)st.mLvl + "" + st.mId, t);
                            Grid.SetRow(t, rid++);
                            Grid.SetColumn(t, 2);
                            gStudent.Children.Add(t);
                        }
                    });
                    mState = NetCode.QuestAnsKeyRetrieving;
                    break;
                case NetCode.QuestAnsKeyRetrieving:
                    offs = 0;
                    Question.ReadByteArr(buf, ref offs, buf.Length, true);
                    Question.ToByteArr(true);
                    LoadQuest();
                    mState = NetCode.PrepMark;
                    return false;
            }
            return true;
        }

        public bool CliBufPrep(ref byte[] outBuf)
        {
            switch (mState)
            {
                case NetCode.DateStudentRetriving:
                    outBuf = BitConverter.GetBytes((Int32)mState);
                    break;
                case NetCode.QuestAnsKeyRetrieving:
                    outBuf = BitConverter.GetBytes((Int32)mState);
                    break;
            }
            return true;
        }

        private void LoadQuest() //same as Operation0.xaml
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
                int x = 0;
                foreach (Question q in Question.svQuest)
                {
                    TextBlock i = new TextBlock();
                    i.Text = ++x + ") " + q.ToString();
                    dark = !dark;
                    if (dark)
                        i.Background = new SolidColorBrush(c);
                    else
                        i.Background = Theme.vBrush[(int)BrushId.LeftPanel_BG];
                    gQuest.Children.Add(i);
                }
            });
        }
    }
}
