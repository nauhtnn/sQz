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
        List<int> vQId;
        IUx mSelQCat;
        QuestSheet mDBQSh;
        QuestSheet mQSh;
        ExamBoard mBrd;

        public Prep0()
        {
            InitializeComponent();
            mSelQCat = IUx._0;
            mDBQSh = new QuestSheet();
            mQSh = new QuestSheet();
            mBrd = new ExamBoard();
        }

        private void btnMMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("MainMenu.xaml", UriKind.Relative));
        }

        private void btnInsBrd_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            if (DT.To_(tbxBrd.Text, DT._, out dt))
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.BOARD_NOK]);
                spMain.Opacity = 1;
            }
            else
            {
                ExamBoard eb = new ExamBoard();
                eb.mDt = dt;
                string msg;
                if(0 < eb.DBIns(out msg))
                {
                    spMain.Opacity = 0.5;
                    WPopup.s.ShowDialog(Txt.s._[(int)TxI.BOARD_OK]);
                    spMain.Opacity = 1;
                    LoadBrd();
                    tbxBrd.Text = string.Empty;
                }
                else
                {
                    spMain.Opacity = 0.5;
                    WPopup.s.ShowDialog(msg);
                    spMain.Opacity = 1;
                }
            }
        }

        private void btnInsSl_Click(object sender, RoutedEventArgs e)
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
        {
            string emsg;
            List<DateTime> v = ExamBoard.DBSel(out emsg);
            if(v == null)
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(emsg);
                spMain.Opacity = 1;
                return;
            }
            lbxBrd.Items.Clear();
            foreach(DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DT.__);
                lbxBrd.Items.Add(it);
            }
        }

        private void LoadSl()
        {
            string emsg;
            List<DateTime> v = mBrd.DBSelSl(out emsg);
            if(v == null)
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

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        { }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.FontSize = 16;
            //double wd = SystemParameters.PrimaryScreenWidth;
            //Application.Current.MainWindow.Width = wd;
            //double rt = spMain.RenderSize.Width / 1280; //d:DesignWidth
            //double rt = w / 1280;
            //ScaleTransform st = new ScaleTransform(rt, rt);
            //spMain.RenderTransform = st;

            LoadTxt();

            InitLbxQCatgry();
            LoadBrd();
            Window w = Window.GetWindow(this);
            if(w != null)
                w.Closing += W_Closing;
        }

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

        private void lbxBrd_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void btnFileQ_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            // set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "text documents (.txt)|*.txt";
            bool? result = dlg.ShowDialog();

            // get the selected file name and display in a textbox
            string filePath = null;
            if (result == true)
                filePath = dlg.FileName;
            mQSh.ReadTxt(Utils.ReadFile(filePath));
            ShowQuest(false);
        }

        private void ShowQuest(bool db) //same as Operation0.xaml
        {
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            SolidColorBrush evenbg = new SolidColorBrush(Colors.Wheat);
            SolidColorBrush bg = evenbg;
            bool even = false;
            Dispatcher.Invoke(() => {
                int x = 0;
                Grid g = db ? gDBQuest : gQuest;
                g.Children.Clear();
                g.RowDefinitions.Clear();
                QuestSheet qs = db ? mDBQSh : mQSh;
                vChk = new List<CheckBox>();
                vQId = new List<int>();
                double w = g.ColumnDefinitions.First().Width.Value;
                foreach (Question q in qs.ShallowCopy())
                {
                    TextBlock i = new TextBlock();
                    i.Text = ++x + ". " + q.Stmt;
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
                    if (even)
                        bg = evenbg;
                    else
                        bg = null;
                    even = !even;
                    sp.Background = bg;
                    RowDefinition rd = new RowDefinition();
                    g.RowDefinitions.Add(rd);
                    Grid.SetRow(sp, x);
                    g.Children.Add(sp);
                    CheckBox chk = new CheckBox();
                    chk.Name = "c" + q.uId;
                    chk.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(chk, 1);
                    Grid.SetRow(chk, x);
                    g.Children.Add(chk);
                    vQId.Add(q.uId);
                    vChk.Add(chk);
                }
            });
        }

        private void btnImpDBQ_Click(object sender, RoutedEventArgs e)
        {
            if (mSelQCat != IUx._0 && 0 < mQSh.Count)
            {
                gDBQuest.Children.Clear();
                gQuest.Children.Clear();
                mQSh.DBIns(mSelQCat);
                mQSh.Clear();
                mDBQSh.DBSelect(mSelQCat);
                ShowQuest(true);
            }
        }

        private void lbxQCatgry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            if (Enum.IsDefined(typeof(IUx), l.SelectedIndex))
            {
                mSelQCat = (IUx)l.SelectedIndex;
                mDBQSh.DBSelect(mSelQCat);
                ShowQuest(true);
            }
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            txtId.Text = t._[(int)TxI.NEEID_S];
            txtName.Text = t._[(int)TxI.NEE_NAME];
            txtBirdate.Text = t._[(int)TxI.BIRDATE];
            txtBirpl.Text = t._[(int)TxI.BIRPL];
            txtRoom.Text = t._[(int)TxI.ROOM];
            txtNeeDB.Text = t._[(int)TxI.NEE_LS_DB];
            txtNeeTmp.Text = t._[(int)TxI.NEE_LS_TMP];
            txtDt.Text = t._[(int)TxI.DATE_L];
            txtHm.Text = t._[(int)TxI.TIME_L];
            txtIU.Text = t._[(int)TxI.IUS];
            tbi1.Header = t._[(int)TxI.PREP_DT_T];
            tbi2.Header = t._[(int)TxI.PREP_Q];
            btnMMenu.Content = t._[(int)TxI.BACK_MMENU];
            btnFileQ.Content = "+";// t._[(int)TxI.Q_ADD];
            btnDelQ.Content = t._[(int)TxI.DEL];
            btnImpDBQ.Content = t._[(int)TxI.PREP_IMP];
            btnAllQ.Content = t._[(int)TxI.SEL_ALL];
            btnDBDelNee.Content = t._[(int)TxI.PREP_DEL];
            btnImpDBNee.Content = t._[(int)TxI.PREP_IMP];
            btnFileNee.Content = "+";// t._[(int)TxI.PREP_LD_FL];
            btnTmpDelNee.Content = t._[(int)TxI.PREP_DEL];
        }

        private void btnAllQ_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox c in vChk)
                c.IsChecked = true;
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
                Question.DBDelete(mSelQCat, qids.ToString());
                toUpdate = true;
            }
            if (toUpdate)
            {
                mDBQSh.DBSelect(mSelQCat);
                ShowQuest(true);
            }
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                NavigationService.Navigate(new Uri("ExamHistory.xaml", UriKind.Relative));
            });
        }

        private void lbxSl_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            ExamSlot sl = new ExamSlot();
            DateTime dt;
            DT.To_(mBrd.mDt.ToString(DT._) + ' ' + i.Content as string, DT.H, out dt);
            sl.Dt = dt;
            sl.DBSelRoomId();
            sl.DBSelNee();
            PrepNeeView pnv = new PrepNeeView(sl);
            pnv.DeepCopy(refSl);
            pnv.Show(true);
            tbcNee.Items.Add(pnv);
        }

        private void lbxSl_Unselected(object sender, RoutedEventArgs e)
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
    }
}
