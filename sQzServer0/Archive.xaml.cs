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
        }

        private void lbxBrd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

        private void lbxSl_Selected(object sender, RoutedEventArgs e)
        {
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
            btnExportExaminees.IsEnabled = true;
            btnExportSheets.IsEnabled = true;
            //StringBuilder sb = new StringBuilder();
            //foreach(QuestPack p in Slot.QuestionPacks.Values)
            //{
            //    List<string> indicies = p.GetDuplicatedIdx_in_Passgae();
            //    foreach (string s in indicies)
            //        sb.Append(s + ", ");
            //}
            //WPopup.s.ShowDialog("Duplicated in passages: " + sb.ToString());
        }

        private void PrintQSheets()
        {
            if (!QSheet2Docx.ForceCreateDocx("exported_question_sheets.docx"))
            {
                MessageBox.Show("PrintQSheets CreateDocx error");
                return;
            }

            QSheet2Docx exporter = new QSheet2Docx();
            foreach(QuestPack pack in Slot.QuestionPacks.Values)
            {
                foreach(QuestSheet qs in pack.vSheet.Values)
                {
                    AnswerSheet ansSheet = null;
                    if (Slot.AnswerKeyPacks.ContainsKey(qs.TestType) &&
                        Slot.AnswerKeyPacks[qs.TestType].vSheet.ContainsKey(qs.ID))
                        ansSheet = Slot.AnswerKeyPacks[qs.TestType].vSheet[qs.ID];
                    if (ansSheet == null)
                    {
                        MessageBox.Show("answer sheet not found: " + qs.TestType +
                            " " + qs.ID);
                        return;
                    }
                    exporter.WriteQsheet(qs, ansSheet.tAns.ToCharArray());
                }
            }

            QSheet2Docx.CloseDocx();
        }

        private void PrintAllExaminees()
        {
            if(!QSheetExamineePrinter.ForceCreateDocx("exported_examinees.docx"))
            {
                MessageBox.Show("PrintAllExaminees CreateDocx error");
                return;
            }

            QSheetExamineePrinter printer = new QSheetExamineePrinter();

            foreach (ExamRoomS0 room in Slot.Rooms.Values)
                foreach(ExamineeS0 nee in room.Examinees.Values)
                {
                    nee.DBGetAns();
                    if (nee.AnswerSheet.BytesOfAnswer == null)
                        continue;
                    nee.DBSelGrade();
                    nee.DBGetQSId();
                    if (nee.TestType < 0)
                        return;
                    QuestSheet qs = null;
                    if (Slot.QuestionPacks.ContainsKey(nee.TestType) &&
                        Slot.QuestionPacks[nee.TestType].vSheet.ContainsKey(nee.AnswerSheet.QuestSheetID))
                    {
                        qs = Slot.QuestionPacks[nee.TestType].vSheet[nee.AnswerSheet.QuestSheetID];
                    }
                    if (qs == null)
                        return;
                    if (nee.AnswerSheet.BytesOfAnswer == null)
                    {
                        MessageBox.Show("DBGetAns AnswerSheet.BytesOfAnswer = null");
                        return;
                    }
                    AnswerSheet ansSheet = null;
                    if(Slot.AnswerKeyPacks.ContainsKey(qs.TestType) &&
                        Slot.AnswerKeyPacks[qs.TestType].vSheet.ContainsKey(qs.ID))
                        ansSheet = Slot.AnswerKeyPacks[qs.TestType].vSheet[qs.ID];
                    if(ansSheet == null)
                    {
                        MessageBox.Show("answer sheet not found: " + qs.TestType +
                            " " + qs.ID);
                        return;
                    }

                    printer.WriteThisExaminee(qs, nee, ansSheet.tAns.ToCharArray());
                    printer.WritePageBreak();
                }
            QSheetExamineePrinter.CloseDocx();
        }

        private void lbxNee_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            ExamineeS0 nee = new ExamineeS0();
            nee.ID = i.Content as string;
            TabItem tbi = new TabItem();
            tbi.Header = i.Content;
            //
            foreach (ExamRoomS0 r in Slot.Rooms.Values)
                if (r.Examinees.ContainsKey(nee.ID))
                    nee = r.Examinees[nee.ID];
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
            //using (QSheetExamineePrinter printer = new QSheetExamineePrinter())
            //{
            //    if(printer.CreateDocx("all_examinees.docx"))
            //    {
            //        printer.WriteThisExaminee(qs, nee);
            //        printer.Write2PageBreaks();
            //        printer.WriteThisExaminee(qs, nee);
            //    }
            //}
            svwr.Height = 560;
            spl.Children.Add(svwr);
            tbi.Content = spl;
            //
        }

        private void btnExportSheets_Click(object sender, RoutedEventArgs e)
        {
            PrintQSheets();
        }

        private void btnExportExaminees_Click(object sender, RoutedEventArgs e)
        {
            PrintAllExaminees();
        }
    }
}
