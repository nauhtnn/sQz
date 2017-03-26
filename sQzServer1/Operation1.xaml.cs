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
        Dictionary<int, TextBlock> vMark;
        Dictionary<int, TextBlock> vComp;

        public Operation1()
        {
            InitializeComponent();
            ShowsNavigationUI = false;

            mState = NetCode.DateStudentRetriving;
            mClient2 = new Client2(CliBufHndl, CliBufPrep);
            mServer = new Server2(SrvrCodeHndl);
            mServer.SrvrPort = 23821;
            mCbMsg = new UICbMsg();

            vMark = new Dictionary<int, TextBlock>();
            vComp = new Dictionary<int, TextBlock>();

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
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 13;

            spMain.Background = Theme.vBrush[(int)BrushId.BG];

            double rt = spMain.RenderSize.Width / 1280;
            spMain.RenderTransform = new ScaleTransform(rt, rt);

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

        public bool SrvrCodeHndl(NetCode c, byte[] dat, int offs, ref byte[] outMsg)
        {
            switch (c)
            {
                case NetCode.Dating:
                    outMsg = Date.sbArr;
                    break;
                case NetCode.Authenticating:
                    string cname;
                    ExamLvl lv;
                    int rid = Examinee.SrvrReadAuthArr(dat, offs, out lv, out cname);
                    if (-1 < rid)
                    {
                        if (cname == null)
                            cname = "";
                        Dispatcher.Invoke(() =>
                        {
                            TextBlock t = null;
                            if (vComp.TryGetValue((int)lv * Examinee.svExaminee[rid].mId, out t))
                                t.Text = cname;
                        });
                        Examinee.SrvrToAuthArr(rid, out outMsg);
                    }
                    else
                    {
                        outMsg = BitConverter.GetBytes(false);
                        return false;//close
                    }
                    break;
                case NetCode.ExamRetrieving:
                    outMsg = Question.sbArrwKey;
                    //outMsg = Question.sbArr;
                    break;
                case NetCode.Submiting:
                    lv = (ExamLvl)BitConverter.ToInt32(dat, offs);
                    offs += 4;
                    int id = BitConverter.ToUInt16(dat, offs);
                    offs += 2;
                    if (dat.Length - offs != Question.svQuest[0].Count * 4)//hardcode
                    {
                        outMsg = BitConverter.GetBytes(101);//todo
                        break;
                    }
                    ushort mark = 0;
                    int j, k;
                    --offs;
                    foreach(Question q in Question.svQuest[0])
                    {
                        j = 0;
                        k = offs;
                        foreach (bool b in q.vKeys)
                            if (dat[++k] != Convert.ToByte(b))
                                break;
                            else
                                ++j;
                        if (j == q.vKeys.Length)
                            ++mark;
                        offs += q.vKeys.Length;
                    }
                    Dispatcher.Invoke(() => {
                        TextBlock t = null;
                        if (vMark.TryGetValue((int)lv * id, out t))//todo
                            t.Text = mark.ToString();
                    });
                    Examinee.svLvId2Idx.TryGetValue((int)lv * id, out rid);
                    Examinee.svExaminee[rid].mMark = mark;
                    outMsg = BitConverter.GetBytes(mark);
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
                        int rid = 1;
                        foreach (Examinee st in Examinee.svExaminee)
                        {
                            RowDefinition rd = new RowDefinition();
                            rd.Height = new GridLength(20);
                            gNee.RowDefinitions.Add(rd);
                            TextBlock t = new TextBlock();
                            t.Text = st.ID;
                            Grid.SetRow(t, rid);
                            gNee.Children.Add(t);
                            t = new TextBlock();
                            t.Text = st.mName;
                            Grid.SetRow(t, rid);
                            Grid.SetColumn(t, 1);
                            gNee.Children.Add(t);
                            t = new TextBlock();
                            t.Text = st.mBirthdate;
                            Grid.SetRow(t, rid);
                            Grid.SetColumn(t, 2);
                            gNee.Children.Add(t);
                            t = new TextBlock();
                            vComp.Add((int)st.mLvl * st.mId, t);
                            Grid.SetRow(t, rid);
                            Grid.SetColumn(t, 3);
                            gNee.Children.Add(t);
                            t = new TextBlock();
                            vMark.Add((int)st.mLvl * st.mId, t);
                            Grid.SetRow(t, rid++);
                            Grid.SetColumn(t, 4);
                            gNee.Children.Add(t);
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
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.QuestAnsKeyRetrieving:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.SrvrSubmitting:
                    Examinee.ToMarkArr(BitConverter.GetBytes((int)mState), out outBuf);
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
                tbcQuest.Items.Clear();
                int e = 0;
                foreach (List<Question> l in Question.svQuest)
                {
                    TabItem ti = new TabItem();
                    ti.Header = ++e;
                    ScrollViewer svwr = new ScrollViewer();
                    svwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    StackPanel sp = new StackPanel();
                    int x = 0;
                    foreach (Question q in l)
                    {
                        TextBlock i = new TextBlock();
                        i.Text = ++x + ") " + q.ToString();
                        dark = !dark;
                        if (dark)
                            i.Background = new SolidColorBrush(c);
                        else
                            i.Background = Theme.vBrush[(int)BrushId.LeftPanel_BG];
                        sp.Children.Add(i);
                    }
                    svwr.Content = sp;
                    ti.Content = svwr;
                    tbcQuest.Items.Add(ti);
                }
            });
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            //todo: check th state to return
            mState = NetCode.SrvrSubmitting;
            Thread th = new Thread(() => { mClient2.ConnectWR(ref mCbMsg); });
            th.Start();
        }
    }
}
