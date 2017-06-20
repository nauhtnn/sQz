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
        Dictionary<ExamLv, TextBox[]> vtxtNEsyDif;
        Dictionary<ExamLv, TextBox[]> vtxtNDiff;
        TabItem tbiSelected;

        public Operation0()
        {
            InitializeComponent();
            mServer = new Server2(SrvrBufHndl);
            mCbMsg = new UICbMsg();

            mBrd = new ExamBoard();

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
            List<DateTime> v = ExamBoard.DBSel(out emsg);
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
                it.Content = dt.ToString(DT.__);
                dark = !dark;
                if (dark)
                    it.Background = new SolidColorBrush(c);
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

            vtxtNEsyDif = new Dictionary<ExamLv, TextBox[]>();
            vtxtNEsyDif.Add(ExamLv.A, new TextBox[QuestSheet.GetIUs(ExamLv.A).Count()]);
            vtxtNEsyDif.Add(ExamLv.B, new TextBox[QuestSheet.GetIUs(ExamLv.B).Count()]);
            vtxtNDiff = new Dictionary<ExamLv, TextBox[]>();
            vtxtNDiff.Add(ExamLv.A, new TextBox[QuestSheet.GetIUs(ExamLv.A).Count()]);
            vtxtNDiff.Add(ExamLv.B, new TextBox[QuestSheet.GetIUs(ExamLv.B).Count()]);
            int i = -1, j = -1;
            foreach (TextBox tbx in grdA.Children.OfType<TextBox>())
            {
                if (Grid.GetColumn(tbx) == 1)
                    vtxtNEsyDif[ExamLv.A][++i] = tbx;
                else
                {
                    vtxtNDiff[ExamLv.A][++j] = tbx;
                    tbx.Name = "_" + j;
                }
            }
            i = j = -1;
            foreach (TextBox tbx in grdB.Children.OfType<TextBox>())
            {
                if (Grid.GetColumn(tbx) == 1)
                    vtxtNEsyDif[ExamLv.B][++i] = tbx;
                else
                {
                    vtxtNDiff[ExamLv.B][++j] = tbx;
                    tbx.Name = "_" + j;
                }
            }

            LoadTxt();
            InitQPanel();

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

        private void btnMMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("MainMenu.xaml", UriKind.Relative));
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
            btnStartSrvr.Content = t._[(int)TxI.STRT_SRVR];
            btnStopSrvr.Content = t._[(int)TxI.STOP_SRVR];
            btnMMenu.Content = t._[(int)TxI.BACK_MMENU];
            btnQGen.Content = t._[(int)TxI.QS_GEN];
            rdoA.Content = t._[(int)TxI.BASIC];
            rdoB.Content = t._[(int)TxI.ADVAN];
            txtMod.Text = t._[(int)TxI.MODULE];
            txtNEsyDif.Text = t._[(int)TxI.N_ESY_DIF];
            txtNDiff.Text = t._[(int)TxI.N_DIFF];
            txtnQs.Text = t._[(int)TxI.QS_N];
            txtnQ.Text = t._[(int)TxI.Q_N];
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
            if ((emsg = sl.DBSelRoomId()) != null)
            {
                WPopup.s.ShowDialog(emsg);
                return;
            }
            DateTime dt;
            DT.To_(mBrd.mDt.ToString(DT._) + ' ' + i.Content as string, DT.H, out dt);
            sl.Dt = dt;
            sl.DBSelRoomId();
            sl.DBSelNee();
            if(sl.DBSelArchieve(out emsg))
            {
                WPopup.s.ShowDialog(emsg);
                return;
            }
            Op0SlotView tbi = new Op0SlotView(sl);
            tbi.DeepCopy(tbiSl);
            tbi.ShowExaminee();
            tbi.ShowQSHeader();
            tbcSl.Items.Add(tbi);
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

        public void InitQPanel()
        {
            ExamLv[] l = new ExamLv[2];
            l[0] = ExamLv.A;
            l[1] = ExamLv.B;
            foreach(ExamLv lv in l)
            {
                int n = QuestSheet.GetIUs(lv).Count();
                for (int i = 0; i < n; ++i)
                {
                    TextBox t = vtxtNEsyDif[lv][i];
                    if (t != null)
                    {
                        t.MaxLength = 2;
                        t.PreviewKeyDown += tbxIU_PrevwKeyDown;
                        t.TextChanged += tbxIU_TextChanged;
                    }
                    t = vtxtNDiff[lv][i];
                    if (t != null)
                    {
                        t.MaxLength = 2;
                        t.PreviewKeyDown += tbxIU_PrevwKeyDown;
                        t.TextChanged += tbxIUdif_TextChanged;
                    }
                }
            }
        }

        private void tbxIU_PrevwKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete && e.Key != Key.Back && e.Key != Key.Tab &&
                ((int)e.Key < (int)Key.Left || (int)Key.Down < (int)e.Key) &&
                ((int)e.Key < (int)Key.D0 || (int)Key.D9 < (int)e.Key))
                e.Handled = true;
        }

        private void Lv_Checked(object sender, RoutedEventArgs e)
        {
            ExamSlot sl = (tbcSl.SelectedItem as Op0SlotView).mSl;
            if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
            {
                grdB.Visibility = Visibility.Collapsed;
                grdA.Visibility = Visibility.Visible;
                txtNqs.Text = sl.CountQSByRoom(ExamLv.A).ToString();
            }
            else
            {
                grdA.Visibility = Visibility.Collapsed;
                grdB.Visibility = Visibility.Visible;
                txtNqs.Text = sl.CountQSByRoom(ExamLv.A).ToString();
            }
            tbxIU_TextChanged(null, null);
        }

        private void tbxIU_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t;
            bool bG = true;
            ExamLv lv;
            if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
                lv = ExamLv.A;
            else
                lv = ExamLv.B;
            int n = 0;
            for(int j = 0; j < vtxtNEsyDif[lv].Length; ++j)
                if ((t = vtxtNEsyDif[lv][j]) != null)
                {
                    if (t.Text != null && 0 < t.Text.Length)
                    {
                        n += int.Parse(t.Text);
                        vtxtNDiff[lv][j].IsEnabled = true;
                    }
                    else
                    {
                        bG = false;
                        vtxtNDiff[lv][j].IsEnabled = false;
                    }
                }
                else
                    bG = false;
            tbxNq.Text = n.ToString();
            if (bG && n == 30)
                btnQGen.IsEnabled = true;
            else
                btnQGen.IsEnabled = false;
        }

        private void tbxIUdif_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t == null)
                return;
            if (t.Text == null || t.Text.Length == 0)
                return;
            ExamLv lv;
            if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
                lv = ExamLv.A;
            else
                lv = ExamLv.B;
            int i = int.Parse(t.Text);
            int idx = int.Parse(t.Name.Substring(1));
            TextBox tm = vtxtNEsyDif[lv][idx];
            if(tm == null)
            {
                t.Text = string.Empty;
                return;
            }
            int m = int.Parse(tm.Text);
            if(m < i)
                t.Text = string.Empty;
        }

        private void btnQGen_Click(object sender, RoutedEventArgs e)
        {
            Op0SlotView vw = tbcSl.SelectedItem as Op0SlotView;
            if (vw == null)
                return;
            ExamLv lv;
            if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
                lv = ExamLv.A;
            else
                lv = ExamLv.B;
            int[] vnesydif = new int[vtxtNEsyDif[lv].Length];
            int[] vndiff = new int[vnesydif.Length];
            for(int i = 0; i < vnesydif.Length; ++i)
            {
                vnesydif[i] = int.Parse(vtxtNEsyDif[lv][i].Text);
                if (!int.TryParse(vtxtNDiff[lv][i].Text, out vndiff[i]))
                    vndiff[i] = 0;
            }
            vw.GenQ(lv, vnesydif, vndiff);
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
                btnQGen.IsEnabled = rdoA.IsEnabled =
                    rdoB.IsEnabled = grdA.IsEnabled =
                    grdB.IsEnabled = false;
                return;
            }
            rdoA.IsEnabled = rdoB.IsEnabled =
                grdA.IsEnabled = grdB.IsEnabled = true;
            rdoA.IsChecked = true;
            ExamLv lv = ExamLv.A;
            List<int[]> nmod = vw.GetNMod(lv);
            if(nmod != null)
            {
                int i = -1;
                foreach (TextBox t in vtxtNEsyDif[lv])
                    t.Text = nmod[0][++i].ToString();
                i = -1;
                foreach (TextBox t in vtxtNDiff[lv])
                    t.Text = nmod[1][++i].ToString();
            }
            else
            {
                foreach (TextBox t in vtxtNEsyDif[lv])
                    t.Text = string.Empty;
                foreach (TextBox t in vtxtNDiff[lv])
                    t.Text = string.Empty;
            }
            lv = ExamLv.B;
            nmod = vw.GetNMod(lv);
            if (nmod != null)
            {
                int i = -1;
                foreach (TextBox t in vtxtNEsyDif[lv])
                    t.Text = nmod[0][++i].ToString();
                i = -1;
                foreach (TextBox t in vtxtNDiff[lv])
                    t.Text = nmod[1][++i].ToString();
            }
            else
            {
                foreach (TextBox t in vtxtNEsyDif[lv])
                    t.Text = string.Empty;
                foreach (TextBox t in vtxtNDiff[lv])
                    t.Text = string.Empty;
            }
        }
    }
}
