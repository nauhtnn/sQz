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
        private Txt mTxt;

        public Prep0()
        {
            ShowsNavigationUI = false;
            InitializeComponent();
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
            //names corresponding to IUxx
            //string[] qCatName = { "Concept of ICT", "Computer", "Word", "Spreadsheet", "Presentation",
            //    "Internet", "Adv Word", "Adv Spreadsheet", "Database Mgmt", "Img processing",
            //    "Project Mgmt", "Img Editting"};
            List<string> qCatName = new List<string>();
            for (int i = (int)TxI.IU01; i <= (int)TxI.IU15; ++i)
                qCatName.Add(mTxt._[i]);
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

            // get the selected file name and display in a textbox
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
            Question.ReadTxt(Utils.ReadFile(filePath));
            LoadQuest(false);
        }

        private void LoadQuest(bool db) //same as Operation0.xaml
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
                int x = 0;
                StackPanel sp = db ? gQuest : gwQuest;
                sp.Children.Clear();
                foreach (Question q in Question.svvQuest[0])
                {
                    TextBlock i = new TextBlock();
                    i.Text = ++x + ") " + q.ToString();
                    dark = !dark;
                    if (dark)
                        i.Background = new SolidColorBrush(c);
                    sp.Children.Add(i);
                }
            });
        }

        private void btnInsQuest_Click(object sender, RoutedEventArgs e)
        {
            if (0 < gwQuest.Children.Count)
            {
                gQuest.Children.Clear();
                gwQuest.Children.Clear();
                Question.DBInsert();
                LoadQuest(true);
            }
        }

        private void lbxQCatgry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            if (Enum.IsDefined(typeof(IUxx), l.SelectedIndex + 1))
            {
                Question.svQuest.Clear();
                Question.sIU = (IUxx)l.SelectedIndex + 1;
                Question.DBSelect();
                LoadQuest(true);
            }
        }

        private void LoadTxt()
        {
            mTxt = new Txt();
            mTxt.ReadByte(Txt.sRPath + "samples/GUI-vi.bin");
            btnInsDate.Content = mTxt._[(int)TxI.DATE_ADD];
            btnNeeBrowse.Content = mTxt._[(int)TxI.NEE_ADD];
            btnQBrowse.Content = mTxt._[(int)TxI.Q_ADD];
        }
    }
}
