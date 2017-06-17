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
        ExamBoard mBrd;

        public Operation0()
        {
            InitializeComponent();
            ShowsNavigationUI = false;
            mServer = new Server2(SrvrBufHndl);
            mCbMsg = new UICbMsg();

            mBrd = new ExamBoard();

            bRunning = true;
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
            List<DateTime> v = ExamBoard.DBSel(out emsg);
            if (v == null)
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(emsg);
                spMain.Opacity = 1;
                return;
            }
            //bool dark = true;
            //Color c = new Color();
            //c.A = 0xff;
            //c.B = c.G = c.R = 0xf0;
            lbxBrd.Items.Clear();
            foreach (DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DT.__);
                //dark = !dark;
                //if (dark)
                //    it.Background = new SolidColorBrush(c);
                lbxBrd.Items.Add(it);
            }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 13;

            spMain.Background = Theme.s._[(int)BrushId.Ans_Highlight];

            LoadTxt();

            LoadBrd();

            double rt = spMain.RenderSize.Width / 1280;
            spMain.RenderTransform = new ScaleTransform(rt, rt);

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void btnStartSrvr_Click(object sender, RoutedEventArgs e)
        {
            Thread th = new Thread(() => {mServer.Start(ref mCbMsg);});
            th.Start();
        }

        private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning && mCbMsg.ToUp())
                Dispatcher.Invoke(() => {
                    lblStatus.Text += mCbMsg.txt; });
        }

        private void btnStopSrvr_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref mCbMsg);
        }

        private void btnPrep_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                NavigationService.Navigate(new Uri("Prep0.xaml", UriKind.Relative));
            });
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        public bool SrvrBufHndl(byte[] buf, out byte[] outMsg)
        {
            int offs = 0;
            NetCode c = (NetCode)BitConverter.ToInt32(buf, offs);
            offs += 4;
            switch (c)
            {
                case NetCode.DateStudentRetriving:
                    if (buf.Length - offs < 4)
                    {
                        outMsg = null;
                        break;
                    }
                    int rId = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    outMsg = mBrd.ToByteSl1(rId);
                    return true;
                case NetCode.QuestRetrieving:
                    outMsg = mBrd.ToByteQPack();
                    return true;
                case NetCode.AnsKeyRetrieving:
                    outMsg = mBrd.ToByteKey();
                    break;
                case NetCode.RequestQuestSheet:
                    //if (buf.Length - offs == 4)
                    //{
                    //    int qsId = BitConverter.ToInt32(buf, offs);
                    //    offs += 4;
                    //    QuestSheet qs = new QuestSheet();
                    //    ExamLv lv;
                    //    if (qsId < (int)ExamLv.B)
                    //        lv = ExamLv.A;
                    //    else
                    //        lv = ExamLv.B;
                    //    bool found = false;

                    //    if (qs.DBSelect(curSl.uId, lv, qsId))
                    //        outMsg = BitConverter.GetBytes(-1);
                    //    else
                    //    {
                    //        curSl.vQPack[lv].vSheet.Add(qs.uId, qs);
                    //        AnsSheet a = curSl.mKeyPack.ExtractKey(qs);
                    //        List<byte[]> bs = qs.ToByte();
                    //        sz = 4;
                    //        if (a != null)
                    //            sz += a.GetByteCount();
                    //        foreach (byte[] b in bs)
                    //            sz += b.Length;
                    //        outMsg = new byte[sz];
                    //        Array.Copy(BitConverter.GetBytes(0), 0, outMsg, 0, 4);
                    //        offs = 4;
                    //        foreach (byte[] b in bs)
                    //        {
                    //            Array.Copy(b, 0, outMsg, offs, b.Length);
                    //            offs += b.Length;
                    //        }
                    //        if (a != null)
                    //            a.ToByte(ref outMsg, ref offs);
                    //        Dispatcher.Invoke(() => ShowQuest());
                    //    }
                    //}
                    //else
                    //    outMsg = BitConverter.GetBytes(-1);
                    outMsg = BitConverter.GetBytes(-1);
                    break;
                case NetCode.SrvrSubmitting:
                    if (!mBrd.ReadByteSl0(buf, ref offs))
                    {
                        string emsg;
                        if (mBrd.DBUpdateRs(out emsg))
                            mCbMsg += emsg;
                        else
                        {
                            mCbMsg += Txt.s._[(int)TxI.SRVR_DB_OK];
                            //Dispatcher.Invoke(() =>
                            //{
                            //    foreach (TabItem ti in tbcSl.Items)
                            //    {
                            //        Op0SlotView vw = ti.Content as Op0SlotView;
                            //        if (vw != null)
                            //            vw.UpdateRsView();
                            //    }
                            //});
                        }
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
            btnExit.Content = t._[(int)TxI.EXIT];
            btnStartSrvr.Content = t._[(int)TxI.STRT_SRVR];
            btnStopSrvr.Content = t._[(int)TxI.STOP_SRVR];
            btnQSGen.Content = t._[(int)TxI.QS_GEN];
            btnExit.Content = t._[(int)TxI.EXIT];
            txtNqs.Text = t._[(int)TxI.QS_N];
            txtNq.Text = t._[(int)TxI.Q_N];
            txtBirdate.Text = t._[(int)TxI.BIRDATE];
            txtBirpl.Text = t._[(int)TxI.BIRPL];
            txtName.Text = t._[(int)TxI.NEE_NAME];
            txtId.Text = t._[(int)TxI.NEEID_S];
            txtGrade.Text = t._[(int)TxI.MARK];
        }

        private void lbxBrd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<TabItem> toRem = new List<TabItem>();
            foreach (TabItem ti in tbcSl.Items)
                if (ti.Header as string != "_0")
                    toRem.Add(ti);
            foreach (TabItem ti in toRem)
                tbcSl.Items.Remove(ti);
            ListBox l = sender as ListBox;
            ListBoxItem i = l.SelectedItem as ListBoxItem;
            if (i == null)
            {
                lbxSl.IsEnabled = false;
                return;
            }
            DateTime dt;
            if (!DT.To_(i.Content as string, DT._, out dt))
            {
                mBrd.mDt = dt;
                lbxSl.IsEnabled = true;
                LoadSl();
            }
        }

        private void LoadSl()
        {
            string emsg;
            List<DateTime> v = mBrd.DBSelSl(out emsg);
            if (v == null)
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(emsg);
                spMain.Opacity = 1;
            }
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

        private void lbxSl_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            if (mBrd.vSl.ContainsKey(i.Content as string))
                return;

            ExamSlot sl = new ExamSlot();
            string emsg;
            if ((emsg = sl.DBSelRoom()) != null)
            {
                WPopup.s.ShowDialog(emsg);
                return;
            }
            DateTime dt;
            DT.To_(mBrd.mDt.ToString(DT._) + ' ' + i.Content as string, DT.H, out dt);
            sl.Dt = dt;
            sl.DBSelRoom();
            sl.DBSelNee();
            Op0SlotView tbi = new Op0SlotView(sl);
            tbi.DeepCopy(tbiSl);
            tbi.ShowExaminee();
            tbcSl.Items.Add(tbi);
            tbi.Focus();
            QuestSheet.DBUpdateCurQSId(mBrd.mDt);
            mBrd.vSl.Add(i.Content as string, sl);
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
    }
}
