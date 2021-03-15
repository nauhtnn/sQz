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
        ExamSlotS0 Slot;
        TabItem tbiSelected;

        public Archieve()
        {
            InitializeComponent();
            mCbMsg = new UICbMsg();

            Slot = new ExamSlotS0();

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
            List<DateTime> v = ExamSlotS0.DBSelectSlotIDs(true, out emsg);
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
                lbxNee.IsEnabled = false;
                return;
            }
            DateTime dt;
            if (!DT.To_(i.Content as string, DT._, out dt))
            {
                Slot.mDt = dt;
                lbxNee.IsEnabled = true;
                lbxSl_Selected(null, null);
                LoadNee();
            }
        }

        //private void LoadSl()
        //{
        //    string emsg;
        //    List<DateTime> v = mBrd.DBSelSl(true, out emsg);
        //    if (v == null)
        //    {
        //        spMain.Opacity = 0.5;
        //        WPopup.s.ShowDialog(emsg);
        //        spMain.Opacity = 1;
        //    }
        //    //bool dark = true;
        //    //Color c = new Color();
        //    //c.A = 0xff;
        //    //c.B = c.G = c.R = 0xf0;
        //    lbxSl.Items.Clear();
        //    foreach (DateTime dt in v)
        //    {
        //        ListBoxItem it = new ListBoxItem();
        //        it.Content = dt.ToString(DT._);
        //        it.Selected += lbxSl_Selected;
        //        it.Unselected += lbxSl_Unselected;
        //        //dark = !dark;
        //        //if (dark)
        //        //    it.Background = new SolidColorBrush(c);
        //        lbxSl.Items.Add(it);
        //    }
        //}

        private void LoadNee()
        {
            Slot.DBSelNee();
            //bool dark = true;
            //Color c = new Color();
            //c.A = 0xff;
            //c.B = c.G = c.R = 0xf0;
            lbxNee.Items.Clear();
            List<string> neeIDs = new List<string>();
            foreach (ExamRoomS0 r in Slot.Rooms.Values)
                foreach (ExamineeA nee in r.Examinees.Values)
                    neeIDs.Add(nee.ID);
            foreach (string tid in neeIDs)
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
            //foreach (TextBox[] vt in vtxtNEsyDif.Values)
            //    foreach(TextBox t in vt)
            //        t.IsEnabled = false;
            //foreach (TextBox[] vt in vtxtNDiff.Values)
            //    foreach (TextBox t in vt)
            //        t.IsEnabled = false;
        }

        void EnableQSGen()
        {
            //foreach (TextBox[] vt in vtxtNEsyDif.Values)
            //    foreach (TextBox t in vt)
            //        t.IsEnabled = true;
        }

        private void lbxSl_Selected(object sender, RoutedEventArgs e)
        {
            //ListBoxItem i = sender as ListBoxItem;
            //if (i == null)
            //    return;

            //Slot = new ExamSlotS0();
            //DateTime dt;
            //DT.To_(i.Content as string, out dt);
            //Slot.Dt = dt;
            string emsg;
            if ((emsg = Slot.DBSelectRoomInfo()) != null)
            {
                WPopup.s.ShowDialog(emsg);
                return;
            }
            Slot.DBSelStt();
            Slot.DBSelNee();
            if (Slot.DBSelectArchiveQPacks_and_AnsPack(out emsg))
            {
                WPopup.s.ShowDialog(emsg);
                return;
            }
            Op0SlotView tbi = new Op0SlotView(Slot);
            tbi.DeepCopy(tbcRefSl);
            tbi.ShowExaminee();
            tbi.ShowQSHeader();
            tbcSl.Items.Add(tbi);
            //QuestSheet.DBUpdateCurQSId(mBrd.mDt);
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
            foreach (TabItem ti in tbcSl.Items)
                if (ti.Name == DT.CreateNameFromDateTime(i.Content as string))
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
                l.Add(ti);
            foreach (TabItem ti in l)
                tbcSl.Items.Remove(ti);
            nee.ID = i.Content as string;
            TabItem tbi = new TabItem();
            tbi.Header = i.Content;
            //
            nee.mDt = Slot.mDt;
            nee.DBGetQSId();
            if (nee.TestType < 0)
                return;
            QuestSheet qs = null;
            if(Slot.QuestionPacks.ContainsKey(nee.TestType) &&
                Slot.QuestionPacks[nee.TestType].vSheet.ContainsKey(nee.AnswerSheet.QuestSheetID))
            {
                qs = Slot.QuestionPacks[nee.TestType].vSheet[nee.AnswerSheet.QuestSheetID];
            }
            //if (qs == null)
            //{
            //    string t = nee.DBGetT();
            //    if (!mBrd.vSl.ContainsKey(t))
            //    {
            //        ExamSlot sl = new ExamSlot();
            //        DateTime dati;
            //        DT.To_(mBrd.mDt.ToString(DT._) + ' ' + t, DT.HS, out dati);
            //        sl.Dt = dati;
            //        string emsg;
            //        if ((emsg = sl.DBSelRoomId()) != null)
            //        {
            //            WPopup.s.ShowDialog(emsg);
            //            return;
            //        }
            //        sl.DBSelStt();
            //        sl.DBSelQPkR();
            //        sl.DBSelNee();
            //        if (sl.DBSelArchieve(out emsg))
            //        {
            //            WPopup.s.ShowDialog(emsg);
            //            return;
            //        }
            //        mBrd.vSl.Add(t.Substring(0, 5), sl);
            //        //
            //        if (sl.QuestionPack[nee.eLv].vSheet.ContainsKey(qsid))
            //            qs = sl.QuestionPack[nee.eLv].vSheet[qsid];
            //        else if (sl.vQPackAlt[nee.eLv].vSheet.ContainsKey(qsid))
            //            qs = sl.vQPackAlt[nee.eLv].vSheet[qsid];
            //    }
            //}
            if (qs == null)
                return;
            nee.DBSelGrade();
            StackPanel spl = new StackPanel();
            TextBlock tx = new TextBlock();
            tx.Text = Txt.s._((int)TxI.QS_ID) + ' ' + qs.GetGlobalID_withTestType() + ", ";
            spl.Children.Add(tx);
            tx = new TextBlock();
            tx.Text = "Test type: " + nee.TestType + ", " + Txt.s._((int)TxI.MARK) + ' ' + nee.Grade;
            spl.Children.Add(tx);
            ScrollViewer svwr = new ScrollViewer();
            svwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //StackPanel sp = new StackPanel();
            //int x = 0;
            //SolidColorBrush evenbg = Theme.s._[(int)BrushId.BG];
            //SolidColorBrush oddbg = Theme.s._[(int)BrushId.Q_BG];
            //SolidColorBrush difbg = Theme.s._[(int)BrushId.Ans_TopLine];
            //SolidColorBrush bg;
            //bool even = false;
            nee.DBGetAns();
            if(nee.AnswerSheet.BytesOfAnswer == null)
            {
                MessageBox.Show("DBGetAns AnswerSheet.BytesOfAnswer = null");
                return;
            }
            //int k = -1;

            //foreach (Question q in qs.)
            //{
            //    if (even)
            //        bg = evenbg;
            //    else
            //        bg = oddbg;
            //    even = !even;
            //    TextBlock j = new TextBlock();
            //    j.Width = tbcSl.Width - SystemParameters.ScrollWidth;
            //    j.TextWrapping = TextWrapping.Wrap;
            //    j.Text = ++x + ". " + q.Stem;
            //    j.Background = bg;
            //    sp.Children.Add(j);
            //    for (int idx = 0; idx < Question.NUMBER_OF_OPTIONS; ++idx)
            //    {
            //        j = new TextBlock();
            //        j.Width = tbcSl.Width - SystemParameters.ScrollWidth;
            //        j.TextWrapping = TextWrapping.Wrap;
            //        j.Text = ((char)('A' + idx)).ToString() + ") " + q.vAns[idx];
            //        j.Background = bg;
            //        if (q.vKeys[idx])
            //            j.FontWeight = FontWeights.Bold;
            //        if (ans[++k] == Question.C1)
            //            j.Background = Theme.s._[(int)BrushId.Ans_Highlight];
            //        sp.Children.Add(j);
            //    }
            //}
            svwr.Content = new QuestionSheetView(qs, nee.AnswerSheet.BytesOfAnswer, FontSize * 2, 820, false);
            svwr.Height = 560;
            spl.Children.Add(svwr);
            tbi.Content = spl;
            //
            tbcSl.Items.Add(tbi);
        }

        public void InitQPanel()
        {
            //ExamLv[] l = new ExamLv[2];
            //l[0] = ExamLv.A;
            //l[1] = ExamLv.B;
            //foreach(ExamLv lv in l)
            //{
            //    int n = QuestSheet.GetIUs(lv).Count();
            //    for (int i = 0; i < n; ++i)
            //    {
            //        TextBox t = vtxtNEsyDif[lv][i];
            //        if (t != null)
            //            t.MaxLength = 2;
            //        t = vtxtNDiff[lv][i];
            //        if (t != null)
            //            t.MaxLength = 2;
            //    }
            //}
        }

        private void Lv_Checked(object sender, RoutedEventArgs e)
        {
            //ExamSlot sl = (tbcSl.SelectedItem as Op0SlotView).mSl;
            //if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
            //{
            //    grdB.Visibility = Visibility.Collapsed;
            //    grdA.Visibility = Visibility.Visible;
            //    txtNqs.Text = sl.CountQSByRoom(ExamLv.A).ToString();
            //}
            //else
            //{
            //    grdA.Visibility = Visibility.Collapsed;
            //    grdB.Visibility = Visibility.Visible;
            //    txtNqs.Text = sl.CountQSByRoom(ExamLv.B).ToString();
            //}
        }

        private void tbcSl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TabControl tbc = sender as TabControl;
            //if (tbc == null)
            //    return;
            //TabItem tbi = tbc.SelectedItem as TabItem;
            //if (tbi == null || tbiSelected == tbi)
            //    return;
            //tbiSelected = tbi;
            //Op0SlotView vw = tbi as Op0SlotView;
            //if (vw == null)
            //{
            //    rdoA.IsEnabled =
            //        rdoB.IsEnabled = grdA.IsEnabled =
            //        grdB.IsEnabled = false;
            //    return;
            //}
            ////vw.InitNMod();
            //rdoA.IsEnabled = rdoB.IsEnabled =
            //    grdA.IsEnabled = grdB.IsEnabled = true;
            //rdoA.IsChecked = true;
            //ExamLv lv = ExamLv.A;
            //List<int[]> nmod = vw.GetNMod(lv);
            //if(nmod != null && nmod.Count == 2)
            //{
            //    int i = -1;
            //    foreach (TextBox t in vtxtNEsyDif[lv])
            //        t.Text = nmod[0][++i].ToString();
            //    i = -1;
            //    foreach (TextBox t in vtxtNDiff[lv])
            //        t.Text = nmod[1][++i].ToString();
            //}
            //else
            //{
            //    foreach (TextBox t in vtxtNEsyDif[lv])
            //        t.Text = string.Empty;
            //    foreach (TextBox t in vtxtNDiff[lv])
            //        t.Text = string.Empty;
            //}
            //lv = ExamLv.B;
            //nmod = vw.GetNMod(lv);
            //if (nmod != null && nmod.Count == 4)
            //{
            //    int i = -1;
            //    foreach (TextBox t in vtxtNEsyDif[lv])
            //        t.Text = nmod[0][++i].ToString();
            //    i = -1;
            //    foreach (TextBox t in vtxtNDiff[lv])
            //        t.Text = nmod[1][++i].ToString();
            //}
            //else
            //{
            //    foreach (TextBox t in vtxtNEsyDif[lv])
            //        t.Text = string.Empty;
            //    foreach (TextBox t in vtxtNDiff[lv])
            //        t.Text = string.Empty;
            //}
            //if ((vw = tbcSl.SelectedItem as Op0SlotView) != null &&
            //        vw.mSl.eStt == ExamStt.Prep)
            //    EnableQSGen();
            //else
            //    DisableQSGen();
        }
    }
}
