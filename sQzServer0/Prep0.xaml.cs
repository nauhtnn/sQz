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
            if (DT.To_(newSlot.Text, DT.SYSTEM_DT_FMT, out dt))
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
            List<DateTime> v = ExamSlot.DBSelectSlots(false, out emsg);
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
                it.Content = dt.ToString(DT.SYSTEM_DT_FMT);
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
            SolidColorBrush evenbg = Theme.s._[(int)BrushId.BG];
            SolidColorBrush oddbg = Theme.s._[(int)BrushId.Q_BG];
            SolidColorBrush difbg = Theme.s._[(int)BrushId.Ans_TopLine];
            SolidColorBrush bg;
            bool even = false;
            int x = -1;
            gDBQuest.Children.Clear();
            gDBQuest.RowDefinitions.Clear();
            vChk.Clear();
            double w = gDBQuest.ColumnDefinitions.First().Width.Value;
            foreach (Question q in mDBQS.ShallowCopy())
            {
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
                if (q.bDiff)
                    bg = difbg;
                else if (even)
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
            }
            //StringBuilder sb = new StringBuilder();
            //sb.AppendFormat(Txt.s._((int)TxI.Q_DB), mDBQS.Count, QuestSheet.DBGetND(mSelQCat));
            tbiDBQ.Header = "CSDL";//sb.ToString();
        }

        private void ShowTmpQ()
        {
            StackPanel sp = new StackPanel();
            //svwrTmpQ.Content = null;
            AddListOfSingleQuestionToPanel(mTmpQS.ShallowCopy(), 0, sp);
            svwrTmpQ.Content = sp;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._((int)TxI.Q_TMP), mTmpQS.Count, mTmpQS.CountD);
            tbiTmpQ.Header = sb.ToString();
        }

        private void AddListOfSingleQuestionToPanel(List<Question> questions, int index, StackPanel panel)
        {
            SolidColorBrush evenbg = Theme.s._[(int)BrushId.BG];
            SolidColorBrush oddbg = Theme.s._[(int)BrushId.Q_BG];
            SolidColorBrush difbg = Theme.s._[(int)BrushId.Ans_TopLine];
            SolidColorBrush bg;
            bool even = false;
            int idx = index;
            double w = svwrTmpQ.Width;
            foreach (Question q in questions)
            {
                if (q.bDiff)
                    bg = difbg;
                else if (even)
                    bg = evenbg;
                else
                    bg = oddbg;
                even = !even;
                AddSingleQuestionToPanel(q, ++idx, w, bg, panel);
            }
        }

        private void AddSingleQuestionToPanel(Question question, int index, double width, SolidColorBrush background, StackPanel panel)
        {
            TextBlock i = new TextBlock();
            i.Text = index + ". " + question.Stmt;
            i.Width = width;
            i.TextWrapping = TextWrapping.Wrap;
            
            i.Background = background;
            panel.Children.Add(i);
            for (int idx = 0; idx < Question.N_ANS; ++idx)
            {
                TextBlock j = new TextBlock();
                j.Text = ((char)('A' + idx)).ToString() + ") " + question.vAns[idx];
                j.Width = width;
                j.TextWrapping = TextWrapping.Wrap;
                if (question.vKeys[idx])
                    j.FontWeight = FontWeights.Bold;
                j.Background = background;
                panel.Children.Add(j);
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
            sb.AppendFormat(Txt.s._((int)TxI.Q_TMP), 0, mTmpQS.CountD);
            tbiTmpQ.Header = sb.ToString();
            mDBQS.DBSelect();
            ShowDBQ();
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            btnMMenu.Content = t._((int)TxI.BACK_MMENU);
            txtDt.Text = DT.SYSTEM_DT_FMT;// t._((int)TxI.DATE_L);
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
            tbiDBQ.Header = "tbiDBQ.Header";
            //sb.Clear();
            //sb.AppendFormat(Txt.s._((int)TxI.Q_TMP), 0, mTmpQS.CountD);
            tbiTmpQ.Header = "tbiTmpQ.Header";
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
            {
                mDBQS.DBSelect();
                ShowDBQ();
            }
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
                if(ti.Name == "_" + (i.Content as string).Replace(':', '_'))
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
    }
}
