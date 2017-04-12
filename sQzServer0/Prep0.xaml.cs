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
        IUxx mSelQCat;
        QuestSheet mDBQSh;
        QuestSheet mQSh;

        public Prep0()
        {
            ShowsNavigationUI = false;
            InitializeComponent();
            mSelQCat = IUxx.IU00;
            mDBQSh = new QuestSheet();
            mQSh = new QuestSheet();
        }

        private void btnInsDate_Click(object sender, RoutedEventArgs e)
        {
            if (Date.Chk224(tbxDate.Text))
            {
                Date.DBInsert(tbxDate.Text);
                LoadDate();
            }
        }

        private void LoadDate()
        {
            Date.DBSelect();
            if(0 < Date.svDate.Count)
            {
                bool dark = true;
                Color c = new Color();
                c.A = 0xff;
                c.B = c.G = c.R = 0xf0;
                Dispatcher.Invoke(() => {
                    lbxDate.Items.Clear();
                    foreach (string s in Date.svDate)
                    {
                        ListBoxItem i = new ListBoxItem();
                        i.Content = s;
                        dark = !dark;
                        if (dark)
                            i.Background = new SolidColorBrush(c);
                        lbxDate.Items.Add(i);
                    }
                });
            }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.FontSize = 16;
            double rt = spMain.RenderSize.Width / 1280; //d:DesignWidth
            ScaleTransform st = new ScaleTransform(rt, rt);
            spMain.RenderTransform = st;

            LoadTxt();

            InitLbxQCatgry();
            LoadDate();
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

        private void LoadExaminees(bool fresh) //same as Operation0.xaml
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
                ListBox l = null;
                if (fresh)
                    l = lbxNewStu;
                else
                    l = lbxStudent;
                l.Items.Clear();
                foreach (Examinee s in Examinee.svExaminee)
                {
                    ListBoxItem i = new ListBoxItem();
                    i.Content = s.ToString();
                    dark = !dark;
                    if (dark)
                        i.Background = new SolidColorBrush(c);
                    l.Items.Add(i);
                }
            });
        }

        private void btnNeeBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            // set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "text documents (*.txt)|*.txt";
            bool? result = dlg.ShowDialog();

            string filePath = null;
            if (result == true)
                filePath = dlg.FileName;
            Examinee.ReadTxt(Utils.ReadFile(filePath));
            LoadExaminees(true);
        }

        private void btnInsNee_Click(object sender, RoutedEventArgs e)
        {
            if (Date.sDBIdx != uint.MaxValue)
            {
                lbxNewStu.Items.Clear();
                Examinee.DBInsert(Date.sDBIdx);
                LoadExaminees(false);
            }
        }

        private void lbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            ListBoxItem i = (ListBoxItem)l.SelectedItem;
            if (i == null)
                return;
            Date.Select((string)i.Content);
            Examinee.DBSelect(Date.sDBIdx);
            LoadExaminees(false);
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
                    vQId.Add(q.mId);
                    vChk.Add(chk);
                }
            });
        }

        private void btnInsQuest_Click(object sender, RoutedEventArgs e)
        {
            if (mSelQCat != IUxx.IU00 && 0 < mQSh.vQuest.Count)
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
            if (Enum.IsDefined(typeof(IUxx), l.SelectedIndex + 1))
            {
                mSelQCat = (IUxx)l.SelectedIndex + 1;
                mDBQSh.DBSelect(mSelQCat);
                ShowQuest(true);
            }
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            btnInsDate.Content = t._[(int)TxI.DATE_ADD];
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
    }
}
