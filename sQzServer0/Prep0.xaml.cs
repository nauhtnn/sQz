using Microsoft.Win32;
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
using MySql.Data.MySqlClient;
using sQzLib;

namespace sQzServer0
{
    /// <summary>
    /// Interaction logic for Prep0.xaml
    /// </summary>
    public partial class Prep0 : Page
    {
        List<CheckBox> vChk;
        QuestSheet mDBQS;
        QuestSheet mTmpQS;

        public Prep0()
        {
            InitializeComponent();
            mDBQS = new QuestSheet();
            mTmpQS = new QuestSheet();
            vChk = new List<CheckBox>();
        }

        private void btnMMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("MainMenu.xaml", UriKind.Relative));
        }

        private void InsertSlot(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            if (DT.To_(newSlot.Text, DT._, out dt))
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._((int)TxI.BOARD_NOK));
                spMain.Opacity = 1;
            }
            else
            {
                ExamSlot slot = new ExamSlot();
                slot.mDt = dt;
                string msg;
                if(0 < slot.InsertSlot(out msg))
                {
                    spMain.Opacity = 0.5;
                    WPopup.s.ShowDialog(Txt.s._((int)TxI.BOARD_OK));
                    spMain.Opacity = 1;
                    LoadSlotView();
                    newSlot.Text = string.Empty;
                }
                else
                {
                    spMain.Opacity = 0.5;
                    WPopup.s.ShowDialog(msg);
                    spMain.Opacity = 1;
                }
            }
        }

        private void LoadSlotView()
        {
            string emsg;
            List<DateTime> v = ExamSlot.DBSelectSlotIDs(false, out emsg);
            if(v == null)
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(emsg);
                spMain.Opacity = 1;
                return;
            }
            SlotsView.Items.Clear();
            foreach (DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DT._);
                it.Selected += SlotsView_Selected;
                it.Unselected += SlotsView_Unselected;
                SlotsView.Items.Add(it);
            }
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        { }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.FontSize = 16;

            LoadTxt();

            LoadSlotView();
            Window w = Window.GetWindow(this);
            if(w != null)
                w.Closing += W_Closing;
        }

        private void btnFileQ_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            // set filter for file extension and default file extension 
            dlg.DefaultExt = ".docx";
            dlg.Filter = "text documents (.docx;.txt)|*.docx;*.txt";
            bool? result = dlg.ShowDialog();

            string fpath = null;
            if (result == true)
                fpath = dlg.FileName;
            else
                return;
            mTmpQS.LoadFromFile(fpath);
            ShowTmpQ();
        }

        private void ShowDBQ()
        {
            int x = -1;
            gDBQuest.Children.Clear();
            gDBQuest.RowDefinitions.Clear();
            vChk.Clear();
            SingleQuestionView.staticMargin = new Thickness(FontSize);
            SingleQuestionView.IdxWidth = FontSize * 2;
            SingleQuestionView.StemWidth = gDBQuest.ColumnDefinitions.First().Width.Value - SingleQuestionView.IdxWidth;
            foreach (Question q in mDBQS.ShallowCopyIndependentQuestions())
                AddSingleQuestionToDBView(q, ++x);

            foreach (PassageWithQuestions p in mDBQS.ShallowCopyPassages())
            {
                AddPassageTextToDBView(p.Passage, ++x, SingleQuestionView.StemWidth);
                foreach (Question q in p.Questions)
                    AddSingleQuestionToDBView(q, ++x);
            }
            
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._((int)TxI.Q_DB), mDBQS.Count, mDBQS.CountPassage);
            tbiDBQ.Header = sb.ToString();
        }

        private void AddPassageTextToDBView(string text, int x, double w)
        {
            TextBlock passageText = new TextBlock();
            passageText.Text = "\n\n" + text + "\n\n";
            passageText.Width = w;
            passageText.TextWrapping = TextWrapping.Wrap;
            passageText.TextAlignment = TextAlignment.Justify;
            RowDefinition rd = new RowDefinition();
            gDBQuest.RowDefinitions.Add(rd);
            Grid.SetRow(passageText, x);
            gDBQuest.Children.Add(passageText);
        }

        static readonly bool[] noSeletectedOptionArray = new bool[1024];

        private void AddSingleQuestionToDBView(Question q, int x)
        {
            SingleQuestionView questionView = new SingleQuestionView(q, x, noSeletectedOptionArray);
            RowDefinition rd = new RowDefinition();
            gDBQuest.RowDefinitions.Add(rd);
            Grid.SetRow(questionView, x);
            gDBQuest.Children.Add(questionView);
            CheckBox chk = new CheckBox();
            chk.Name = "c" + q.uId;
            chk.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(chk, 1);
            Grid.SetRow(chk, x);
            gDBQuest.Children.Add(chk);
            vChk.Add(chk);
        }

        private void ShowTmpQ()
        {
            StackPanel sp = new StackPanel();
            AddListOfSingleQuestionsToPanel(mTmpQS.ShallowCopyIndependentQuestions(), 0, sp);
            AddListOfPassageQuestionsToPanel(mTmpQS.ShallowCopyPassages(), 0, sp);
            svwrTmpQ.Content = sp;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._((int)TxI.Q_TMP), mTmpQS.Count, mTmpQS.CountPassage);
            tbiTmpQ.Header = sb.ToString();
        }

        private void AddListOfSingleQuestionsToPanel(List<Question> questions, int idx, StackPanel panel)
        {
            SingleQuestionView.IdxWidth = FontSize * 2;
            SingleQuestionView.StemWidth = svwrTmpQ.Width - 30 - SingleQuestionView.IdxWidth;
            foreach (Question q in questions)
                panel.Children.Add(new SingleQuestionView(q, idx++, noSeletectedOptionArray));
        }

        private void AddListOfPassageQuestionsToPanel(List<PassageWithQuestions> passages, int index, StackPanel panel)
        {
            SolidColorBrush evenbg = Theme.s._[(int)BrushId.BG];
            SolidColorBrush oddbg = Theme.s._[(int)BrushId.Q_BG];
            SolidColorBrush difbg = Theme.s._[(int)BrushId.Ans_TopLine];
            SolidColorBrush bg;
            bool even = false;
            int idx = index;
            double w = svwrTmpQ.Width - 30;
            foreach (PassageWithQuestions p in passages)
            {
                TextBlock passageText = new TextBlock();
                passageText.Text = "\n\n" + p.Passage + "\n\n";
                passageText.Width = w;
                passageText.TextWrapping = TextWrapping.Wrap;
                passageText.TextAlignment = TextAlignment.Justify;
                panel.Children.Add(passageText);
                AddListOfSingleQuestionsToPanel(p.Questions, ++idx, panel);
            }
        }

        private void btnImpQ_Click(object sender, RoutedEventArgs e)
        {
            if (mTmpQS.Count == 0)
                return;
            gDBQuest.Children.Clear();
            svwrTmpQ.Content = null;
            mTmpQS.DBIns();
            mTmpQS.Clear();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._((int)TxI.Q_TMP), mTmpQS.Count, mTmpQS.CountPassage);
            tbiTmpQ.Header = sb.ToString();
            LoadAndShowQuestionFromDB();
        }

        private void LoadAndShowQuestionFromDB()
        {
            mDBQS.DBSelectNondeletedQuestions();
            ShowDBQ();
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            btnMMenu.Content = t._((int)TxI.BACK_MMENU);
            txtDt.Text = DT._;
            tbi1.Header = t._((int)TxI.PREP_NEE);
            tbi2.Header = t._((int)TxI.PREP_Q);
            txtId.Text = t._((int)TxI.NEEID_S);
            txtName.Text = t._((int)TxI.NEE_NAME);
            txtBirdate.Text = t._((int)TxI.BIRDATE);
            txtBirpl.Text = t._((int)TxI.BIRPL);
            txtRoom.Text = t._((int)TxI.ROOM);
            btnImp.Content = t._((int)TxI.PREP_IMP);
            btnDelQ.Content = t._((int)TxI.PREP_DEL_SEL);
            btnImpQ.Content = t._((int)TxI.PREP_IMP);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._((int)TxI.Q_TMP), mTmpQS.Count, mTmpQS.CountPassage);
            tbiTmpQ.Header = sb.ToString();
        }

        private void btnDelQ_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder qids = new StringBuilder();
            foreach(CheckBox c in vChk)
                if(c.IsChecked == true)
                {
                    int uqid;
                    if (int.TryParse(c.Name.Substring(1), out uqid))
                        qids.Append("id=" + uqid + " OR ");
                }
            bool toUpdate = false;
            if (0 < qids.Length)
            {
                qids.Remove(qids.Length - 4, 4);//remove the last " OR "
                Question.DBDelete(qids.ToString());
                toUpdate = true;
            }
            if (toUpdate)
                LoadAndShowQuestionFromDB();
            chkAll.IsChecked = false;
        }

        private void SlotsView_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            ExamSlot sl = new ExamSlot();
            DateTime dt;
            DT.To_(i.Content as string, out dt);
            sl.Dt = dt;
            sl.DBSelRoomId();
            sl.DBSelStt();
            sl.DBSelNee();
            PrepNeeView pnv = new PrepNeeView(sl);
            pnv.DeepCopy(refSl);
            pnv.Show(true);
            tbcNee.Items.Add(pnv);
            pnv.Focus();
        }

        private void SlotsView_Unselected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            foreach(TabItem ti in tbcNee.Items)
                if(ti.Name == DT.CreateNameFromDateTime(i.Content as string))
                {
                    tbcNee.Items.Remove(ti);
                    break;
                }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox c in vChk)
                c.IsChecked = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox c in vChk)
                c.IsChecked = false;
        }

        private void tbiDBQ_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAndShowQuestionFromDB();
        }
    }
}
