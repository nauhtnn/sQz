using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Threading;
using sQzLib;

namespace sQzServer0
{
    /// <summary>
    /// Interaction logic for Operation.xaml
    /// </summary>
    public partial class Operation0 : Page
    {
        Server2 mServer;
        UICbMsg mCbMsg;
        bool bRunning;
        ExamSlot Slot;
        TabItem tbiSelected;

        public Operation0()
        {
            InitializeComponent();
            mServer = new Server2(SrvrBufHndl);
            mCbMsg = new UICbMsg();

            //mBrd = new ExamBoard();

            bRunning = true;

            tbiSelected = null;
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            UICbMsg dummy = new UICbMsg();
            mServer.Stop(ref dummy);
        }

        private void LoadBrd()
        {
            string emsg;
            List<DateTime> v = ExamSlot.DBSelectSlotIDs(false, out emsg);
            if (v == null)
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(emsg);
                spMain.Opacity = 1;
                return;
            }
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            lbxBrd.Items.Clear();
            foreach (DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DT._);
                dark = !dark;
                if (dark)
                    it.Background = new SolidColorBrush(c);
                lbxBrd.Items.Add(it);
            }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Application.Current.MainWindow;
            w.Closing += W_Closing;
            w.FontSize = 16;

            LoadTxt();

