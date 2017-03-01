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
    /// Interaction logic for ExamList.xaml
    /// </summary>
    public partial class ExamList : Page
    {
        List<string> vDate;
        List<string> vStudentInfo;
        List<Label> vTxtDate;
        int mDateIdx;

        public ExamList()
        {
            InitializeComponent();
            ShowsNavigationUI = false;
            vDate = new List<string>();
            vStudentInfo = new List<string>();
            vTxtDate = new List<Label>();
            mDateIdx = -1;
            TakeExam.InitBrush();
            LoadDates();
            ShowDates();
        }

        private void LoadDates()
        {
            string filePath = "ExamList.txt";
            string[] dates = null;
            if (System.IO.File.Exists(filePath))
                dates = System.IO.File.ReadAllLines(filePath);
            if (dates == null)
                return;
            foreach (string d in dates)
                vDate.Add(d);
            vDate.Sort();
        }

        private void ShowDates()
        {
            if (vDate.Count == 0)
                return;
            StackPanel sp = new StackPanel();
            for(int i = 0; i < vDate.Count; ++i)
            {
                Label t = new Label();
                DecorateTxt(ref t);
                t.Content = vDate[i];
                t.MouseLeftButtonUp += HighlightTxt;
                vTxtDate.Add(t);
                sp.Children.Add(t);
            }
            gMain.Children.Add(sp);
        }

        private void DecorateTxt(ref Label t)
        {
            t.Background = TakeExam.vBrush[(int)BrushId.Q_BG];
            t.BorderBrush = new SolidColorBrush(Colors.Gray);
            t.BorderThickness = new Thickness(1);
            t.FontSize = TakeExam.em;
        }

        private void HighlightTxt(object sender, MouseButtonEventArgs e)
        {
            Label t = (Label)sender;
            if (0 < mDateIdx)
            {
                t.Background = TakeExam.vBrush[(int)BrushId.Q_BG];
                t.BorderBrush = new SolidColorBrush(Colors.Gray);
                t.BorderThickness = new Thickness(1);
            }
            t.Background = TakeExam.vBrush[(int)BrushId.Ans_Highlight];
        }

        private void LoadStudents(string date)
        {
            string filePath = "ExamList.txt";
            string[] dates = null;
            if (System.IO.File.Exists(filePath))
                dates = System.IO.File.ReadAllLines(filePath);
            if (dates == null)
                return;
            foreach (string d in dates)
                vDate.Add(d);
            vDate.Sort();
        }

        private void ShowStudents()
        {

        }

        private void gMain_Loaded(object sender, RoutedEventArgs e)
        {
            gMain.Background = TakeExam.vBrush[(int)BrushId.BG];
        }
    }
}
