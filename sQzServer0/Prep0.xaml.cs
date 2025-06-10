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

        private void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("MainMenu.xaml", UriKind.Relative));
        }

<<<<<<< HEAD
        private void InsertSlot(object sender, RoutedEventArgs e)
=======
        private void InsertBoard(object sender, RoutedEventArgs e)
>>>>>>> master
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
                ExamSlotS0 slot = new ExamSlotS0();
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

<<<<<<< HEAD
        private void LoadSlotView()
=======
        private void InsertSlot(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            string t = tbxSl.Text;
            if (DT.To_(t, DT.h, out dt))
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.SLOT_NOK]);
                spMain.Opacity = 1;
            }
            else
            {
                string msg;
                if(0 < mBrd.DBInsSl(dt, out msg))
                {
                    spMain.Opacity = 0.5;
                    WPopup.s.ShowDialog(Txt.s._[(int)TxI.SLOT_OK]);
                    spMain.Opacity = 1;
                    LoadSl();
                    tbxSl.Text = string.Empty;
                }
                else
                {
                    spMain.Opacity = 0.5;
                    WPopup.s.ShowDialog(msg);
                    spMain.Opacity = 1;
                }
            }
        }

        private void LoadBrd()
>>>>>>> master
        {
            string emsg;
            List<DateTime> v = ExamSlotS0.DBSelectSlotIDs(false, out emsg);
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

<<<<<<< HEAD
        private void btnFileQ_Click(object sender, RoutedEventArgs e)
=======
        void InitLbxQCatgry()
        {
            List<string> qCatName = new List<string>();
            for (int i = (int)TxI.IU01; i <= (int)TxI.IU15; ++i)
                qCatName.Add(Txt.s._[i]);
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Brush b = new SolidColorBrush(c);
            Dispatcher.Invoke(() => {
                lbxQCatgry.Items.Clear();
                foreach (string i in qCatName)
                {
                    ListBoxItem it = new ListBoxItem();
                    it.Content = i;
                    dark = !dark;
                    if (dark)
                        it.Background = b;
                    lbxQCatgry.Items.Add(i);
                }
            });
        }

        private void SelectBoard(object sender, SelectionChangedEventArgs e)
        {
            tbcNee.Items.Clear();
            ListBox l = sender as ListBox;
            ListBoxItem i = l.SelectedItem as ListBoxItem;
            if (i == null)
            {
                lbxSl.IsEnabled = false;
                return;
            }
            DateTime dt;
            if(!DT.To_(i.Content as string, DT._, out dt))
            {
                mBrd.mDt = dt;
                lbxSl.IsEnabled = true;
                LoadSl();
            }
        }

        private void OpenRawFile_of_MCItems(object sender, RoutedEventArgs e)
>>>>>>> master
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
            gDBQuest.Children.Clear();
            gDBQuest.RowDefinitions.Clear();
            vChk.Clear();
            SingleQuestionView.IdxWidth = FontSize * 2;
            SingleQuestionView.StemWidth = gDBQuest.ColumnDefinitions.First().Width.Value - SingleQuestionView.IdxWidth;
            AnswerSheet ansSheet = new AnswerSheet();
            mDBQS.ExtractKey(ansSheet);
            int rowIdx = -1;
            //foreach (Question q in mDBQS.ShallowCopyIndependentQuestions())
            //    AddSingleQuestionToDBView(q, ++rowIdx, rowIdx, ansSheet.BytesOfAnswer);

            int questionIdx = rowIdx;
            foreach (QSheetSection section in mDBQS.Sections)
            {
<<<<<<< HEAD
                AddPassageTextToDBView(section.Requirements, ++rowIdx, SingleQuestionView.StemWidth);
                foreach (Question q in section.Questions)
                    AddSingleQuestionToDBView(q, ++questionIdx, ++rowIdx, ansSheet.BytesOfAnswer);
=======
                TextBlock i = new TextBlock();
                i.Text = (++x + 1) + ". " + q.Stmt;
                i.Width = w;
                i.TextWrapping = TextWrapping.Wrap;
                StackPanel sp = new StackPanel();
                sp.Children.Add(i);
                for (int idx = 0; idx < Question.N_ANS; ++idx)
                {
                    TextBlock j = new TextBlock();
                    j.Text = ((char)('A' + idx)).ToString() + ") " + q.vAns[idx];
                    j.Width = w;
                    j.TextWrapping = TextWrapping.Wrap;
                    if (q.vKeys[idx])
                        j.FontWeight = FontWeights.Bold;
                    sp.Children.Add(j);
                }
                /*if (q.bDiff)
                    bg = difbg;
                else*/ if (even)
                    bg = evenbg;
                else
                    bg = oddbg;
                even = !even;
                sp.Background = bg;
                RowDefinition rd = new RowDefinition();
                gDBQuest.RowDefinitions.Add(rd);
                Grid.SetRow(sp, x);
                gDBQuest.Children.Add(sp);
                CheckBox chk = new CheckBox();
                chk.Name = "c" + q.uId;
                chk.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(chk, 1);
                Grid.SetRow(chk, x);
                gDBQuest.Children.Add(chk);
                vChk.Add(chk);
>>>>>>> master
            }
            tbiDBQ.Header = CreateQuestSheetHeader(mDBQS);
        }

        private void AddPassageTextToDBView(string text, int rowIdx, double w)
        {
            TextBlock passageText = new TextBlock();
            passageText.Text = "\n\n" + text + "\n\n";
            passageText.Width = w;
            passageText.TextWrapping = TextWrapping.Wrap;
            passageText.TextAlignment = TextAlignment.Justify;
            RowDefinition rd = new RowDefinition();
            gDBQuest.RowDefinitions.Add(rd);
            Grid.SetRow(passageText, rowIdx);
            gDBQuest.Children.Add(passageText);
        }

        private void AddSingleQuestionToDBView(Question q, int qIdx, int rowIdx, byte[] optionStatusArray)
        {
            SingleQuestionView questionView = new SingleQuestionView(q, qIdx, optionStatusArray, false);
            RowDefinition rd = new RowDefinition();
            gDBQuest.RowDefinitions.Add(rd);
            Grid.SetRow(questionView, rowIdx);
            gDBQuest.Children.Add(questionView);
            CheckBox chk = new CheckBox();
            chk.Name = "c" + q.uId;
            chk.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(chk, 1);
            Grid.SetRow(chk, rowIdx);
            gDBQuest.Children.Add(chk);
            vChk.Add(chk);
        }

        private void ShowTmpQ()
        {
<<<<<<< HEAD
            AnswerSheet ansSheet = new AnswerSheet();
            mTmpQS.ExtractKey(ansSheet);
            svwrTmpQ.Content = new QuestionSheetView(mTmpQS, ansSheet.BytesOfAnswer, FontSize * 2,
                svwrTmpQ.Width - FontSize * 2 - SystemParameters.ScrollWidth, false);
            tbiTmpQ.Header = CreateQuestSheetHeader(mTmpQS);
        }

        private string CreateQuestSheetHeader(QuestSheet qs)
        {
=======
            SolidColorBrush evenbg = Theme.s._[(int)BrushId.BG];
            SolidColorBrush oddbg = Theme.s._[(int)BrushId.Q_BG];
            SolidColorBrush difbg = Theme.s._[(int)BrushId.Ans_TopLine];
            SolidColorBrush bg;
            bool even = false;
            int x = -1;
            double w = svwrTmpQ.Width;
            svwrTmpQ.Content = null;
            StackPanel sp = new StackPanel();
            foreach (Question q in mTmpQS.ShallowCopy())
            {
                TextBlock i = new TextBlock();
                i.Text = (++x + 1) + ". " + q.Stmt;
                i.Width = w;
                i.TextWrapping = TextWrapping.Wrap;
                /*if (q.bDiff)
                    bg = difbg;
                else*/ if (even)
                    bg = evenbg;
                else
                    bg = oddbg;
                even = !even;
                i.Background = bg;
                sp.Children.Add(i);
                for (int idx = 0; idx < Question.N_ANS; ++idx)
                {
                    TextBlock j = new TextBlock();
                    j.Text = ((char)('A' + idx)).ToString() + ") " + q.vAns[idx];
                    j.Width = w;
                    j.TextWrapping = TextWrapping.Wrap;
                    if (q.vKeys[idx])
                        j.FontWeight = FontWeights.Bold;
                    j.Background = bg;
                    sp.Children.Add(j);
                }
            }
            svwrTmpQ.Content = sp;
>>>>>>> master
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            foreach (QSheetSection section in qs.Sections)
                sb.AppendFormat(section.Questions.Count + ", ");
            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);
            sb.Append(")");
            return sb.ToString();
        }

        private void InsertMCItems(object sender, RoutedEventArgs e)
        {
            if (mTmpQS.CountAllQuestions() == 0)
            {
                MessageBox.Show("No question to import!");
                return;
            }
            mTmpQS.TestType = GetTestType_FromTextBox();
            if (mTmpQS.TestType < 0)
                return;
            gDBQuest.Children.Clear();
            svwrTmpQ.Content = null;
            mTmpQS.DBInsertOriginQuestions();
            mTmpQS.Clear();
            tbiTmpQ.Header = CreateQuestSheetHeader(mTmpQS);
            LoadAndShowQuestionFromDB();
        }

        private void LoadAndShowQuestionFromDB()
        {
            int testTypeID = GetTestType_FromTextBox();
            if (testTypeID < 0)
                return;
            mDBQS.DBSelectNondeletedQuestions(testTypeID);
            ShowDBQ();
        }

