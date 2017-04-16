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
        Client2 mClnt;
        NetCode mState;
        Server2 mServer;
        UICbMsg mCbMsg;
        bool bRunning;
        ExamDate mDt;
        ExamRoom mRoom;
        Dictionary<short, TextBlock> vComp;
        Dictionary<short, TextBlock> vTime1;
        Dictionary<short, TextBlock> vTime2;
        Dictionary<short, TextBlock> vMark;
        Dictionary<short, CheckBox> vLock;//supervisor side
        Dictionary<short, bool> vbLock;//examinee side
        QuestPack mQPack;
        int mQShIdx;
        int mQShMaxIdx;
        AnsPack mAnsPack;

        public Operation1()
        {
            InitializeComponent();
            ShowsNavigationUI = false;

            mState = NetCode.DateStudentRetriving;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, true);
            mServer = new Server2(SrvrCodeHndl);
            mServer.SrvrPort = 23821;
            mCbMsg = new UICbMsg();
            bRunning = true;

            mDt = new ExamDate();
            mRoom = new ExamRoom();

            vComp = new Dictionary<short, TextBlock>();
            vTime1 = new Dictionary<short, TextBlock>();
            vTime2 = new Dictionary<short, TextBlock>();
            vMark = new Dictionary<short, TextBlock>();
            vLock = new Dictionary<short, CheckBox>();
            vbLock = new Dictionary<short, bool>();

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void spMain_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 13;

            spMain.Background = Theme.s._[(int)BrushId.BG];

            double rt = spMain.RenderSize.Width / 1280;
            spMain.RenderTransform = new ScaleTransform(rt, rt);

            FirewallHandler fwHndl = new FirewallHandler(1);
            string msg = fwHndl.OpenFirewall();
            lblStatus.Text = msg;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            //todo: check th state to return
            Thread th = new Thread(() => { mClnt.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void StartSrvr_Click(object sender, RoutedEventArgs e)
        {
            Thread th = new Thread(() => { mServer.Start(ref mCbMsg); });
            th.Start();
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning && mCbMsg.ToUp())
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
            bRunning = false;
            UICbMsg dummy = new UICbMsg();
            mServer.Stop(ref dummy);
        }

        public bool SrvrCodeHndl(NetCode c, byte[] buf, int offs, ref byte[] outMsg)
        {
            QuestSheet qs;
            short lvid;
            Examinee e;
            switch (c)
            {
                case NetCode.Dating:
                    outMsg = new byte[mDt.GetByteCount()];
                    int offst = 0;
                    mDt.ToByte(outMsg, ref offst);
                    break;
                case NetCode.Authenticating:
                    e = mRoom.ReadByteSgning(buf, offs);
                    if (e != null)
                    {
                        bool lck;
                        if (!vbLock.TryGetValue((short)(e.Lvl * e.uId), out lck))
                            lck = false;//err, default value benefits examinees
                        if (!lck)
                        {
                            e.dtTim1 = DateTime.Now;
                            Dispatcher.Invoke(() =>
                            {
                                TextBlock t;
                                lvid = (short)(e.Lvl * e.uId);
                                if (vComp.TryGetValue(lvid, out t))
                                    t.Text = e.tComp;
                                if (vTime1.TryGetValue(lvid, out t))
                                    t.Text = e.dtTim1.ToString("HH:mm");
                                CheckBox cbx;
                                if (vLock.TryGetValue(lvid, out cbx))
                                {
                                    cbx.IsChecked = true;
                                    cbx.IsEnabled = true;
                                }
                                if (vbLock.Keys.Contains(lvid))
                                    vbLock[lvid] = true;
                            });
                            e.ToByte(out outMsg);
                        }
                        else
                        {
                            string msg = Txt.s._[(int)TxI.SIGNIN_AL_1] +
                                e.dtTim1.ToString("HH:mm dd/MM/yyyy") + Txt.s._[(int)TxI.SIGNIN_AL_2] + e.tComp + ".";
                            byte[] b = Encoding.UTF8.GetBytes(msg);
                            outMsg = new byte[5 + b.Length];
                            Buffer.BlockCopy(BitConverter.GetBytes(false), 0, outMsg, 0, 1);
                            Buffer.BlockCopy(BitConverter.GetBytes(b.Length), 0, outMsg, 1, 4);
                            Buffer.BlockCopy(b, 0, outMsg, 5, b.Length);
                        }
                    }
                    else
                    {
                        outMsg = BitConverter.GetBytes(false);
                        return false;//close
                    }
                    break;
                case NetCode.ExamRetrieving:
                    uint qshidx = mQPack.vSheet.Keys.ElementAt(mQShIdx);
                    if (mQShMaxIdx < ++mQShIdx)
                        mQShIdx = 0;
                    if(mQPack.vSheet.TryGetValue(qshidx, out qs))
                        outMsg = qs.aQuest;
                    break;
                case NetCode.Submiting:
                    AnsSheet s = new AnsSheet();
                    s.ReadByte(buf, ref offs);
                    AnsSheet wKey;
                    if(!mAnsPack.vSheet.TryGetValue(s.uId, out wKey))
                    {
                        outMsg = BitConverter.GetBytes(101);//todo
                        break;
                    }
                    ushort grade = wKey.Grade(s.aAns);
                    lvid = (short)(s.Lvl * s.uNeeId);
                    if (mRoom.vExaminee.TryGetValue(lvid, out e))
                    {
                        e.uGrade = grade;
                        e.dtTim2 = DateTime.Now;
                        if (vbLock.Keys.Contains(lvid))
                            vbLock[lvid] = true;
                        Dispatcher.Invoke(() =>
                        {
                            TextBlock t = null;
                            if (vTime2.TryGetValue(lvid, out t))//todo
                                t.Text = e.dtTim2.ToString("HH:mm");
                            if (vMark.TryGetValue(lvid, out t))
                                t.Text = grade.ToString();
                            CheckBox cbx;
                            if (vLock.TryGetValue(lvid, out cbx))
                            {
                                cbx.IsChecked = true;
                                cbx.IsEnabled = false;
                            }
                        });
                    }
                    outMsg = BitConverter.GetBytes(grade);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public bool ClntBufHndl(byte[] buf, int offs)
        {
            switch (mState)
            {
                case NetCode.DateStudentRetriving:
                    if (mDt.ReadByte(buf, ref offs))
                        return false;
                    mRoom.ReadByte(buf, ref offs);
                    Dispatcher.Invoke(() => {
                        if (mDt.mDt.Year != ExamDate.INVALID)
                            txtDate.Text = mDt.mDt.ToString(ExamDate.FORM_H);
                        vComp.Clear();
                        vMark.Clear();
                        vTime1.Clear();
                        vTime2.Clear();
                        int rid = 0;
                        foreach (Examinee e in mRoom.vExaminee.Values)
                        {
                            RowDefinition rd = new RowDefinition();
                            rd.Height = new GridLength(20);
                            gNee.RowDefinitions.Add(rd);
                            TextBlock t = new TextBlock();
                            t.Text = e.tId;
                            Grid.SetRow(t, ++rid);
                            gNee.Children.Add(t);
                            t = new TextBlock();
                            t.Text = e.tName;
                            Grid.SetRow(t, rid);
                            Grid.SetColumn(t, 1);
                            gNee.Children.Add(t);
                            t = new TextBlock();
                            t.Text = e.tBirdate;
                            Grid.SetRow(t, rid);
                            Grid.SetColumn(t, 2);
                            gNee.Children.Add(t);
                            t = new TextBlock();
                            short lvid = (short)(e.Lvl * e.uId);
                            vComp.Add(lvid, t);
                            Grid.SetRow(t, rid);
                            Grid.SetColumn(t, 3);
                            gNee.Children.Add(t);
                            CheckBox cbx = new CheckBox();
                            if (lvid < 0)
                                cbx.Name = "n" + (-lvid);
                            else
                                cbx.Name = "p" + lvid;
                            cbx.Unchecked += cbxLock_Unchecked;
                            cbx.IsEnabled = true;//default value empowers supervisors
                            Grid.SetRow(cbx, rid);
                            Grid.SetColumn(cbx, 7);
                            vLock.Add(lvid, cbx);
                            gNee.Children.Add(cbx);
                            t = new TextBlock();
                            if (e.dtTim1.Hour != 0)
                            {
                                t.Text = e.dtTim1.ToString("HH:mm");
                                vbLock.Add(lvid, true);
                            }
                            else
                            {
                                vbLock.Add(lvid, false);
                                cbx.IsEnabled = false;
                            }
                            vTime1.Add(lvid, t);
                            Grid.SetRow(t, rid);
                            Grid.SetColumn(t, 4);
                            gNee.Children.Add(t);
                            t = new TextBlock();
                            if (e.dtTim2.Hour != 0)
                                t.Text = e.dtTim2.ToString("HH:mm");
                            vTime2.Add(lvid, t);
                            Grid.SetRow(t, rid);
                            Grid.SetColumn(t, 5);
                            gNee.Children.Add(t);
                            t = new TextBlock();
                            if (e.uGrade != ushort.MaxValue)
                            {
                                t.Text = e.uGrade.ToString();
                                cbx.IsEnabled = false;
                            }
                            vMark.Add(lvid, t);
                            Grid.SetRow(t, rid);
                            Grid.SetColumn(t, 6);
                            gNee.Children.Add(t);
                        }
                    });
                    mState = NetCode.QuestRetrieving;
                    break;
                case NetCode.QuestRetrieving:
                    offs = 0;
                    mQPack = new QuestPack();
                    mQPack.ReadByte(buf, ref offs);
                    mQShIdx = 0;
                    mQShMaxIdx = mQPack.vSheet.Keys.Count - 1;
                    LoadQuest();
                    mState = NetCode.AnsKeyRetrieving;
                    break;
                case NetCode.AnsKeyRetrieving:
                    mAnsPack = new AnsPack();
                    mAnsPack.ReadByte(buf, ref offs);
                    return false;
            }
            return true;
        }

        private void cbxLock_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cbx = sender as CheckBox;
            short key;
            if (short.TryParse(cbx.Name.Substring(1), out key))
            {
                if (cbx.Name[0] == 'n')
                    key = (short)-key;
                vbLock[key] = false;//todo: safer
            }
        }

        public bool ClntBufPrep(ref byte[] outBuf)
        {
            switch (mState)
            {
                case NetCode.DateStudentRetriving:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.QuestRetrieving:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.AnsKeyRetrieving:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.SrvrSubmitting:
                    mRoom.ToByteGrade(BitConverter.GetBytes((int)mState), out outBuf);
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
                foreach (QuestSheet qs in mQPack.vSheet.Values)
                {
                    TabItem ti = new TabItem();
                    ti.Header = qs.uId;
                    ScrollViewer svwr = new ScrollViewer();
                    svwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    StackPanel sp = new StackPanel();
                    int x = 0;
                    foreach (Question q in qs.vQuest)
                    {
                        TextBlock i = new TextBlock();
                        i.Text = ++x + ") " + q.ToString();
                        dark = !dark;
                        if (dark)
                            i.Background = new SolidColorBrush(c);
                        else
                            i.Background = Theme.s._[(int)BrushId.LeftPanel_BG];
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
            Thread th = new Thread(() => { mClnt.ConnectWR(ref mCbMsg); });
            th.Start();
        }
    }
}
