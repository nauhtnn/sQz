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
        List<SortedList<int, bool>> vfbLock;

        public Operation1()
        {
            InitializeComponent();

            mState = NetCode.Srvr1DatRetriving;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, true);
            mServer = new Server2(SrvrBufHndl);
            mServer.SrvrPort = 23821;
            mCbMsg = new UICbMsg();
            bRunning = true;

            mBrd = new ExamBoard();

            if(!System.IO.File.Exists("Room.txt") ||
                !int.TryParse(System.IO.File.ReadAllText("Room.txt"), out uRId))
                uRId = 0;

            vfbLock = new List<SortedList<int, bool>>();

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 13;

            WPopup.nwIns(w);

            spMain.Background = Theme.s._[(int)BrushId.BG];

            LoadTxt();
        }

        private void btnConn_Click(object sender, RoutedEventArgs e)
        {
            btnStop_Click(null, null);
            //todo: check th state to return
            Task.Run(() => { mClnt.ConnectWR(ref mCbMsg); });
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { mServer.Start(ref mCbMsg); });
            btnStrt.IsEnabled = false;
            btnStrt.Foreground = Theme.s._[(int)BrushId.FG_Gray];
            btnStrt.Background = Theme.s._[(int)BrushId.BG_Gray];
            btnStop.IsEnabled = true;
            btnStop.Foreground = Theme.s._[(int)BrushId.FG];
            btnStop.Background = Theme.s._[(int)BrushId.mReconn];
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref mCbMsg);
            btnStop.IsEnabled = false;
            btnStop.Foreground = Theme.s._[(int)BrushId.FG_Gray];
            btnStop.Background = Theme.s._[(int)BrushId.BG_Gray];
            btnStrt.IsEnabled = true;
            btnStrt.Foreground = Theme.s._[(int)BrushId.FG];
            btnStrt.Background = Theme.s._[(int)BrushId.mConn];
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
                    outMsg = new byte[DT.BYTE_COUNT];
                    offs = 0;
                    DT.ToByte(outMsg, ref offs, mBrd.mDt);
                    return true;
                case NetCode.Authenticating:
                    e = new ExamineeS1();
                    e.bFromC = true;
                    e.ReadByte(buf, ref offs);
                    bool lck = false;
                    bool found = false;
                    foreach (SortedList<int, bool> l in vfbLock)
                        if (l.TryGetValue(e.LvId, out lck))
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
                            if (o.dtTim1.Hour == DT.INV)
                                o.dtTim1 = DateTime.Now;
                            Dispatcher.InvokeAsync(() =>
                            {
                                foreach(Op1SlotView vw in tbcSl.Items.OfType<Op1SlotView>())
                                {
                                    TextBlock t;
                                    lvid = o.LvId;
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
                            if ((o = sl.Find(e.LvId)) != null)
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

                        Buffer.BlockCopy(BitConverter.GetBytes(o.dtTim1.Hour), 0, outMsg, offs, 4);
                        offs += 4;
                        Buffer.BlockCopy(BitConverter.GetBytes(o.dtTim1.Minute), 0, outMsg, offs, 4);
                        break;
                    }
                    return true;
                case NetCode.ExamRetrieving:
                    outMsg = null;
                    lvid = BitConverter.ToInt32(buf, offs);
                    ExamSlot slo = null;
                    foreach (ExamSlot s in mBrd.vSl.Values)
                        foreach(ExamRoom r in s.vRoom.Values)
                            if(r.vExaminee.ContainsKey(lvid))
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
                    ExamLv lv = (lvid < ExamineeA.LV_CAP) ? ExamLv.A : ExamLv.B;
                    offs += 4;
                    int qsid = BitConverter.ToInt32(buf, offs);
                    if (qsid == ExamineeA.LV_CAP)
                    {
                        byte[] a = slo.ToByteNextQS(lv);
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
                        mCbMsg += Txt.s._[(int)TxI.QS_NFOUND] + (qsid);
                        outMsg = new byte[8];
                        Array.Copy(BitConverter.GetBytes((int)TxI.QS_NFOUND), 0, outMsg, 0, 4);
                        Array.Copy(BitConverter.GetBytes(qsid), 0, outMsg, 4, 4);
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
                            if(sl.mKeyPack.vSheet.TryGetValue(e.mAnsSh.uQSLvId, out keySh))
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
                        lvid = e.LvId;
                        found = false;
                        foreach (ExamSlot sl in mBrd.vSl.Values)
                            if ((o = sl.Find(lvid)) != null)
                                break;
                        if (o != null)
                        {
                            o.eStt = NeeStt.Finished;
                            o.mAnsSh = e.mAnsSh;
                            o.uGrade = keySh.Grade(e.mAnsSh.aAns);
                            o.dtTim2 = DateTime.Now;
                            foreach (SortedList<int, bool> sl in vfbLock)
                                if (sl.ContainsKey(lvid))
                                    sl[lvid] = true;
                            Dispatcher.InvokeAsync(() =>
                            {
                                bool toSubm = true;
                                foreach (Op1SlotView vw in tbcSl.Items.OfType<Op1SlotView>())
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
                                    if(vw.vAbsen.TryGetValue(lvid, out cbx))
                                        cbx.IsChecked = cbx.IsEnabled = false;
                                    if (!vw.ToSubmit())
                                        toSubm = false;
                                }
                                if (toSubm)
                                    ToSubmit(true);
                            });
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
                case NetCode.Srvr1Auth:
                    if (buf.Length - offs < 4)
                        break;
                    int rs = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    if (rs == (int)TxI.OP_AUTH_OK)
                    {
                        mState = NetCode.Srvr1DatRetriving;
                        return true;
                    }
                    else
                        WPopup.s.ShowDialog(Txt.s._[(int)TxI.OP_AUTH_NOK]);
                    break;
                case NetCode.Srvr1DatRetriving:
                    if (mBrd.ReadByteSl1(buf, ref offs))
                        break;//show err msg
                    Dispatcher.InvokeAsync(() => LoadSl());
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
                    else
                        Dispatcher.InvokeAsync(()=> {
                            btnStrt.IsEnabled = true;
                            btnStrt.Foreground = Theme.s._[(int)BrushId.FG];
                            btnStrt.Background = Theme.s._[(int)BrushId.mConn];
                        });
                    break;
                case NetCode.SrvrSubmitting:
                    if (buf.Length - offs == 4 && BitConverter.ToInt32(buf, offs) == 1)
                    {
                        //mCbMsg += Txt.s._[(int)TxI.SRVR_SUBMT_OK];
                        Dispatcher.InvokeAsync(() =>
                        {
                            btnSubmit.IsEnabled = false;
                            btnSubmit.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                            btnSubmit.Background = Theme.s._[(int)BrushId.BG_Gray];
                        });
                    }
                    break;
            }
            return false;
        }

        public byte[] ClntBufPrep()
        {
            byte[] outMsg = null;
            switch (mState)
            {
                case NetCode.Srvr1DatRetriving:
                    outMsg = new byte[8];
                    Array.Copy(BitConverter.GetBytes((int)mState), 0, outMsg, 0, 4);
                    Array.Copy(BitConverter.GetBytes(uRId), 0, outMsg, 4, 4);
                    break;
                case NetCode.QuestRetrieving:
                    outMsg = new byte[8];
                    Buffer.BlockCopy(BitConverter.GetBytes((int)mState), 0, outMsg, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(uRId), 0, outMsg, 4, 4);
                    break;
                case NetCode.AnsKeyRetrieving:
                    outMsg = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.SrvrSubmitting:
                    outMsg = mBrd.ToByteSl0(BitConverter.GetBytes((int)NetCode.SrvrSubmitting));
                    break;
            }
            return outMsg;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            //todo: check th state to return
            mState = NetCode.SrvrSubmitting;
            Task.Run(() => { mClnt.ConnectWR(ref mCbMsg); });
        }

        void LoadTxt()
        {
            Txt s = Txt.s;
            btnClose.Content = s._[(int)TxI.EXIT];
            btnConn.Content = s._[(int)TxI.CONN];
            btnStrt.Content = s._[(int)TxI.STRT_SRVR];
            btnStop.Content = s._[(int)TxI.STOP_SRVR];
            btnSubmit.Content = s._[(int)TxI.SUBMIT];

            txtId.Text = s._[(int)TxI.NEEID_S];
            txtName.Text = s._[(int)TxI.NEE_NAME];
            txtBirdate.Text = s._[(int)TxI.BIRDATE];
            txtBirpl.Text = s._[(int)TxI.BIRPL];
            txtComp.Text = s._[(int)TxI.COMP];
            txtT1.Text = s._[(int)TxI.T1];
            txtT2.Text = s._[(int)TxI.T2];
            txtGrade.Text = s._[(int)TxI.MARK];
            txtLock.Text = s._[(int)TxI.OP_LCK];
            txtAbsence.Text = s._[(int)TxI.OP_ABSENCE];
        }

        void ToSubmit(bool bEnable)
        {
            if(bEnable)
            {
                btnSubmit.IsEnabled = true;
                btnSubmit.Foreground = Theme.s._[(int)BrushId.FG];
                btnSubmit.Background = Theme.s._[(int)BrushId.mSubmit];
            }
            else
            {
                btnSubmit.IsEnabled = false;
                btnSubmit.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                btnSubmit.Background = Theme.s._[(int)BrushId.BG_Gray];
            }
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
            vw.mSl = sl;
            vw.DeepCopyNee(tbiRefNee);
            vw.ShowExaminee();
            vfbLock.Add(vw.vbLock);
            vw.Name = "_" + (i.Content as string).Replace(':', '_');
            vw.Header = sl.Dt.ToString(DT.hh);
            vw.toSubmCb = ToSubmit;
            tbcSl.Items.Add(vw);
            vw.Focus();
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
                it.Content = dt.ToString(DT.hh);
                it.Selected += lbxSl_Selected;
                it.Unselected += lbxSl_Unselected;
                //dark = !dark;
                //if (dark)
                //    it.Background = new SolidColorBrush(c);
                lbxSl.Items.Add(it);
            }
        }

        private void btnHck_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists("Hck.txt"))
            {
                string t = System.IO.File.ReadAllText("Hck.txt");
                if (t == "allButtons")
                {
                    btnConn.IsEnabled = btnStrt.IsEnabled =
                        btnStop.IsEnabled = btnSubmit.IsEnabled = true;
                    btnConn.Foreground = btnStrt.Foreground =
                        btnStop.Foreground = btnSubmit.Foreground =
                        Theme.s._[(int)BrushId.mSubmit];
                }
            }
        }
    }
}