<<<<<<< HEAD
        private int GetTestType_FromTextBox()
=======
        private void SelectIUx(object sender, SelectionChangedEventArgs e)
>>>>>>> master
        {
            int testTypeID;
            if (!int.TryParse(tbxTestType.Text, out testTypeID))
            {
                MessageBox.Show("Test type is not number!");
                return -1;
            }
            return testTypeID;
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
            txtBirpl.Text = "Test type";// t._((int)TxI.BIRPL);
            txtRoom.Text = t._((int)TxI.ROOM);
            btnImp.Content = t._((int)TxI.PREP_IMP);
            btnDelQ.Content = t._((int)TxI.PREP_DEL_SEL);
            btnImpQ.Content = t._((int)TxI.PREP_IMP);
            tbiTmpQ.Header = CreateQuestSheetHeader(mTmpQS);
        }

        private void DeleteSelectedMCItems(object sender, RoutedEventArgs e)
        {
            StringBuilder qids = new StringBuilder();
            qids.Append("id IN (");
            foreach(CheckBox c in vChk)
                if(c.IsChecked == true)
                {
                    int uqid;
                    if (int.TryParse(c.Name.Substring(1), out uqid))
                        qids.Append(uqid.ToString() + ",");
                }
            bool toUpdate = false;
            if ("id IN (".Length < qids.Length)
            {
                qids.Remove(qids.Length - 1, 1);//remove the last " OR "
                qids.Append(")");
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
            ExamSlotS0 sl = new ExamSlotS0();
            DateTime dt;
            DT.To_(i.Content as string, out dt);
            sl.Dt = dt;
            sl.DBSelectRoomInfo();
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
            
        }

        private void btnShowQuestions_Click(object sender, RoutedEventArgs e)
        {
            LoadAndShowQuestionFromDB();
        }
    }
}
