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
using System.Threading;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Operation.xaml
    /// </summary>
    public partial class Operation0 : Page
    {
        List<string> vDate;
        List<string> vStudent;
        string mSelectedDate;
        Server0 mServer;
        bool bSrvrMsg;
        string mSrvrMsg;

        public Operation0()
        {
            InitializeComponent();
            ShowsNavigationUI = false;
            vDate = new List<string>();
            vStudent = new List<string>();
            mSelectedDate = String.Empty;
            mServer = new Server0(ResponseMsg);
            bSrvrMsg = false;
            mSrvrMsg = String.Empty;

            lbxDate.SelectionMode = SelectionMode.Single;
            lbxDate.SelectionChanged += lbxDate_SelectionChanged;
            TakeExam.InitBrush();
            LoadDates();

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool dummy1 = false;
            string dummy2 = null;
            mServer.Stop(ref dummy1, ref dummy2);
        }

        public string ResponseMsg(char code)
        {
            string msg = null;
            switch (code)
            {
                case (char)RequestCode.DateStudentRetriving:
                    msg = mSelectedDate + "\n";
                    foreach(string s in vStudent)
                        msg += s + "\n";
                    break;
                case (char)RequestCode.QuestAnsKeyRetrieving:
                    //msg = ;
                    break;
                case (char)RequestCode.MarkSubmitting:
                    break;
                default:
                    msg = "unknown";
                    break;
            }
            return msg;
        }

        private void lbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            mSelectedDate = (string)((ListBoxItem)l.SelectedItem).Content;
            LoadStudents(mSelectedDate);
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
            //lblStatus.FontSize = TakeExam.em;
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
            w.Closing += W_Closing;

            PrepDatesGUI();

            double scaleW = spMain.RenderSize.Width / 640; //d:DesignWidth
            //double scaleH = spMain.RenderSize.Height / 360; //d:DesignHeight
            ScaleScreen(scaleW);

            FirewallHandler fwHndl = new FirewallHandler(0);
            string msg = fwHndl.OpenFirewall();
            lblStatus.Text = msg;
        }

        private void StartSvrt_Click(object sender, RoutedEventArgs e)
        {

            Thread th = new Thread(() => { mServer.Start(ref bSrvrMsg, ref mSrvrMsg); /*StartSrvr(ref bSrvrMsg, ref mSrvrMsg); */});
            th.Start();
        }

        private void StartSrvr(ref bool bUpdate, ref string msg)
        {
            mServer.Start(ref bUpdate, ref msg);
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (bSrvrMsg)
                Dispatcher.Invoke(() => {
                    lblStatus.Text += mSrvrMsg; bSrvrMsg = false; mSrvrMsg = String.Empty; });
        }

        private void StopSvrv_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref bSrvrMsg, ref mSrvrMsg);
        }

        private void op1_Click(object sender, RoutedEventArgs e)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    NavigationService.Navigate(new Uri("Operation1.xaml", UriKind.Relative));
            //});
            Window w = (Window)Parent;
            w.Close();
        }
    }
}
