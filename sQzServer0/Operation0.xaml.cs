﻿using System;
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
using sQzLib;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Operation.xaml
    /// </summary>
    public partial class Operation0 : Page
    {
        Server0 mServer;
        bool bSrvrMsg;
        string mSrvrMsg;
        byte[] vQuestAnsKey;
        //byte[] vDateStud;

        public Operation0()
        {
            InitializeComponent();
            ShowsNavigationUI = false;
            mServer = new Server0(ResponseMsg);
            bSrvrMsg = false;
            mSrvrMsg = String.Empty;

            lbxDate.SelectionMode = SelectionMode.Single;
            lbxDate.SelectionChanged += lbxDate_SelectionChanged;
            Theme.InitBrush();
            LoadDates();

            vQuestAnsKey = null;
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool dummy1 = false;
            string dummy2 = null;
            mServer.Stop(ref dummy1, ref dummy2);
        }

        public byte[] ResponseMsg(char code)
        {
            byte[] msg = null;
            switch (code)
            {
                case (char)NetSttCode.DateStudentRetriving:
                    int sz = 0;
                    if (Date.sbArr != null)
                        sz += Date.sbArr.Length;
                    if (Student.sbArr != null)
                        sz += Student.sbArr.Length;
                    msg = new byte[sz];
                    sz = 0;
                    if (Date.sbArr != null)
                    {
                        sz = Date.sbArr.Length;
                        Buffer.BlockCopy(Date.sbArr, 0, msg, 0, sz);
                    }
                    if(Student.sbArr != null)
                        Buffer.BlockCopy(Student.sbArr, 0, msg, sz, Student.sbArr.Length);
                    break;
                case (char)NetSttCode.QuestAnsKeyRetrieving:
                    if (vQuestAnsKey == null)
                        msg = BitConverter.GetBytes((char)NetSttCode.Unknown);
                    else
                        msg = vQuestAnsKey;
                    break;
                case (char)NetSttCode.MarkSubmitting:
                    break;
                default:
                    msg = BitConverter.GetBytes((char)NetSttCode.Unknown);
                    break;
            }
            return msg;
        }

        private void lbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            ListBoxItem i = (ListBoxItem)l.SelectedItem;
            Date.Select((string)i.Content);
            Student.DBSelect(Date.sDBIdx);
            LoadStudents();
        }

        //private void LoadDates()
        //{
        //    string filePath = "Dates.txt";
        //    string[] dates = null;
        //    vDate.Clear();
        //    if (System.IO.File.Exists(filePath))
        //        dates = System.IO.File.ReadAllLines(filePath);
        //    if (dates == null)
        //        return;
        //    foreach (string d in dates)
        //    {
        //        string[] s = d.Split('\t');
        //        vDateId.Add(Convert.ToInt16(s[0]));
        //        vDate.Add(s[1]);
        //    }
        //    vDate.Sort();
        //}

        private void LoadDates()
        {
            Date.DBSelect();
            if (0 < Date.svDate.Count)
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

        //private void PrepDatesGUI()
        //{
        //    if (vDate.Count == 0)
        //    {
        //        Dispatcher.Invoke(() =>
        //        {
        //            lbxDate.Items.Clear();
        //        });
        //        return;
        //    }
        //    bool dark = true;
        //    Color c = new Color();
        //    c.A = 0xff;
        //    c.B = c.G = c.R = 0xf0;
        //    Dispatcher.Invoke(() =>
        //    {
        //        lbxDate.Items.Clear();
        //        for (int i = 0; i < vDate.Count; ++i)
        //        {
        //            ListBoxItem t = new ListBoxItem();
        //            t.Content = vDate[i];
        //            dark = !dark;
        //            if (dark)
        //                t.Background = new SolidColorBrush(c);
        //            t.FontSize = TakeExam.em;
        //            lbxDate.Items.Add(t);
        //        }
        //    });
        //}

        //private void LoadStudents(short dateId)
        //{
        //    Student.ReadTxt(dateId);
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
        //    PrepStudentsGUI();
        //}

        private void LoadStudents() //same as Prep0.xaml
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

        //private void PrepStudentsGUI()
        //{
        //    if (vStudent.Count == 0)
        //    {
        //        Dispatcher.Invoke(() => { spDown.Children.Clear(); });
        //        return;
        //    }
        //    Dispatcher.Invoke(() =>
        //    {
        //        spDown.Children.Clear();
        //        bool dark = true;
        //        Color c = new Color();
        //        c.A = 0xff;
        //        c.B = c.G = c.R = 0xf0;
        //        for (int i = 0; i < vStudent.Count; ++i)
        //        {
        //            Label t = new Label();
        //            t.Content = vStudent[i];
        //            dark = !dark;
        //            if (dark)
        //                t.Background = new SolidColorBrush(c);
        //            t.FontSize = TakeExam.em;
        //            spDown.Children.Add(t);
        //        }
        //    });
        //}

        private void ScaleScreen(double r)
        {
            spUp.Height = spUp.Height * r;
            spUp.Background = new SolidColorBrush(Colors.AliceBlue);
            //spLeft.Width = spLeft.Width * r;
            lblStatus.Height = spLeft.Height * r;
            txtStatus.FontSize = Theme.em;
            lblStatus.Width = lblStatus.Width * r;
            //lblStatus.FontSize = TakeExam.em;
            spLeft.Background = new SolidColorBrush(Colors.AntiqueWhite);
            spCenter.Height = spCenter.Height * r;
            spCenter.Background = new SolidColorBrush(Colors.Aqua);
        }

        private void spMain_Loaded(object sender, RoutedEventArgs e)
        {
            spMain.Background = Theme.vBrush[(int)BrushId.BG];
            Window w = (Window)Parent;
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.Closing += W_Closing;

            //PrepDatesGUI();
            LoadDates();

            double scaleW = spMain.RenderSize.Width / 640; //d:DesignWidth
            //double scaleH = spMain.RenderSize.Height / 360; //d:DesignHeight
            ScaleScreen(scaleW);

            FirewallHandler fwHndl = new FirewallHandler(0);
            string msg = fwHndl.OpenFirewall();
            lblStatus.Text = msg;

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void StartSrvr_Click(object sender, RoutedEventArgs e)
        {
            Thread th = new Thread(() => {mServer.Start(ref bSrvrMsg, ref mSrvrMsg);});
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

        private void StopSrvr_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref bSrvrMsg, ref mSrvrMsg);
        }

        private void btnQSheet_Click(object sender, RoutedEventArgs e)
        {
            Question.DBSelect();
        }

        private void btnPrep_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                NavigationService.Navigate(new Uri("Prep0.xaml", UriKind.Relative));
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window w = (Window)Parent;
            w.Close();
        }
    }
}