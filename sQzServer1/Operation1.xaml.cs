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
        Dictionary<int, TextBlock> vComp;
        Dictionary<int, TextBlock> vTime1;
        Dictionary<int, TextBlock> vTime2;
        Dictionary<int, TextBlock> vMark;
        Dictionary<int, CheckBox> vLock;//supervisor side
        Dictionary<int, bool> vbLock;//examinee side
        ExamSlot mSl;
        int uRId;//todo change to enum
        bool bAllR;
        bool bStrtReqQSh;
        bool bQShReqting;
        int uReqQSh;

        public Operation1()
        {
            InitializeComponent();
            ShowsNavigationUI = false;

            mState = NetCode.DateStudentRetriving;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, true);
            mServer = new Server2(SrvrBufHndl);
            mServer.SrvrPort = 23821;
            mCbMsg = new UICbMsg();
            bRunning = true;

            mSl = new ExamSlot();

            vComp = new Dictionary<int, TextBlock>();
            vTime1 = new Dictionary<int, TextBlock>();
            vTime2 = new Dictionary<int, TextBlock>();
            vMark = new Dictionary<int, TextBlock>();
            vLock = new Dictionary<int, CheckBox>();
            vbLock = new Dictionary<int, bool>();

            bAllR = false;
            uRId = 1;//todo
            bStrtReqQSh = bQShReqting = false;
            uReqQSh = ushort.MaxValue;

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
            btnStopSrvr_Click(null, null);
            //todo: check th state to return
            Thread th = new Thread(() => { mClnt.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void btnStartSrvr_Click(object sender, RoutedEventArgs e)
        {
            Thread th = new Thread(() => { mServer.Start(ref mCbMsg); });
            th.Start();
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning)
            {
                if(bStrtReqQSh)
                {
                    mState = NetCode.RequestQuestSheet;
                    btnConnect_Click(null, null);
                    bStrtReqQSh = false;
                    bQShReqting = true;
                }
                if (mCbMsg.ToUp())
                    Dispatcher.Invoke(() =>
                    {
                        lblStatus.Text += mCbMsg.txt;
                    });
            }
        }

        private void btnStopSrvr_Click(object sender, RoutedEventArgs e)
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

        public bool SrvrBufHndl(byte[] buf, out byte[] outMsg)
        {
            int offs = 0;
            NetCode c = (NetCode)BitConverter.ToInt32(buf, offs);
            offs += 4;
            QuestSheet qs;
            int lvid;
            ExamineeA e;
            switch (c)
            {
                case NetCode.Dating:
                    outMsg = new byte[20];
                    offs = 0;
                    ExamSlot.ToByteDt(outMsg, ref offs, mSl.mDt);
                    return true;
                case NetCode.Authenticating:
                    e = new ExamineeS1();
                    e.bFromC = true;
                    e.ReadByte(buf, ref offs);
                    bool lck;
                    lvid = e.mLv + e.uId;
                    if (!vbLock.TryGetValue(lvid, out lck))
                        lck = false;//err, default value benefits examinees
                    if (!lck)
                    {
                        e = mSl.Signin(e);
                        if (e != null)
                        {
                            if (e.dtTim1.Hour == ExamSlot.INVALID)
                                e.dtTim1 = DateTime.Now;
                            Dispatcher.Invoke(() =>
                            {
                                TextBlock t;
                                lvid = e.mLv + e.uId;
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
                            e.bFromC = true;
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
                        e = mSl.Find(lvid);
                        if (e == null)
                            e = new ExamineeC();
                        if (e.tComp == null)
                            outMsg = new byte[16];
                        else
                            outMsg = new byte[16 + e.tComp.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes((int)TxI.SIGNIN_AL), 0, outMsg, 0, 4);
                        if(e.tComp == null)
                        {
                            Buffer.BlockCopy(BitConverter.GetBytes(0), 0, outMsg, 4, 4);
                            offs = 8;
                        }
                        else
                        {
                            byte[] comp = Encoding.UTF8.GetBytes(e.tComp);
                            Buffer.BlockCopy(BitConverter.GetBytes(comp.Length), 0, outMsg, 4, 4);
                            offs = 8;
                            Buffer.BlockCopy(comp, 0, outMsg, offs, e.tComp.Length);
                            offs += comp.Length;
                        }
                        
                        Buffer.BlockCopy(BitConverter.GetBytes(e.dtTim1.Hour), 0, outMsg, offs, 4);
                        offs += 4;
                        Buffer.BlockCopy(BitConverter.GetBytes(e.dtTim1.Minute), 0, outMsg, offs, 4);
                        break;
                    }
                    return true;
                case NetCode.ExamRetrieving:
                    outMsg = null;
                    int x;
                    if (!Enum.IsDefined(typeof(ExamLv), x = BitConverter.ToInt32(buf, offs)))
                        break;
                    offs += 4;
                    int qsid = BitConverter.ToInt32(buf, offs);
                    if (qsid == ushort.MaxValue)
                    {
                        byte[] a = mSl.vQPack[(ExamLv)x].ToByteNextQS();
                        if(a != null)
                        {
                            outMsg = new byte[a.Length + 4];
                            Array.Copy(BitConverter.GetBytes(0), outMsg, 4);
                            Array.Copy(a, 0, outMsg, 4, a.Length);
                        }
                    }
                    else if (mSl.vQPack[(ExamLv)x].vSheet.TryGetValue(qsid, out qs))
                    {
                        outMsg = new byte[qs.aQuest.Length + 4];
                        Array.Copy(BitConverter.GetBytes(0), outMsg, 4);
                        Array.Copy(qs.aQuest, 0, outMsg, 4, qs.aQuest.Length);
                    }
                    if(outMsg == null)
                    {
                        mCbMsg += Txt.s._[(int)TxI.QS_NFOUND] + (x + qsid);
                        outMsg = new byte[8];
                        Array.Copy(BitConverter.GetBytes((int)TxI.QS_NFOUND), 0, outMsg, 0, 4);
                        Array.Copy(BitConverter.GetBytes(qsid), 0, outMsg, 4, 4);
                        if (!bQShReqting)
                        {
                            bStrtReqQSh = true;
                            uReqQSh = x + qsid;
                        }
                    }
                    break;
                case NetCode.Submiting:
                    e = new ExamineeS1();
                    e.bFromC = true;
                    if (!e.ReadByte(buf, ref offs))
                    {
                        AnsSheet keySh;
                        if (!mSl.mKeyPack.vSheet.TryGetValue(e.mAnsSh.uQSId, out keySh))
                        {
                            outMsg = BitConverter.GetBytes(101);//todo
                            break;
                        }
                        ExamineeA o;
                        lvid = e.mLv + e.uId;
                        if ((o = mSl.Find(lvid)) != null)
                        {
                            o.eStt = ExamStt.Finished;
                            o.mAnsSh = e.mAnsSh;
                            o.uGrade = keySh.Grade(e.mAnsSh.aAns);
                            o.dtTim2 = DateTime.Now;
                            if (vbLock.Keys.Contains(lvid))
                                vbLock[lvid] = true;
                            Thread th = new Thread(() =>
                                Dispatcher.Invoke(() =>
                                {
                                    TextBlock t = null;
                                    if (vTime2.TryGetValue(lvid, out t))//todo
                                        t.Text = o.dtTim2.ToString("HH:mm");
                                    if (vMark.TryGetValue(lvid, out t))
                                        t.Text = o.uGrade.ToString();
                                    CheckBox cbx;
                                    if (vLock.TryGetValue(lvid, out cbx))
                                    {
                                        cbx.IsChecked = true;
                                        cbx.IsEnabled = false;
                                    }
                                }));
                            th.Start();
                            o.ToByte(out outMsg, 0);
                        }
                        else
                        {
                            string s = "ERROR submitting clnt not found " + lvid;//todo
                            mCbMsg += s;
                            byte[] b = Encoding.UTF8.GetBytes(s);
                            outMsg = new byte[5 + b.Length];
                            Array.Copy(BitConverter.GetBytes(false), outMsg, 1);
                            Array.Copy(BitConverter.GetBytes(b.Length), 0, outMsg, 1, 4);
                            Array.Copy(b, 0, outMsg, 5, b.Length);
                        }
                    }
                    else
                    {
                        string s = "ERROR submitting clnt's data is error";//todo
                        mCbMsg += s;
                        byte[] b = Encoding.UTF8.GetBytes(s);
                        outMsg = new byte[5 + b.Length];
                        Array.Copy(BitConverter.GetBytes(false), outMsg, 1);
                        Array.Copy(BitConverter.GetBytes(b.Length), 0, outMsg, 1, 4);
                        Array.Copy(b, 0, outMsg, 5, b.Length);
                    }
                    break;
                default:
                    outMsg = null;
                    break;
            }
            return false;
        }

        public bool ClntBufHndl(byte[] buf)
        {
            int offs = 0;
            switch (mState)
            {
                case NetCode.DateStudentRetriving:
                    if (ExamSlot.ReadByteDt(buf, ref offs, out mSl.mDt))
                        break;
                    mSl.ReadByteR1(buf, ref offs);
                    Dispatcher.Invoke(() => {
                        if (mSl.mDt.Year != ExamSlot.INVALID)
                            txtDate.Text = mSl.mDt.ToString(ExamSlot.FORM_H);
                        vComp.Clear();
                        vMark.Clear();
                        vTime1.Clear();
                        vTime2.Clear();
                        int rid = 0;
                        foreach(ExamRoom r in mSl.vRoom.Values)
                            foreach (ExamineeA e in r.vExaminee.Values)
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
                                int lvid = e.mLv + e.uId;
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
                    return true;
                case NetCode.QuestRetrieving:
                    mSl.ReadByteQPack(buf, ref offs);
                    ShowQuest();
                    mState = NetCode.AnsKeyRetrieving;
                    return true;
                case NetCode.AnsKeyRetrieving:
                    if (mSl.mKeyPack.ReadByte(buf, ref offs))
                        offs = 0;//todo handle error
                    break;
                case NetCode.RequestQuestSheet:
                    int rs = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    if(rs == 0)
                    {
                        ExamLv lv;
                        if (uReqQSh < (int)ExamLv.B)
                            lv = ExamLv.A;
                        else
                            lv = ExamLv.B;
                        if (mSl.ReadByteQPack1(lv, buf, ref offs))
                            offs = 0;//todo handle error
                        else
                        {
                            if (mSl.mKeyPack.ReadByte1(buf, ref offs))
                                offs = 0;//todo handle error
                            else
                                Dispatcher.Invoke(() => ShowQuest());
                        }
                    }
                    btnStartSrvr_Click(null, null);
                    bQShReqting = false;
                    mState = NetCode.Unknown;
                    break;
                case NetCode.SrvrSubmitting:
                    if (buf.Length - offs == 4 && BitConverter.ToInt32(buf, offs) == 1)
                        mCbMsg += Txt.s._[(int)TxI.SRVR_SUBMT_OK];
                    break;
            }
            return false;
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

        public byte[] ClntBufPrep()
        {
            byte[] outMsg = null;
            switch (mState)
            {
                case NetCode.DateStudentRetriving:
                    outMsg = new byte[8];
                    Array.Copy(BitConverter.GetBytes((int)mState), 0, outMsg, 0, 4);
                    if(bAllR)
                        Array.Copy(BitConverter.GetBytes(0), 0, outMsg, 4, 4);
                    else
                        Array.Copy(BitConverter.GetBytes(uRId), 0, outMsg, 4, 4);
                    break;
                case NetCode.QuestRetrieving:
                    outMsg = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.AnsKeyRetrieving:
                    outMsg = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.RequestQuestSheet:
                    outMsg = new byte[8];
                    Array.Copy(BitConverter.GetBytes((int)mState), 0, outMsg, 0, 4);
                    Array.Copy(BitConverter.GetBytes(uReqQSh), 0, outMsg, 4, 4);
                    break;
                case NetCode.SrvrSubmitting:
                    byte[] prefx = new byte[8];
                    Array.Copy(BitConverter.GetBytes((int)mState), prefx, 4);
                    //Array.Copy(BitConverter.GetBytes(mRoom.uId), 0, prefx, 4, 4);//todo
                    //mRoom.ToByteS0(prefx, out outMsg);
                    break;
            }
            return outMsg;
        }

        private void ShowQuest() //same as Operation0.xaml
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
                tbcQuest.Items.Clear();
                foreach(QuestPack p in mSl.vQPack.Values)
                    foreach (QuestSheet qs in p.vSheet.Values)
                    {
                        TabItem ti = new TabItem();
                        ti.Header = qs.eLv.ToString() + qs.uId;
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
            bAllR = true;
        }

        private void ckbAllNee_Unchecked(object sender, RoutedEventArgs e)
        {
            bAllR = false;
        }
    }
}
