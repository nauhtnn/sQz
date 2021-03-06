﻿using System;
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
        Dictionary<ExamLv, TextBlock[]> vtxtN;
        Dictionary<ExamLv, TextBox[]> vtxtNDiff;
        Dictionary<ExamLv, TextBlock[]> vtxtND;
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
            Window w = Application.Current.MainWindow;
            w.Closing += W_Closing;
            w.FontSize = 16;

            vtxtNEsyDif = new Dictionary<ExamLv, TextBox[]>();
            vtxtNEsyDif.Add(ExamLv.A, new TextBox[QuestSheet.GetIUs(ExamLv.A).Count()]);
            vtxtNEsyDif.Add(ExamLv.B, new TextBox[QuestSheet.GetIUs(ExamLv.B).Count()]);
            vtxtN = new Dictionary<ExamLv, TextBlock[]>();
            vtxtN.Add(ExamLv.A, new TextBlock[QuestSheet.GetIUs(ExamLv.A).Count()]);
            vtxtN.Add(ExamLv.B, new TextBlock[QuestSheet.GetIUs(ExamLv.B).Count()]);
            vtxtNDiff = new Dictionary<ExamLv, TextBox[]>();
            vtxtNDiff.Add(ExamLv.A, new TextBox[QuestSheet.GetIUs(ExamLv.A).Count()]);
            vtxtNDiff.Add(ExamLv.B, new TextBox[QuestSheet.GetIUs(ExamLv.B).Count()]);
            vtxtND = new Dictionary<ExamLv, TextBlock[]>();
            vtxtND.Add(ExamLv.A, new TextBlock[QuestSheet.GetIUs(ExamLv.A).Count()]);
            vtxtND.Add(ExamLv.B, new TextBlock[QuestSheet.GetIUs(ExamLv.B).Count()]);
            int i = -1, j = -1;
            foreach (TextBox tbx in grdA.Children.OfType<TextBox>())
            {
                if (Grid.GetColumn(tbx) == 1)
                {
                    vtxtNEsyDif[ExamLv.A][++i] = tbx;
                    tbx.Name = "n" + i;
                }
                else
                {
                    vtxtNDiff[ExamLv.A][++j] = tbx;
                    tbx.Name = "d" + j;
                }
            }
            i = j = -1;
            foreach (TextBox tbx in grdB.Children.OfType<TextBox>())
            {
                if (Grid.GetColumn(tbx) == 1)
                {
                    vtxtNEsyDif[ExamLv.B][++i] = tbx;
                    tbx.Name = "n" + i;
                }
                else
                {
                    vtxtNDiff[ExamLv.B][++j] = tbx;
                    tbx.Name = "d" + j;
                }
            }
            List<int[]> vn = QuestSheet.DBGetNMod(ExamLv.A);
            i = j = -1;
            foreach (TextBlock txt in grdA.Children.OfType<TextBlock>())
            {
                if (Grid.GetColumn(txt) == 2)
                {
                    vtxtN[ExamLv.A][++i] = txt;
                    txt.Name = "g" + i;
                    txt.Text = "/ " + vn[0][i];
                }
                else if (Grid.GetColumn(txt) == 4)
                {
                    vtxtND[ExamLv.A][++j] = txt;
                    txt.Name = "h" + j;
                    txt.Text = "/ " + vn[1][j];
                }
            }
            vn = QuestSheet.DBGetNMod(ExamLv.B);
            i = j = -1;
            foreach (TextBlock txt in grdB.Children.OfType<TextBlock>())
            {
                if (Grid.GetColumn(txt) == 2)
                {
                    vtxtN[ExamLv.B][++i] = txt;
                    txt.Name = "g" + i;
                    txt.Text = "/ " + vn[0][i];
                }
                else if (Grid.GetColumn(txt) == 4)
                {
                    vtxtND[ExamLv.B][++j] = txt;
                    txt.Name = "h" + j;
                    txt.Text = "/ " + vn[1][j];
                }
            }

            LoadTxt();
            InitQPanel();

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
            int x;
            switch (c)
            {
                case NetCode.Srvr1Auth:
                    outMsg = BitConverter.GetBytes((int)TxI.OP_AUTH_NOK);
                    if (buf.Length - offs < 12)
                        break;
                    x = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    string pw = Encoding.ASCII.GetString(buf, offs, 8);
                    offs += 8;
                    foreach (ExamSlot sl in mBrd.vSl.Values)
                    {
                        if(sl.vRoom.ContainsKey(x))
                        {
                            if(pw == sl.vRoom[x].tPw)
                            {
                                outMsg = BitConverter.GetBytes((int)TxI.OP_AUTH_OK);
                                break;
                            }
                        }
                    }
                    break;
                case NetCode.Srvr1DatRetriving:
                    if (buf.Length - offs < 4)
                    {
                        outMsg = null;
                        break;
                    }
                    x = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    outMsg = mBrd.ToByteSl1(x);
                    Dispatcher.InvokeAsync(() =>
                    {
                        foreach (Op0SlotView vw in tbcSl.Items.OfType<Op0SlotView>())
                            vw.UpRT1(x);
                    });
                    return true;
                case NetCode.QuestRetrieving:
                    if(buf.Length - offs < 4)
                    {
                        outMsg = null;
                        break;
                    }
                    x = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    outMsg = mBrd.ToByteQPack(x);
                    return true;
                case NetCode.AnsKeyRetrieving:
                    outMsg = mBrd.ToByteKey();
                    break;
                case NetCode.SrvrSubmitting:
                    int rid;
                    if (-1 < (rid = mBrd.ReadByteSl0(buf, ref offs)))
                    {
                        string emsg;
                        if (mBrd.DBUpdateRs(rid, out emsg))
                            mCbMsg += emsg;
                        else if (emsg == null)
                        {
                            mCbMsg += Txt.s._[(int)TxI.SRVR_DB_OK];
                            Dispatcher.InvokeAsync(() =>
                            {
                                foreach (Op0SlotView vw in tbcSl.Items.OfType<Op0SlotView>())
                                    vw.UpdateRsView(rid);
                            });
                        }
                        mBrd.DBUpStt();
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

            btnStart.Content = t._[(int)TxI.STRT_SRVR];
            btnStop.Content = t._[(int)TxI.STOP_SRVR];
            btnMMenu.Content = t._[(int)TxI.BACK_MMENU];
            btnQGen.Content = t._[(int)TxI.QS_GEN];
            btnQSav.Content = t._[(int)TxI.OP_Q_SAV];

            txtDate.Text = t._[(int)TxI.DATE_L];
            txtTime.Text = t._[(int)TxI.TIME_L];

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
            txtRoom.Text = t._[(int)TxI.ROOM];
            txtT1.Text = t._[(int)TxI.T1];
            txtT2.Text = t._[(int)TxI.T2];
            txtComp.Text = t._[(int)TxI.COMP];

            txtRId.Text = t._[(int)TxI.ROOM];
            txtRN.Text = t._[(int)TxI.OP_N_NEE];
            txtRT1.Text = t._[(int)TxI.T1];
            txtRT2.Text = t._[(int)TxI.T2];
            txtRQPack.Text = t._[(int)TxI.OP_Q];
            txtQPackR0.Text = t._[(int)TxI.OP_PRI];
            txtQPackR1.Text = t._[(int)TxI.OP_ALT];
            txtPw.Text = t._[(int)TxI.OP_PW];
        }

        private void lbxBrd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbcSl.Items.Clear();
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
            List<DateTime> v = mBrd.DBSelSl(false, out emsg);
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

        void DisableQSGen()
        {
            foreach (TextBox[] vt in vtxtNEsyDif.Values)
                foreach(TextBox t in vt)
                {
                    t.IsEnabled = false;
                    t.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                    t.Background = Theme.s._[(int)BrushId.BG_Gray];
                }
            foreach (TextBox[] vt in vtxtNDiff.Values)
                foreach (TextBox t in vt)
                {
                    t.IsEnabled = false;
                    t.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                    t.Background = Theme.s._[(int)BrushId.BG_Gray];
                }
            btnQGen.IsEnabled = false;
            btnQGen.Foreground = Theme.s._[(int)BrushId.FG_Gray];
            btnQGen.Background = Theme.s._[(int)BrushId.BG_Gray];
        }

        void EnableQSGen()
        {
            foreach (TextBox[] vt in vtxtNEsyDif.Values)
                foreach (TextBox t in vt)
                {
                    t.IsEnabled = true;
                    t.Background = Theme.s._[(int)BrushId.FG];
                    t.Foreground = Theme.s._[(int)BrushId.mBlack];
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
            DateTime dt;
            DT.To_(mBrd.mDt.ToString(DT._) + ' ' + i.Content as string, DT.H, out dt);
            sl.Dt = dt;
            string emsg;
            if ((emsg = sl.DBSelRoomId()) != null)
            {
                WPopup.s.ShowDialog(emsg);
                return;
            }
            sl.DBSelStt();
            sl.DBSelQPkR();
            sl.DBSelNee();
            if(sl.DBSelArchieve(out emsg))
            {
                WPopup.s.ShowDialog(emsg);
                return;
            }
            Op0SlotView tbi = new Op0SlotView(sl);
            tbi.DeepCopy(tbcRefSl);
            tbi.ShowExaminee();
            tbi.ShowQSHeader();
            tbcSl.Items.Add(tbi);
            QuestSheet.DBUpdateCurQSId(mBrd.mDt);
            mBrd.vSl.Add(i.Content as string, sl);
            if ((tbi = tbcSl.SelectedItem as Op0SlotView) != null &&
                    tbi.mSl.eStt == ExamStt.Prep)
                EnableQSGen();
            else
                DisableQSGen();
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
                txtNqs.Text = sl.CountQSByRoom(ExamLv.B).ToString();
            }
            tbxIU_TextChanged(null, null);
        }

        private void tbxIU_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            bool bG = true;
            ExamLv lv;
            if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
                lv = ExamLv.A;
            else
                lv = ExamLv.B;
            if(t != null && 0 < t.Text.Length)
            {
                int idx = int.Parse(t.Name.Substring(1));
                TextBlock tb = vtxtN[lv][idx];
                if (tb.Text.Length < 3)
                    t.Text = string.Empty;
                else
                {
                    int i = int.Parse(t.Text);
                    int m = int.Parse(tb.Text.Substring(2));
                    if (m < i)
                        t.Text = string.Empty;
                }
            }
            int n = 0;
            for(int j = 0; j < vtxtNEsyDif[lv].Length; ++j)
                if ((t = vtxtNEsyDif[lv][j]) != null)
                {
                    if (0 < t.Text.Length)
                    {
                        n += int.Parse(t.Text);
                        vtxtNDiff[lv][j].IsEnabled = true;
                        vtxtNDiff[lv][j].Background = Theme.s._[(int)BrushId.FG];
                        vtxtNDiff[lv][j].Foreground = Theme.s._[(int)BrushId.mBlack];
                    }
                    else
                    {
                        bG = false;
                        vtxtNDiff[lv][j].IsEnabled = false;
                        vtxtNDiff[lv][j].Background = Theme.s._[(int)BrushId.BG_Gray];
                        vtxtNDiff[lv][j].Foreground = Theme.s._[(int)BrushId.FG_Gray];
                    }
                }
                else
                    bG = false;
            tbxNq.Text = n.ToString();
            if (bG && n == 30)
            {
                btnQGen.IsEnabled = true;
                btnQGen.Foreground = Theme.s._[(int)BrushId.FG];
                btnQGen.Background = Theme.s._[(int)BrushId.mBackup];
            }
            else
            {
                btnQGen.IsEnabled = false;
                btnQGen.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                btnQGen.Background = Theme.s._[(int)BrushId.BG_Gray];
            }
        }

        private void tbxIUdif_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t.Text.Length == 0)
                return;
            ExamLv lv;
            if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
                lv = ExamLv.A;
            else
                lv = ExamLv.B;
            int i = int.Parse(t.Text);
            int idx = int.Parse(t.Name.Substring(1));
            TextBox tm = vtxtNEsyDif[lv][idx];
            if(tm.Text.Length == 0)
            {
                t.Text = string.Empty;
                return;
            }
            int m = int.Parse(tm.Text);
            if(m < i)
                t.Text = string.Empty;
            TextBlock tb = vtxtND[lv][idx];
            if (tb.Text.Length < 3)
            {
                t.Text = string.Empty;
                return;
            }
            m = int.Parse(tb.Text.Substring(2));
            if (m < i)
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
                btnQGen.Background = Theme.s._[(int)BrushId.BG_Gray];
                btnQGen.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                return;
            }
            vw.InitNMod();
            rdoA.IsEnabled = rdoB.IsEnabled =
                grdA.IsEnabled = grdB.IsEnabled = true;
            rdoA.IsChecked = true;
            ExamLv lv = ExamLv.A;
            List<int[]> nmod = vw.GetNMod(lv);
            if(nmod != null && nmod.Count == 2)
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
            if (nmod != null && nmod.Count == 4)
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
            if ((vw = tbcSl.SelectedItem as Op0SlotView) != null &&
                    vw.mSl.eStt == ExamStt.Prep)
                EnableQSGen();
            else
                DisableQSGen();
        }
    }
}
