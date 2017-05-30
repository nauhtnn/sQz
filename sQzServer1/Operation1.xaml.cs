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
        ExamBoard mBrd;
        int uRId;//todo change to enum
        bool bAllR;
        bool bStrtReqQSh;
        bool bQShReqting;
        int uReqQSh;
        List<SortedList<int, bool>> vfbLock;

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

            mBrd = new ExamBoard();

            bAllR = false;
            uRId = 1;//todo
            bStrtReqQSh = bQShReqting = false;
            uReqQSh = ushort.MaxValue;

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            vfbLock = new List<SortedList<int, bool>>();
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
            outMsg = null;
            int offs = 0;
            NetCode c = (NetCode)BitConverter.ToInt32(buf, offs);
            offs += 4;
            QuestSheet qs;
            int lvid;
            ExamineeA e;
            DateTime dt;
            switch (c)
            {
                case NetCode.Dating:
                    outMsg = new byte[ExamBoard.BYTE_COUNT_DT];
                    offs = 0;
                    ExamBoard.ToByteDt(outMsg, ref offs, mBrd.mDt);
                    return true;
                case NetCode.Authenticating:
                    e = new ExamineeS1();
                    e.bFromC = true;
                    e.ReadByte(buf, ref offs);
                    bool lck = false;
                    lvid = e.mLv + e.uId;
                    bool found = false;
                    foreach (SortedList<int, bool> l in vfbLock)
                        if (l.TryGetValue(lvid, out lck))
                        {
                            found = true;
                            break;
                        }
                    if (!found)
                        lck = false;
                    if (!lck)
                    {
                        ExamineeA o = null;
                        dt = DateTime.Now;
                        foreach(ExamSlot sl in mBrd.vSl.Values)
                            if ((o = sl.Signin(e)) != null)
                            {
                                dt = sl.Dt;
                                break;
                            }
                        
                        if (o != null)
                        {
                            if (o.dtTim1.Hour == DtFmt.INV)
                                o.dtTim1 = DateTime.Now;
                            Dispatcher.Invoke(() =>
                            {
                                foreach(TabItem ti in tbcSl.Items)
                                {
                                    Op1SlotView vw = ti.Content as Op1SlotView;
                                    if(vw != null)
                                    {
                                        TextBlock t;
                                        lvid = o.mLv + o.uId;
                                        if (vw.vComp.TryGetValue(lvid, out t))
                                            t.Text = o.tComp;
                                        if (vw.vDt1.TryGetValue(lvid, out t))
                                            t.Text = o.dtTim1.ToString("HH:mm");
                                        CheckBox cbx;
                                        if (vw.vLock.TryGetValue(lvid, out cbx))
                                        {
                                            cbx.IsChecked = true;
                                            cbx.IsEnabled = true;
                                        }
                                        if (vw.vbLock.Keys.Contains(lvid))
                                            vw.vbLock[lvid] = true;
                                    }
                                }
                            });
                            byte[] a;
                            o.bFromC = true;
                            o.ToByte(out a);
                            outMsg = new byte[4 + a.Length];
                            Buffer.BlockCopy(BitConverter.GetBytes(0), 0, outMsg, 0, 4);
                            Buffer.BlockCopy(a, 0, outMsg, 4, a.Length);
                        }
                        else
                        {
                            outMsg = new byte[4];
                            Buffer.BlockCopy(BitConverter.GetBytes((int)TxI.SIGNIN_NOK), 0, outMsg, 0, 4);
                            return false;//close
                        }
                    }
                    else
                    {
                        ExamineeA o = null;
                        foreach (ExamSlot sl in mBrd.vSl.Values)
                            if ((o = sl.Find(lvid)) != null)
                                break;
                        if (o == null)
                            o = new ExamineeC();
                        if (o.tComp == null)
                            outMsg = new byte[16];
                        else
                            outMsg = new byte[16 + o.tComp.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes((int)TxI.SIGNIN_AL), 0, outMsg, 0, 4);
                        if (o.tComp == null)
                        {
                            Buffer.BlockCopy(BitConverter.GetBytes(0), 0, outMsg, 4, 4);
                            offs = 8;
                        }
                        else
                        {
                            byte[] comp = Encoding.UTF8.GetBytes(o.tComp);
                            Buffer.BlockCopy(BitConverter.GetBytes(comp.Length), 0, outMsg, 4, 4);
                            offs = 8;
                            Buffer.BlockCopy(comp, 0, outMsg, offs, o.tComp.Length);
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
                    int uid = BitConverter.ToInt32(buf, offs);
                    ExamSlot slo = null;
                    foreach (ExamSlot s in mBrd.vSl.Values)
                        foreach(ExamRoom r in s.vRoom.Values)
                            if(r.vExaminee.ContainsKey(uid))
                            {
                                slo = s;
                                break;
                            }
                    if(slo == null)
                    {
                        outMsg = new byte[4];
                        Array.Copy(BitConverter.GetBytes((int)TxI.NEEID_NF), 0, outMsg, 0, 4);
                        break;
                    }
                    ExamLv lv = (uid < (int)ExamLv.B) ? ExamLv.A : ExamLv.B;
                    offs += 4;
                    int qsid = BitConverter.ToInt32(buf, offs);
                    if (qsid == ushort.MaxValue)
                    {
                        byte[] a = slo.vQPack[lv].ToByteNextQS();
                        if (a != null)
                        {
                            outMsg = new byte[a.Length + 4];
                            Array.Copy(BitConverter.GetBytes(0), outMsg, 4);
                            Array.Copy(a, 0, outMsg, 4, a.Length);
                        }
                    }
                    else if (slo.vQPack[lv].vSheet.TryGetValue(qsid, out qs))
                    {
                        outMsg = new byte[qs.aQuest.Length + 4];
                        Array.Copy(BitConverter.GetBytes(0), outMsg, 4);
                        Array.Copy(qs.aQuest, 0, outMsg, 4, qs.aQuest.Length);
                    }
                    if (outMsg == null)
                    {
                        mCbMsg += Txt.s._[(int)TxI.QS_NFOUND] + ((int)lv + qsid);
                        outMsg = new byte[8];
                        Array.Copy(BitConverter.GetBytes((int)TxI.QS_NFOUND), 0, outMsg, 0, 4);
                        Array.Copy(BitConverter.GetBytes(qsid), 0, outMsg, 4, 4);
                        if (!bQShReqting)
                        {
                            bStrtReqQSh = true;
                            uReqQSh = (int)lv + qsid;
                        }
                    }
                    break;
                case NetCode.Submiting:
                    e = new ExamineeS1();
                    e.bFromC = true;
                    if (!e.ReadByte(buf, ref offs))
                    {
                        AnsSheet keySh = null;
                        found = false;
                        foreach(ExamSlot sl in mBrd.vSl.Values)
                            if(sl.mKeyPack.vSheet.TryGetValue(e.mAnsSh.uQSId, out keySh))
                            {
                                found = true;
                                break;
                            }
                        if (!found)
                        {
                            outMsg = BitConverter.GetBytes(101);//todo
                            break;
                        }
                        ExamineeA o = null;
                        lvid = e.mLv + e.uId;
                        found = false;
                        foreach (ExamSlot sl in mBrd.vSl.Values)
                            if ((o = sl.Find(lvid)) != null)
                                break;
                        if (o != null)
                        {
                            o.eStt = ExamStt.Finished;
                            o.mAnsSh = e.mAnsSh;
                            o.uGrade = keySh.Grade(e.mAnsSh.aAns);
                            o.dtTim2 = DateTime.Now;
                            foreach (SortedList<int, bool> sl in vfbLock)
                                if (sl.ContainsKey(lvid))
                                    sl[lvid] = true;
                            Thread th = new Thread(() =>
                                Dispatcher.Invoke(() =>
                                {
                                    foreach (TabItem ti in tbcSl.Items)
                                    {
                                        Op1SlotView vw = ti.Content as Op1SlotView;
                                        if (vw != null)
                                        {
                                            TextBlock t = null;
                                            if (vw.vDt2.TryGetValue(lvid, out t))
                                                t.Text = o.dtTim2.ToString("HH:mm");
                                            if (vw.vMark.TryGetValue(lvid, out t))
                                                t.Text = o.Grade.ToString();
                                            CheckBox cbx;
                                            if (vw.vLock.TryGetValue(lvid, out cbx))
                                            {
                                                cbx.IsChecked = true;
                                                cbx.IsEnabled = false;
                                            }
                                        }
                                    }
                                }));
                            th.Start();
                            o.ToByte(out outMsg, 0);
                        }
                        else
                        {
                            mCbMsg += Txt.s._[(int)TxI.NEEID_NF] + ' ' + lvid;
                            outMsg = BitConverter.GetBytes((int)TxI.NEEID_NF);
                        }
                    }
                    else
                    {
                        mCbMsg += Txt.s._[(int)TxI.RECV_DAT_ER];
                        outMsg = BitConverter.GetBytes((int)TxI.RECV_DAT_ER);
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
                    if (mBrd.ReadByteR1(buf, ref offs))
                        break;//show err msg
                    Dispatcher.Invoke(() => LoadSl());
                    mState = NetCode.QuestRetrieving;
                    return true;
                case NetCode.QuestRetrieving:
                    if (mBrd.ReadByteQPack(buf, ref offs))
                        break;//show err msg
                    mState = NetCode.AnsKeyRetrieving;
                    return true;
                case NetCode.AnsKeyRetrieving:
                    if (mBrd.ReadByteKey(buf, ref offs))
                        offs = 0;//todo handle error
                    break;
                case NetCode.RequestQuestSheet:
                    //int rs = BitConverter.ToInt32(buf, offs);
                    //offs += 4;
                    //if(rs == 0)
                    //{
                    //    ExamLv lv;
                    //    if (uReqQSh < (int)ExamLv.B)
                    //        lv = ExamLv.A;
                    //    else
                    //        lv = ExamLv.B;
                    //    if (mSl.ReadByteQPack1(lv, buf, ref offs))
                    //        offs = 0;//todo handle error
                    //    else
                    //    {
                    //        if (mSl.mKeyPack.ReadByte1(buf, ref offs))
                    //            offs = 0;//todo handle error
                    //        else
                    //            Dispatcher.Invoke(() => ShowQuest());
                    //    }
                    //}
                    //btnStartSrvr_Click(null, null);
                    //bQShReqting = false;
                    //mState = NetCode.Unknown;
                    break;
                case NetCode.SrvrSubmitting:
                    //if (buf.Length - offs == 4 && BitConverter.ToInt32(buf, offs) == 1)
                    //    mCbMsg += Txt.s._[(int)TxI.SRVR_SUBMT_OK];
                    break;
            }
            return false;
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

        private void lbxSl_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            foreach (TabItem t in tbcSl.Items)
                if (t.Name == "_" + (i.Content as string).Replace(':', '_'))
                    return;
            ExamSlot sl;
            if (!mBrd.vSl.TryGetValue(i.Content as string, out sl))
                return;

            Op1SlotView vw = new Op1SlotView();
            vw.ShallowCopy(refSl);
            vw.mSl = sl;
            vw.ShowExaminee();
            vw.ShowQuest();
            vfbLock.Add(vw.vbLock);
            TabItem ti = new TabItem();
            ti.Name = "_" + (i.Content as string).Replace(':', '_');
            ti.Header = sl.Dt.ToString(DtFmt.hh);
            ti.Content = vw;
            tbcSl.Items.Add(ti);
            ti.Focus();
        }

        private void lbxSl_Unselected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            mBrd.vSl.Remove(i.Content as string);
            foreach (TabItem ti in tbcSl.Items)
                if (ti.Name == "_" + (i.Content as string).Replace(':', '_'))
                {
                    tbcSl.Items.Remove(ti);
                    break;
                }
        }

        private void LoadSl()
        {
            List<DateTime> v = mBrd.ListSl();
            //bool dark = true;
            //Color c = new Color();
            //c.A = 0xff;
            //c.B = c.G = c.R = 0xf0;
            lbxSl.Items.Clear();
            foreach (DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DtFmt.hh);
                it.Selected += lbxSl_Selected;
                it.Unselected += lbxSl_Unselected;
                //dark = !dark;
                //if (dark)
                //    it.Background = new SolidColorBrush(c);
                lbxSl.Items.Add(it);
            }
        }
    }
}
