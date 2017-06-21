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
            //double rt = spMain.RenderSize.Width / 1280; //d:DesignWidth
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

        private void btnQBrowse_Click(object sender, RoutedEventArgs e)
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
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
                int x = -1;
                Grid g = db ? gDBQuest : gQuest;
                g.Children.Clear();
				g.RowDefinitions.Clear();
                QuestSheet qs = db ? mDBQSh : mQSh;
                vChk = new List<CheckBox>();
                vQId = new List<int>();
                foreach (Question q in qs.ShallowCopy())
                {
                    TextBlock i = new TextBlock();
					Grid.SetRow(i, ++x);
                    i.Text = x + ") " + q.ToString();
                    dark = !dark;
                    if (dark)
                        i.Background = new SolidColorBrush(c);
                    g.Children.Add(i);
                    CheckBox chk = new CheckBox();
                    chk.Name = "c" + q.uId;
					Grid.SetColumn(chk, 1);
					Grid.SetRow(chk, x);
					RowDefinition rd = new RowDefinition();
					g.RowDefinitions.Add(rd);
                    g.Children.Add(chk);
                    vQId.Add(q.uId);
                    vChk.Add(chk);
                }
            });
        }

        private void btnInsQuest_Click(object sender, RoutedEventArgs e)
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
            //btnNeeBrowse.Content = t._[(int)TxI.NEE_ADD];
            txtNeeDB.Text = t._[(int)TxI.NEE_LS_DB];
            txtNeeTmp.Text = t._[(int)TxI.NEE_LS_TMP];
            btnQBrowse.Content = t._[(int)TxI.Q_ADD];
            btnDel.Content = t._[(int)TxI.DEL];
            btnSelAll.Content = t._[(int)TxI.SEL_ALL];
        }

        private void btnSelAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox c in vChk)
                c.IsChecked = true;
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
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
            PrepNeeView pnv = new PrepNeeView();
            pnv.mSlDB = new ExamSlot();
            DateTime dt;
            DT.To_(mBrd.mDt.ToString(DT._) + ' ' + i.Content as string, DT.H, out dt);
            pnv.mSlDB.Dt = dt;
            pnv.mSlDB.DBSelRoomId();
            pnv.mSlDB.DBSelNee();
            pnv.mSlFile = new ExamSlot();
            pnv.mSlFile.Dt = pnv.mSlDB.Dt;
            pnv.mSlFile.DBSelRoomId();
            pnv.ShallowCopy(refSl);
            pnv.Show(true);
            TabItem ti = new TabItem();
            ti.Name = "_" + (i.Content as string).Replace(':', '_');
            ti.Header = pnv.mSlDB.Dt.ToString(DT.hh);
            ti.Content = pnv;
            tbcNee.Items.Add(ti);
            ti.Focus();
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
