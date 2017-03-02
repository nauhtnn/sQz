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
    /// Interaction logic for Operation.xaml
    /// </summary>
    public partial class Operation : Page
    {
        List<string> vDate;
        List<string> vStudent;

        public Operation()
        {
            InitializeComponent();
            ShowsNavigationUI = false;
            vDate = new List<string>();
            vStudent = new List<string>();
            lbxDate.SelectionMode = SelectionMode.Single;
            lbxDate.SelectionChanged += lbxDate_SelectionChanged;
            TakeExam.InitBrush();
            LoadDates();
        }

        private void lbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            LoadStudents((string)((ListBoxItem)l.SelectedItem).Content);
        }

        private void LoadDates()
        {
            string filePath = "Dates.txt";
            string[] dates = null;
            vDate.Clear();
            if (System.IO.File.Exists(filePath))
                dates = System.IO.File.ReadAllLines(filePath);
            if (dates == null)
                return;
            foreach (string d in dates)
                vDate.Add(d);
            vDate.Sort();
        }

        private void PrepDatesGUI()
        {
            if (vDate.Count == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    lbxDate.Items.Clear();
                });
                return;
            }
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() =>
            {
                lbxDate.Items.Clear();
                for (int i = 0; i < vDate.Count; ++i)
                {
                    ListBoxItem t = new ListBoxItem();
                    t.Content = vDate[i];
                    dark = !dark;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    t.FontSize = TakeExam.em;
                    lbxDate.Items.Add(t);
                }
            });
        }

        private void LoadStudents(string date)
        {
            string filePath = "Students.txt";
            string[] students = null;
            vStudent.Clear();
            if (System.IO.File.Exists(filePath))
                students = System.IO.File.ReadAllLines(filePath);
            if (students == null)
                return;
            foreach (string s in students)
            {
                if(date.Equals(s.Substring(0, 10)))
                    vStudent.Add(s.Substring(10));
            }
            vStudent.Sort();
            PrepStudentsGUI();
        }

        private void PrepStudentsGUI()
        {
            if (vStudent.Count == 0)
            {
                Dispatcher.Invoke(() => { spDown.Children.Clear(); });
                return;
            }
            Dispatcher.Invoke(() =>
            {
                spDown.Children.Clear();
                bool dark = true;
                Color c = new Color();
                c.A = 0xff;
                c.B = c.G = c.R = 0xf0;
                for (int i = 0; i < vStudent.Count; ++i)
                {
                    Label t = new Label();
                    t.Content = vStudent[i];
                    dark = !dark;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    t.FontSize = TakeExam.em;
                    spDown.Children.Add(t);
                }
            });
        }

        private void ScaleScreen(double r)
        {
            spUp.Height = spUp.Height * r;
            spUp.Background = new SolidColorBrush(Colors.AliceBlue);
            //spLeft.Width = spLeft.Width * r;
            lblStatus.Height = spLeft.Height * r;
            txtStatus.FontSize = TakeExam.em;
            lblStatus.Width = lblStatus.Width * r;
            lblStatus.FontSize = TakeExam.em;
            lblStatus.Text = "abc\n\n\nabc";
            spLeft.Background = new SolidColorBrush(Colors.AntiqueWhite);
            spCenter.Height = spCenter.Height * r;
            spCenter.Background = new SolidColorBrush(Colors.Aqua);
            svwrDown.Height = svwrDown.Height * r;
        }

        private void spMain_Loaded(object sender, RoutedEventArgs e)
        {
            spMain.Background = TakeExam.vBrush[(int)BrushId.BG];
            Window w = (Window)Parent;
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;

            PrepDatesGUI();

            double scaleW = spMain.RenderSize.Width / 640; //d:DesignWidth
            //double scaleH = spMain.RenderSize.Height / 360; //d:DesignHeight
            ScaleScreen(scaleW);
        }
    }
}
