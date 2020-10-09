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
    public partial class Archieve : Page
    {
        UICbMsg mCbMsg;
        bool bRunning;
        ExamBoard mBrd;
        Dictionary<ExamLv, TextBox[]> vtxtNEsyDif;
        Dictionary<ExamLv, TextBox[]> vtxtNDiff;
        TabItem tbiSelected;

        public Archieve()
        {
            InitializeComponent();
            mCbMsg = new UICbMsg();

            mBrd = new ExamBoard();

            bRunning = true;

            tbiSelected = null;
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
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
            vtxtNDiff = new Dictionary<ExamLv, TextBox[]>();
            vtxtNDiff.Add(ExamLv.A, new TextBox[QuestSheet.GetIUs(ExamLv.A).Count()]);
            vtxtNDiff.Add(ExamLv.B, new TextBox[QuestSheet.GetIUs(ExamLv.B).Count()]);
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

            LoadTxt();
            InitQPanel();

            LoadBrd();

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            //if (bRunning && mCbMsg.ToUp())
            //    Dispatcher.Invoke(() => {
            //        lblStatus.Text += mCbMsg.txt; });
        }

        private void btnMMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("MainMenu.xaml", UriKind.Relative));
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            btnMMenu.Content = t._((int)TxI.BACK_MMENU);
            rdoA.Content = t._((int)TxI.BASIC);
            rdoB.Content = t._((int)TxI.ADVAN);
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
        }

        private void lbxBrd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbcSl.Items.Clear();
            ListBox l = sender as ListBox;
            ListBoxItem i = l.SelectedItem as ListBoxItem;
            if (i == null)
            {
                lbxSl.IsEnabled = lbxNee.IsEnabled = false;
                return;
            }
            DateTime dt;
            if (!DT.To_(i.Content as string, DT._, out dt))
            {
                mBrd.mDt = dt;
                lbxSl.IsEnabled = lbxNee.IsEnabled = true;
                LoadSl();
                LoadNee();
            }
        }

        private void LoadSl()
        {
            string emsg;
            List<DateTime> v = mBrd.DBSelSl(true, out emsg);
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

        private void LoadNee()
        {
            string emsg;
            List<string> v = mBrd.DBSelNee(out emsg);
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
            lbxNee.Items.Clear();
            foreach (string tid in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = tid;
                it.Selected += lbxNee_Selected;
                //dark = !dark;
                //if (dark)
                //    it.Background = new SolidColorBrush(c);
                lbxNee.Items.Add(it);
            }
        }

        void DisableQSGen()
        {
            foreach (TextBox[] vt in vtxtNEsyDif.Values)
                foreach(TextBox t in vt)
                    t.IsEnabled = false;
            foreach (TextBox[] vt in vtxtNDiff.Values)
                foreach (TextBox t in vt)
                    t.IsEnabled = false;
        }

        void EnableQSGen()
        {
            foreach (TextBox[] vt in vtxtNEsyDif.Values)
                foreach (TextBox t in vt)
                    t.IsEnabled = true;
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

        private void lbxNee_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            ExamineeS0 nee = new ExamineeS0();
            List<TabItem> l = new List<TabItem>();
            foreach (TabItem ti in tbcSl.Items)
                if (!nee.ParseLvId(ti.Header as string))
                    l.Add(ti);
            foreach (TabItem ti in l)
                tbcSl.Items.Remove(ti);
            if (nee.ParseLvId(i.Content as string))
                return;
            TabItem tbi = new TabItem();
            tbi.Header = i.Content;
            //
            nee.mDt = mBrd.mDt;
            int qsid = nee.DBGetQSId();
            if (qsid < 0)
                return;
            QuestSheet qs = null;
            foreach(ExamSlot sl in mBrd.vSl.Values)
            {
                if (sl.vQPack[nee.eLv].vSheet.ContainsKey(qsid))
                {
                    qs = sl.vQPack[nee.eLv].vSheet[qsid];
                    break;
                }
                else if (sl.vQPackAlt[nee.eLv].vSheet.ContainsKey(qsid))
                {
                    qs = sl.vQPackAlt[nee.eLv].vSheet[qsid];
                    break;
                }
            }
            if (qs == null)
            {
                string t = nee.DBGetT();
                if (!mBrd.vSl.ContainsKey(t))
                {
                    ExamSlot sl = new ExamSlot();
                    DateTime dati;
                    DT.To_(mBrd.mDt.ToString(DT._) + ' ' + t, DT.HS, out dati);
                    sl.Dt = dati;
                    string emsg;
                    if ((emsg = sl.DBSelRoomId()) != null)
                    {
                        WPopup.s.ShowDialog(emsg);
                        return;
                    }
                    sl.DBSelStt();
                    sl.DBSelQPkR();
                    sl.DBSelNee();
                    if (sl.DBSelArchieve(out emsg))
                    {
                        WPopup.s.ShowDialog(emsg);
                        return;
                    }
                    mBrd.vSl.Add(t.Substring(0, 5), sl);
                    //
                    if (sl.vQPack[nee.eLv].vSheet.ContainsKey(qsid))
                        qs = sl.vQPack[nee.eLv].vSheet[qsid];
                    else if (sl.vQPackAlt[nee.eLv].vSheet.ContainsKey(qsid))
                        qs = sl.vQPackAlt[nee.eLv].vSheet[qsid];
                }
            }
            if (qs == null)
                return;
            nee.DBSelGrade();
            StackPanel spl = new StackPanel();
            TextBlock tx = new TextBlock();
            tx.Text = Txt.s._((int)TxI.QS_ID) + ' ' + qs.tId + ", ";
            spl.Children.Add(tx);
            tx = new TextBlock();
            tx.Text = Txt.s._((int)TxI.MARK) + ' ' + nee.Grade;
            spl.Children.Add(tx);
            ScrollViewer svwr = new ScrollViewer();
            svwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            StackPanel sp = new StackPanel();
            int x = 0;
            SolidColorBrush evenbg = Theme.s._[(int)BrushId.BG];
            SolidColorBrush oddbg = Theme.s._[(int)BrushId.Q_BG];
            SolidColorBrush difbg = Theme.s._[(int)BrushId.Ans_TopLine];
            SolidColorBrush bg;
            bool even = false;
            char[] ans = nee.DBGetAns();
            int k = -1;
            foreach (Question q in qs.ShallowCopy())
            {
                if (q.bDiff)
                    bg = difbg;
                else if (even)
                    bg = evenbg;
                else
                    bg = oddbg;
                even = !even;
                TextBlock j = new TextBlock();
                j.Width = tbcSl.Width - SystemParameters.ScrollWidth;
                j.TextWrapping = TextWrapping.Wrap;
                j.Text = ++x + ". " + q.Stmt;
                j.Background = bg;
                sp.Children.Add(j);
                for (int idx = 0; idx < Question.N_ANS; ++idx)
                {
                    j = new TextBlock();
                    j.Width = tbcSl.Width - SystemParameters.ScrollWidth;
                    j.TextWrapping = TextWrapping.Wrap;
                    j.Text = ((char)('A' + idx)).ToString() + ") " + q.vAns[idx];
                    j.Background = bg;
                    if (q.vKeys[idx])
                        j.FontWeight = FontWeights.Bold;
                    if (ans[++k] == Question.C1)
                        j.Background = Theme.s._[(int)BrushId.Ans_Highlight];
                    sp.Children.Add(j);
                }
            }
            svwr.Content = sp;
            svwr.Height = 560;
            spl.Children.Add(svwr);
            tbi.Content = spl;
            //
            tbcSl.Items.Add(tbi);
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
                        t.MaxLength = 2;
                    t = vtxtNDiff[lv][i];
                    if (t != null)
                        t.MaxLength = 2;
                }
            }
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
                rdoA.IsEnabled =
                    rdoB.IsEnabled = grdA.IsEnabled =
                    grdB.IsEnabled = false;
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
