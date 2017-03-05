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

namespace WpfApplication1
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

        private void LoadStudents()
        {
            //if(dateId != -1)
            //    Student.ReadTxt(dateId);
            //if(Student.svStudent.Count == 0)

            //string[] students = null;
            //vStudent.Clear();
            //if (System.IO.File.Exists(filePath))
            //    students = System.IO.File.ReadAllLines(filePath);
            //if (students == null)
            //    return;
            //foreach (string s in students)
            //{
            //    if(date.Equals(s.Substring(0, 10)))
            //        vStudent.Add(s.Substring(10));
            //}
            //vStudent.Sort();
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
                tbxFilePath.Text = filePath = dlg.FileName;
            Student.ReadTxt(sQzCS.Utils.ReadFile(filePath));
            LoadStudents();
        }

        private void btnInsNee_Click(object sender, RoutedEventArgs e)
        {
            if(Date.mDBIdx != UInt32.MaxValue)
                Student.DBInsert(Date.mDBIdx);
        }

        private void lbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            ListBoxItem i = (ListBoxItem)l.SelectedItem;
            Date.DBIdx((string)i.Content);
            Student.DBSelect(Date.mDBIdx);
            LoadStudents();
        }
    }
}
