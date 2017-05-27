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
        List<uint> vQId;
        IUx mSelQCat;
        QuestSheet mDBQSh;
        QuestSheet mQSh;
        ExamBoard mBrd;

        public Prep0()
        {
            ShowsNavigationUI = false;
            InitializeComponent();
            mSelQCat = IUx._0;
            mDBQSh = new QuestSheet();
            mQSh = new QuestSheet();
            mBrd = new ExamBoard();
        }

        private void btnInsBrd_Click(object sender, RoutedEventArgs e)
        {
            if (DtFmt.ToDt(tbxBrd.Text, DtFmt._, out mBrd.mDt))
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.BIRDATE_NOK]);
                spMain.Opacity = 1;
            }
            else
            {
                mBrd.DBIns();
                LoadBrd();
            }
        }

        private void btnInsSl_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            string t = tbxSl.Text;
            if (DtFmt.ToDt(t, DtFmt.h, out dt))
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.BIRDATE_NOK]);
                spMain.Opacity = 1;
            }
            else
            {
                mBrd.DBInsSl(dt);
                LoadBrd();
            }
        }

        private void LoadBrd()
        {
            List<DateTime> v = ExamBoard.DBSel();
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            lbxBrd.Items.Clear();
            foreach(DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DtFmt._);
                dark = !dark;
                if (dark)
                    it.Background = new SolidColorBrush(c);
                lbxBrd.Items.Add(it);
            }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.FontSize = 16;
            //double rt = spMain.RenderSize.Width / 1280; //d:DesignWidth
            //ScaleTransform st = new ScaleTransform(rt, rt);
            //spMain.RenderTransform = st;

            LoadTxt();

            InitLbxQCatgry();
            LoadBrd();
        }

        void InitLbxQCatgry()
        {
            List<string> qCatName = new List<string>();
            for (int i = (int)TxI._1; i <= (int)TxI._15; ++i)
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

        private void LoadExaminees(bool fresh) //same as Operation0.xaml
        {
            //bool dark = true;
            //Color c = new Color();
            //c.A = 0xff;
            //c.B = c.G = c.R = 0xf0;
            //Dispatcher.Invoke(() => {
            //    ListBox l = null;
            //    if (fresh)
            //        l = lbxNewStu;
            //    else
            //        l = lbxStudent;
            //    l.Items.Clear();
            //    foreach(ExamRoom r in mSl.vRoom.Values)
            //        foreach (ExamineeA e in r.vExaminee.Values)
            //        {
            //            ListBoxItem i = new ListBoxItem();
            //            i.Content = e.ToString();
            //            dark = !dark;
            //            if (dark)
            //                i.Background = new SolidColorBrush(c);
            //            l.Items.Add(i);
            //        }
            //});
        }

        private void btnNeeBrowse_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog dlg = new OpenFileDialog();

            //// set filter for file extension and default file extension 
            //dlg.DefaultExt = ".txt";
            //dlg.Filter = "text documents (*.txt)|*.txt";
            //bool? result = dlg.ShowDialog();

            //string filePath = null;
            //if (result == true)
            //    filePath = dlg.FileName;
            //mSl.ReadF(filePath);
            
            //LoadExaminees(true);
        }

        private void btnInsNee_Click(object sender, RoutedEventArgs e)
        {
            //if (mSl.uId != uint.MaxValue)
            //{
            //    lbxNewStu.Items.Clear();
            //    mSl.DBInsertNee();
            //    LoadExaminees(false);
            //}
        }

        private void lbxBrd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = sender as ListBox;
            ListBoxItem i = l.SelectedItem as ListBoxItem;
            if (i == null)
                return;
            if(DtFmt.ToDt(i.Content as string, DtFmt._, out mBrd.mDt))
            {
                lbxSl.Items.Clear();
                foreach(DateTime dt in mBrd.DBSelSl())
                {
                    ListBoxItem it = new ListBoxItem();
                    it.Content = dt.ToString(DtFmt.h);
                    lbxSl.Items.Add(it);
                }
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
                vQId = new List<uint>();
                foreach (Question q in qs.vQuest)
                {
                    TextBlock i = new TextBlock();
					Grid.SetRow(i, ++x);
                    i.Text = x + ") " + q.ToString();
                    dark = !dark;
                    if (dark)
                        i.Background = new SolidColorBrush(c);
                    g.Children.Add(i);
                    CheckBox chk = new CheckBox();
                    chk.Name = "c" + x;
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
            if (mSelQCat != IUx._0 && 0 < mQSh.vQuest.Count)
            {
                gDBQuest.Children.Clear();
                gQuest.Children.Clear();
                mQSh.DBInsert(mSelQCat);
                mQSh.vQuest.Clear();
                mDBQSh.DBSelect(mSelQCat);
                ShowQuest(true);
            }
        }

        private void lbxQCatgry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            if (Enum.IsDefined(typeof(IUx), l.SelectedIndex + 1))
            {
                mSelQCat = (IUx)l.SelectedIndex + 1;
                mDBQSh.DBSelect(mSelQCat);
                ShowQuest(true);
            }
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            btnNeeBrowse.Content = t._[(int)TxI.NEE_ADD];
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
            bool toUpdate = false;
            foreach(CheckBox c in vChk)
                if(c.IsChecked == true)
                {
                    uint qId;
                    if (uint.TryParse(c.Name.Substring(1), out qId))
                    {
                        Question.DBDelete(mSelQCat, vQId[(int)qId]);
                        toUpdate = true;
                    }
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
    }
}
