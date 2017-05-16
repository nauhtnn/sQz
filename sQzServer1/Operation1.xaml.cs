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
        DateTime mDt;
        ExamRoom mRoom;
        Dictionary<int, TextBlock> vComp;
        Dictionary<int, TextBlock> vTime1;
        Dictionary<int, TextBlock> vTime2;
        Dictionary<int, TextBlock> vMark;
        Dictionary<int, CheckBox> vLock;//supervisor side
        Dictionary<int, bool> vbLock;//examinee side
        QuestPack mQPack;
        int mQShIdx;
        int mQShMaxIdx;
        AnsPack mKeyPack;
        AnsPack mAnsPack;
        bool bAllNee;

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

            mDt = ExamSlot.INVALID_DT;
            mRoom = new ExamRoom();
            if(System.IO.File.Exists("RoomId.txt"))
                mRoom.uId = int.Parse(System.IO.File.ReadAllText("RoomId.txt"));
            else
                mRoom.uId = 2;//todo

            vComp = new Dictionary<int, TextBlock>();
            vTime1 = new Dictionary<int, TextBlock>();
            vTime2 = new Dictionary<int, TextBlock>();
            vMark = new Dictionary<int, TextBlock>();
            vLock = new Dictionary<int, CheckBox>();
            vbLock = new Dictionary<int, bool>();

            bAllNee = false;

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

            //FirewallHandler fwHndl = new FirewallHandler(1);
            //string msg = fwHndl.OpenFirewall();
            //lblStatus.Text = msg;
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
            int lvid;
            Examinee e;
            switch (c)
            {
                case NetCode.Dating:
                    outMsg = new byte[20];
                    offs = 0;
                    ExamSlot.ToByteDt(outMsg, ref offs, mDt);
                    break;
                case NetCode.Authenticating:
                    //e = mRoom.ReadByteSgning(buf, offs);
                    e = new Examinee();
                    e.ReadByte(buf, ref offs);
                    bool lck;
                    if (!vbLock.TryGetValue((int)(e.Lv * e.uId), out lck))
                        lck = false;//err, default value benefits examinees
                    if (!lck)
                    {
                        e = mRoom.Signing(e);
                        if (e != null)
                        {
                            if (e.dtTim1.Hour == ExamSlot.INVALID)
                                e.dtTim1 = DateTime.Now;
                            Dispatcher.Invoke(() =>
                            {
                                TextBlock t;
                                lvid = (int)(e.Lv * e.uId);
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
                            byte[] a;
                            e.ToByte(out a);
                            outMsg = new byte[1 + a.Length];
                            Buffer.BlockCopy(BitConverter.GetBytes(true), 0, outMsg, 0, 1);
                            Buffer.BlockCopy(a, 0, outMsg, 1, a.Length);
                        }
                        else
                        {
                            outMsg = new byte[5];
                            Buffer.BlockCopy(BitConverter.GetBytes(false), 0, outMsg, 0, 1);
                            Buffer.BlockCopy(BitConverter.GetBytes((int)TxI.SIGNIN_NOK), 0, outMsg, 1, 4);
                            return false;//close
                        }
                    }
                    else
                    {
                        byte[] a;
                        e.ToByte(out a);
                        outMsg = new byte[5 + a.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes(false), 0, outMsg, 0, 1);
                        Buffer.BlockCopy(BitConverter.GetBytes((int)TxI.SIGNIN_AL_1), 0, outMsg, 1, 4);
                        Buffer.BlockCopy(a, 0, outMsg, 5, a.Length);
                        return false;//close
                    }
                    break;
                case NetCode.ExamRetrieving:
                    uint qshidx = BitConverter.ToUInt16(buf, offs);
                    if (qshidx == ushort.MaxValue)
                    {
                        if (mQShMaxIdx < ++mQShIdx)
                            mQShIdx = 0;
                        qshidx = mQPack.vSheet.Keys.ElementAt(mQShIdx);
                    }
                    if (mQPack.vSheet.TryGetValue(qshidx, out qs))
                    {
                        outMsg = new byte[qs.aQuest.Length + 4];
                        Array.Copy(BitConverter.GetBytes(0), outMsg, 4);
                        Array.Copy(qs.aQuest, 0, outMsg, 4, qs.aQuest.Length);
                    }
                    else
                    {
                        mCbMsg += Txt.s._[(int)TxI.QSH_NFOUND] + qshidx;
                        outMsg = BitConverter.GetBytes((int)TxI.QSH_NFOUND);
                    }
                    break;
                case NetCode.Submiting:
                    AnsSheet s = new AnsSheet();
                    s.ReadByte(buf, ref offs);
                    AnsSheet keySh;
                    if(!mKeyPack.vSheet.TryGetValue(s.uQSId, out keySh))
                    {
                        outMsg = BitConverter.GetBytes(101);//todo
                        break;
                    }
                    ushort grade = keySh.Grade(s.aAns);
                    lvid = s.Lv * s.uNeeId;
                    if (mRoom.vExaminee.TryGetValue(lvid, out e))
                    {
                        e.eStt = Examinee.eFINISHED;
                        e.mAnsSh = s;
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
                    else
                        mCbMsg += "ERROR submit clnt not found " + lvid + "-" + grade;//todo
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
                    if (ExamSlot.ReadByteDt(buf, ref offs, out mDt))
                        return false;
                    mRoom.ReadByte(buf, ref offs);
                    Dispatcher.Invoke(() => {
                        if (mDt.Year != ExamSlot.INVALID)
                            txtDate.Text = mDt.ToString(ExamSlot.FORM_H);
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
                            int lvid = (int)(e.Lv * e.uId);
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
                            if (e.dtTim1.Hour != ExamSlot.INVALID)
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
                            if (e.dtTim2.Hour != ExamSlot.INVALID)
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
                    mQShIdx = -1;
                    mQShMaxIdx = mQPack.vSheet.Keys.Count - 1;
                    LoadQuest();
                    mState = NetCode.AnsKeyRetrieving;
                    break;
                case NetCode.AnsKeyRetrieving:
                    mKeyPack = new AnsPack();
                    mKeyPack.ReadByte(buf, ref offs);
                    mAnsPack = new AnsPack();
                    return false;
                case NetCode.SrvrSubmitting:
                    if (buf.Length - offs == 4 && BitConverter.ToInt32(buf, offs) == 1)
                        mCbMsg += Txt.s._[(int)TxI.SRVR_SUBMT_OK];
                    return false;
            }
            return true;
        }

        private void cbxLock_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cbx = sender as CheckBox;
            int key;
            if (int.TryParse(cbx.Name.Substring(1), out key))
            {
                if (cbx.Name[0] == 'n')
                    key = (int)-key;
                vbLock[key] = false;//todo: safer
            }
        }

        public bool ClntBufPrep(ref byte[] outBuf)
        {
            switch (mState)
            {
                case NetCode.DateStudentRetriving:
                    outBuf = new byte[8];
                    Array.Copy(BitConverter.GetBytes((int)mState), 0, outBuf, 0, 4);
                    if(bAllNee)
                        Array.Copy(BitConverter.GetBytes(0), 0, outBuf, 4, 4);
                    else
                        Array.Copy(BitConverter.GetBytes(mRoom.uId), 0, outBuf, 4, 4);
                    break;
                case NetCode.QuestRetrieving:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.AnsKeyRetrieving:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.SrvrSubmitting:
                    byte[] prefx = new byte[8];
                    Array.Copy(BitConverter.GetBytes((int)mState), prefx, 4);
                    Array.Copy(BitConverter.GetBytes(mRoom.uId), 0, prefx, 4, 4);
                    mRoom.ToByteGrade(prefx, out outBuf);
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

        private void ckbAllNee_Checked(object sender, RoutedEventArgs e)
        {
            bAllNee = true;
        }

        private void ckbAllNee_Unchecked(object sender, RoutedEventArgs e)
        {
            bAllNee = false;
        }
    }
}