            LoadBrd();

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            foreach(TabItem i in tbcSl.Items)
            {
                Op0SlotView vw = i as Op0SlotView;
                if (vw == null)
                    continue;
                if (vw.mSl.eStt == ExamStt.Prep)
                {
                    vw.mSl.eStt = ExamStt.Oper;
                    vw.mSl.DBUpStt();
                }
            }
            DisableQSGen();
            Thread th = new Thread(() => {mServer.Start(ref mCbMsg);});
            th.Start();
            btnStop.IsEnabled = true;
            btnStop.Foreground = Theme.s._[(int)BrushId.FG];
            btnStop.Background = Theme.s._[(int)BrushId.mReconn];
            btnStart.IsEnabled = false;
            btnStart.Foreground = Theme.s._[(int)BrushId.FG_Gray];
            btnStart.Background = Theme.s._[(int)BrushId.BG_Gray];
        }

        private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            //if (bRunning && mCbMsg.ToUp())
            //    Dispatcher.Invoke(() => {
            //        lblStatus.Text += mCbMsg.txt; });
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref mCbMsg);
            btnStop.IsEnabled = false;
            btnStop.Foreground = Theme.s._[(int)BrushId.FG_Gray];
            btnStop.Background = Theme.s._[(int)BrushId.BG_Gray];
            btnStart.IsEnabled = true;
            btnStart.Foreground = Theme.s._[(int)BrushId.FG];
            btnStart.Background = Theme.s._[(int)BrushId.mConn];
        }

        private void btnMMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("MainMenu.xaml", UriKind.Relative));
        }

        public bool SrvrBufHndl(byte[] buf, out byte[] outMsg)
        {
            int offs = 0;
            NetCode c = (NetCode)BitConverter.ToInt32(buf, offs);
            offs += 4;
            int roomId;
            switch (c)
            {
                case NetCode.Srvr1Auth:
                    outMsg = BitConverter.GetBytes((int)TxI.OP_AUTH_NOK);
                    if (buf.Length - offs < 12)
                        break;
                    roomId = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    string pw = Encoding.ASCII.GetString(buf, offs, 8);
                    offs += 8;
                    if(Slot.vRoom.ContainsKey(roomId))
                    {
                        if(pw == Slot.vRoom[roomId].tPw)
                        {
                            outMsg = BitConverter.GetBytes((int)TxI.OP_AUTH_OK);
                            break;
                        }
                    }
                    break;
                case NetCode.Srvr1DatRetriving:
                    if (buf.Length - offs < 4)
                    {
                        outMsg = null;
                        break;
                    }
                    roomId = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    outMsg = Slot.ToByteR1(roomId);
                    Dispatcher.InvokeAsync(() =>
                    {
                        foreach (Op0SlotView vw in tbcSl.Items.OfType<Op0SlotView>())
                            vw.UpRT1(roomId);
                    });
                    return true;
                case NetCode.QuestRetrieving:
                    if(buf.Length - offs < 4)
                    {
                        outMsg = null;
                        break;
                    }
                    roomId = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    outMsg = Slot.ToByteQPack(roomId);
                    return true;
                case NetCode.AnsKeyRetrieving:
                    outMsg = Slot.ToByteKey();
                    break;
                case NetCode.SrvrSubmitting:
                    int rid;
                    if (-1 < (rid = Slot.ReadByteSl0(buf, ref offs)))
                    {
                        string emsg;
                        if (Slot.DBUpdateRs(rid, out emsg))
                            mCbMsg += emsg;
                        else if (emsg == null)
                        {
                            mCbMsg += Txt.s._((int)TxI.SRVR_DB_OK);
                            Dispatcher.InvokeAsync(() =>
                            {
                                foreach (Op0SlotView vw in tbcSl.Items.OfType<Op0SlotView>())
                                    vw.UpdateRsView(rid);
                            });
                        }
                        Slot.DBSafeUpdateArchiveStatus();
                    }
                    outMsg = BitConverter.GetBytes(1);
                    break;
                default:
                    outMsg = null;
                    break;
            }
            return false;
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;

            btnStart.Content = t._((int)TxI.STRT_SRVR);
            btnStop.Content = t._((int)TxI.STOP_SRVR);
            btnMMenu.Content = t._((int)TxI.BACK_MMENU);
            btnQGen.Content = t._((int)TxI.QS_GEN);
            btnQSav.Content = t._((int)TxI.OP_Q_SAV);

            txtDate.Text = DT._;

            txtMod.Text = t._((int)TxI.MODULE);
            txtNEsyDif.Text = t._((int)TxI.N_ESY_DIF);
            txtNDiff.Text = t._((int)TxI.N_DIFF);
            txtnQs.Text = t._((int)TxI.QS_N);
            txtnQ.Text = t._((int)TxI.Q_N);
            txtBirdate.Text = t._((int)TxI.BIRDATE);
            txtBirpl.Text = t._((int)TxI.BIRPL);
            txtName.Text = t._((int)TxI.NEE_NAME);
            txtId.Text = t._((int)TxI.NEEID_S);
            txtGrade.Text = t._((int)TxI.MARK);
            txtRoom.Text = t._((int)TxI.ROOM);
            txtT1.Text = t._((int)TxI.T1);
            txtT2.Text = t._((int)TxI.T2);
            txtComp.Text = t._((int)TxI.COMP);

            txtRId.Text = t._((int)TxI.ROOM);
            txtRN.Text = t._((int)TxI.OP_N_NEE);
            txtRT1.Text = t._((int)TxI.T1);
            txtRT2.Text = t._((int)TxI.T2);
            txtRQPack.Text = t._((int)TxI.OP_Q);
            txtQPackR0.Text = t._((int)TxI.OP_PRI);
            txtQPackR1.Text = t._((int)TxI.OP_ALT);
            txtPw.Text = t._((int)TxI.OP_PW);
        }

        private void lbxBrd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbcSl.Items.Clear();
            if(lbxBrd.SelectedItem != null)
                lbxSl_Selected(lbxBrd.SelectedItem, null);
        }

        void DisableQSGen()
        {
            btnQGen.IsEnabled = false;
            btnQGen.Foreground = Theme.s._[(int)BrushId.FG_Gray];
            btnQGen.Background = Theme.s._[(int)BrushId.BG_Gray];
        }

        void EnableQSGen()
        {
            
        }

        private void lbxSl_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;

            Slot = new ExamSlot();
            DateTime dt;
            DT.To_(i.Content as string, out dt);
            Slot.Dt = dt;
            string emsg;
            if ((emsg = Slot.DBSelRoomId()) != null)
            {
                WPopup.s.ShowDialog(emsg);
                return;
            }
            Slot.DBSelStt();
            Slot.DBSelNee();
            if (Slot.DBSelArchieve(out emsg))
            {
                WPopup.s.ShowDialog(emsg);
                return;
            }
            Op0SlotView tbi = new Op0SlotView(Slot);
            tbi.DeepCopy(tbcRefSl);
            tbi.ShowExaminee();
            tbi.ShowQSHeader();
            tbcSl.Items.Add(tbi);
            QuestSheet.GetMaxID_inDB(Slot.Dt);
            if ((tbi = tbcSl.SelectedItem as Op0SlotView) != null &&
                    tbi.mSl.eStt == ExamStt.Prep)
                EnableQSGen();
            else
                DisableQSGen();
        }

        private void btnQGen_Click(object sender, RoutedEventArgs e)
        {
            Op0SlotView vw = tbcSl.SelectedItem as Op0SlotView;
            if (vw == null)
                return;
            vw.GenQ();
        }

        private void tbcSl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tbc = sender as TabControl;
            if (tbc == null)
                return;
            TabItem tbi = tbc.SelectedItem as TabItem;
            if (tbi == null || tbiSelected == tbi)
                return;
            tbiSelected = tbi;
            Op0SlotView vw = tbi as Op0SlotView;
            if (vw == null)
            {
                btnQGen.IsEnabled = false;
                btnQGen.Background = Theme.s._[(int)BrushId.BG_Gray];
                btnQGen.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                return;
            }
            if ((vw = tbcSl.SelectedItem as Op0SlotView) != null &&
                    vw.mSl.eStt == ExamStt.Prep)
                EnableQSGen();
            else
                DisableQSGen();
        }
    }
}
