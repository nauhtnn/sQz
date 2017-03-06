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
        public Prep0()
        {
            InitializeComponent();
        }

        private void btnInsDate_Click(object sender, RoutedEventArgs e)
        {
            if (Date.ChkFmt(tbxDate.Text))
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

        private void spMain_Loaded(object sender, RoutedEventArgs e)
        {
            double rt = spMain.RenderSize.Width / 640; //d:DesignWidth
            //double scaleH = spMain.RenderSize.Height / 360; //d:DesignHeight

            ScaleTransform st = new ScaleTransform(rt, rt);
            spMain.RenderTransform = st;
            LoadDate();
        }

        private void LoadStudents() //same as Operation0.xaml
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
                lbxStudent.Items.Clear();
                foreach (Student s in Student.svStudent)
                {
                    ListBoxItem i = new ListBoxItem();
                    i.Content = s.ToString();
                    dark = !dark;
                    if (dark)
                        i.Background = new SolidColorBrush(c);
                    lbxStudent.Items.Add(i);
                }
            });
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            // set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "text documents (.txt)|*.txt";
            bool? result = dlg.ShowDialog();

            // get the selected file name and display in a textbox
            string filePath = null;
            if (result == true)
                tbxNeePath.Text = filePath = dlg.FileName;
            Student.ReadTxt(Utils.ReadFile(filePath));
            LoadStudents();
        }

        private void btnInsNee_Click(object sender, RoutedEventArgs e)
        {
            if(Date.sDBIdx != UInt32.MaxValue)
                Student.DBInsert(Date.sDBIdx);
        }

        private void lbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            ListBoxItem i = (ListBoxItem)l.SelectedItem;
            Date.Select((string)i.Content);
            Student.DBSelect(Date.sDBIdx);
            LoadStudents();
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
                tbxQPath.Text = filePath = dlg.FileName;
            Question.ReadTxt(Utils.ReadFile(filePath));
            LoadQuest();
        }

        private void LoadQuest() //same as Operation0.xaml
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
                lbxStudent.Items.Clear();
                foreach (Question q in Question.svQuest)
                {
                    TextBlock i = new TextBlock();
                    i.Text = q.ToString();
                    dark = !dark;
                    if (dark)
                        i.Background = new SolidColorBrush(c);
                    gQuest.Children.Add(i);
                }
            });
        }

        private void btnInsQuest_Click(object sender, RoutedEventArgs e)
        {
            Question.DBInsert();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Question.DBSelect();
            LoadQuest();
        }
    }
}
