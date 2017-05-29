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
        ExamSlot mSl;

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
            DateTime dt;
            if (DtFmt.ToDt(tbxBrd.Text, DtFmt._, out dt))
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
            if (DtFmt.ToDt(t, DtFmt.h, out dt))
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
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            lbxBrd.Items.Clear();
            foreach(DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DtFmt.__);
                dark = !dark;
                if (dark)
                    it.Background = new SolidColorBrush(c);
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
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            lbxSl.Items.Clear();
            foreach (DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DtFmt.hh);
                it.Selected += lbxSl_Selected;
                it.Unselected += lbxSl_Unselected;
                dark = !dark;
                if (dark)
                    it.Background = new SolidColorBrush(c);
                lbxSl.Items.Add(it);
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
            if(!DtFmt.ToDt(i.Content as string, DtFmt._, out dt))
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
            //btnNeeBrowse.Content = t._[(int)TxI.NEE_ADD];
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

        private void lbxSl_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            if (mBrd.vSl.ContainsKey(i.Content as string))
                return;
            mSl = new ExamSlot();
            DtFmt.ToDt(mBrd.mDt.ToString(DtFmt._) + ' ' + i.Content as string, DtFmt.H, out mSl.mDt);
            mSl.DBSelNee();
            mBrd.vSl.Add(i.Content as string, mSl);
            PrepNeeView pnv = new PrepNeeView();
            pnv.mSl = mSl;
            pnv.ShallowCopy(refSl);
            pnv.Show(true);
            TabItem ti = new TabItem();
            ti.Name = "_" + (i.Content as string).Replace(':', '_');
            ti.Header = pnv.mSl.mDt.ToString(DtFmt.hh);
            ti.Content = pnv;
            tbcNee.Items.Add(ti);
        }

        private void lbxSl_Unselected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            mBrd.vSl.Remove(i.Content as string);
            foreach(TabItem ti in tbcNee.Items)
                if(ti.Name == "_" + (i.Content as string).Replace(':', '_'))
                {
                    tbcNee.Items.Remove(ti);
                    break;
                }
        }
    }
}
