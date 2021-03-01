using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using sQzLib;

namespace sQzClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TakeExam : Page
    {
        DateTime kDtStart;
        TimeSpan dtRemn;
        DateTime dtLastLog;
        TimeSpan kLogIntvl;
        bool bRunning;
        bool bBtnBusy;
        UICbMsg mCbMsg;
        System.Timers.Timer mTimer;

        Dictionary<int, Label> SelectedLabels;

        const int SMT_OK_M = 20;
        const int SMT_OK_S = 60;

        public QuestSheet QuestionSheet;

        Client2 mClnt;
        NetCode mState;

        public static double qaWh;
        public ExamineeC thisExaminee;//reference to Auth.thisExaminee

        public TakeExam()
        {
            InitializeComponent();
            mState = NetCode.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;

            QuestionSheet = new QuestSheet();
        }

        private void LoadTxt()
        {
            txtAnsSh.Text = Txt.s._((int)TxI.ANS_SHEET);
            btnSubmit.Content = Txt.s._((int)TxI.SUBMIT);
            btnExit.Content = Txt.s._((int)TxI.EXIT);
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 16;

            double mrg = FontSize / 2;
            SingleQuestionView.IdxWidth = 3 * mrg;
            SingleQuestionView.staticMargin = new Thickness(mrg, mrg, 0, mrg);
            SingleQuestionView.StemWidth = (svwrQSh.Width - SystemParameters.ScrollWidth) / 2 - mrg - mrg - SingleQuestionView.IdxWidth;

            InitQuesttonSheetView();
            InitAnswerSheet();

            bBtnBusy = false;

            txtExamineeInfo.Text = thisExaminee.ToString();

            LoadTxt();

            WPopup.nwIns(w);
            WPopup.s.wpCbCncl = WPCancel;

            InitRemainingTime();

            txtRTime.Text = "" + dtRemn.Minutes + " : " + dtRemn.Seconds;
            kLogIntvl = new TimeSpan(0, 0, 30);

            System.Text.StringBuilder msg = new System.Text.StringBuilder();
            msg.Append(thisExaminee.ID + " (" + thisExaminee.Name + ")");
            if (thisExaminee.kDtDuration.Seconds == 0)
                msg.Append(Txt.s._((int)TxI.EXAMING_MSG_1));
            else
                msg.AppendFormat(Txt.s._((int)TxI.EXAMING_MSG_2),
                    thisExaminee.kDtDuration.Minutes, thisExaminee.kDtDuration.Seconds);
            WPopup.s.wpCb = ShowQuestion;
            spMain.Opacity = 0.5;
            WPopup.s.ShowDialog(msg.ToString());
            spMain.Opacity = 1;
            if (thisExaminee.eStt < NeeStt.Examing)
                thisExaminee.eStt = NeeStt.Examing;
            else if (thisExaminee.eStt == NeeStt.Submitting)
                Submit();
        }

        private void InitRemainingTime()
        {
            int m = -1, s = -1;
            if (thisExaminee.eStt < NeeStt.Submitting)
            {
                string t = null;
                if (System.IO.File.Exists("Duration.txt"))
                    t = System.IO.File.ReadAllText("Duration.txt");
                if (t != null)
                {
                    string[] vt = t.Split('\t');
                    if (vt.Length == 2)
                    {
                        int.TryParse(vt[0], out m);
                        int.TryParse(vt[1], out s);
                    }
                    if (-1 < m && -1 < s)
                        dtRemn = thisExaminee.kDtDuration = new TimeSpan(0, m, s);
                }
            }
            if (m < 0 || s < 0)
                dtRemn = thisExaminee.kDtDuration;
        }

        void ShowQuestion()
        {
            WPopup.s.wpCb = null;
            spMain.Effect = null;
            bBtnBusy = false;
            svwrQSh.Visibility = Visibility.Visible;

            mTimer = new System.Timers.Timer(1000);
            mTimer.Elapsed += UpdateSrvrMsg;
            mTimer.AutoReset = true;
            mTimer.Enabled = true;
            dtLastLog = kDtStart = DateTime.Now;
        }

        void InitAnswerSheet()
        {
            thisExaminee.AnswerSheet.Init(QuestionSheet);
            thisExaminee.AnswerSheet.bChanged = false;

            //left panel
            spLp.HorizontalAlignment = HorizontalAlignment.Left;
            spLp.Background = Theme.s._[(int)BrushId.LeftPanel_BG];
            //title
            Label l = new Label();
            gAnsSh.Background = Theme.s._[(int)BrushId.Sheet_BG];
            //int nAns = 4;//hardcode
            int n = QuestionSheet.CountAllQuestions();
            SolidColorBrush brBK = new SolidColorBrush(Colors.Black);
            //next lines
            //n -= 1;
            SelectedLabels = new Dictionary<int, Label>();
            int j = 1;
            for (; j < n; ++j)
            {
                gAnsSh.RowDefinitions.Add(new RowDefinition());
                l = new Label();
                l.Content = j;
                l.BorderBrush = brBK;
                l.BorderThickness = Theme.s.l[(int)ThicknessId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.FontWeight = FontWeights.Bold;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, 0);
                gAnsSh.Children.Add(l);
                l = new Label();
                l.BorderBrush = brBK;
                l.BorderThickness = Theme.s.l[(int)ThicknessId.RT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.FontWeight = FontWeights.Bold;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, 1);
                gAnsSh.Children.Add(l);
                SelectedLabels.Add(j, l);
            }
            //bottom lines
            ++j;
            gAnsSh.RowDefinitions.Add(new RowDefinition());
            l = new Label();
            l.Content = j;
            l.BorderBrush = brBK;
            l.BorderThickness = Theme.s.l[(int)ThicknessId.LB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.FontWeight = FontWeights.Bold;
            Grid.SetRow(l, j);
            Grid.SetColumn(l, 0);
            gAnsSh.Children.Add(l);
            l = new Label();
            l.BorderBrush = brBK;
            l.BorderThickness = Theme.s.l[(int)ThicknessId.RB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.FontWeight = FontWeights.Bold;
            Grid.SetRow(l, j);
            Grid.SetColumn(l, 1);
            gAnsSh.Children.Add(l);
            SelectedLabels.Add(j, l);

            //for (j = Question.svQuest[0].Count; -1 < j; --j)
            //    gAnsSh.RowDefinitions[j].Height = new GridLength(32, GridUnitType.Pixel);
        }

        //void InitQuesttonSheetView()
        //{
        //    QuestionSheetContainer.Background = Theme.s._[(int)BrushId.Q_BG];
        //    int n = QuestionSheet.Count;
        //    for (int i = 0, j = 0; i < n; i += 2, ++j)
        //    {
        //        QuestionSheetContainer.RowDefinitions.Add(new RowDefinition());
        //        SingleQuestionView q = new SingleQuestionView(QuestionSheet.Q(i), i, thisExaminee.AnswerSheet.BytesOfAnswer);
        //        Grid.SetRow(q, j);
        //        Grid.SetColumn(q, 0);
        //        QuestionSheetContainer.Children.Add(q);
        //        q.optionsView.SelectionChanged += OptionsView_SelectionChanged;
        //        q.optionsView.Name = "_" + i.ToString();
        //    }
        //    for (int i = 1, j = 0; i < n; i += 2, ++j)
        //    {
        //        SingleQuestionView q = new SingleQuestionView(QuestionSheet.Q(i), i, thisExaminee.AnswerSheet.BytesOfAnswer);
        //        Grid.SetRow(q, j);
        //        Grid.SetColumn(q, 1);
        //        QuestionSheetContainer.Children.Add(q);
        //        q.optionsView.SelectionChanged += OptionsView_SelectionChanged;
        //        q.optionsView.Name = "_" + i.ToString();
        //    }
        //    QuestionSheetContainer.Background = Theme.s._[(int)BrushId.BG];
        //}

        void InitQuesttonSheetView()
        {
            QuestionSheetView qsheetView = new QuestionSheetView(QuestionSheet, thisExaminee.AnswerSheet.BytesOfAnswer, FontSize * 2,
                svwrQSh.Width - FontSize * 2 - SystemParameters.ScrollWidth * 2);
            foreach(object i in qsheetView.Children)
            {
                SingleQuestionView q = i as SingleQuestionView;
                if(q != null)
                {
                    q.optionsView.SelectionChanged += OptionsView_SelectionChanged;
                    q.optionsView.Name = "_" + q.Idx.ToString();
                }
            }
            svwrQSh.Content = qsheetView;
        }

        private void OptionsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            thisExaminee.AnswerSheet.bChanged = true;
            ListBox l = sender as ListBox;
            if (l.SelectedItem == null)
                return;
            int qid = Convert.ToInt32(l.Name.Substring(1));
            int i = -1;
            foreach (ListBoxItem li in l.Items)
            {
                ++i;
                if (li.IsSelected)
                {
                    thisExaminee.AnswerSheet.BytesOfAnswer[qid * 4 + i] = 1;
                    OptionView v = li as OptionView;
                    if (v != null)
                        SelectedLabels[qid+1].Content = v.Idx_Label;
                }
                else
                    thisExaminee.AnswerSheet.BytesOfAnswer[qid * 4 + i] = 0;
            }
        }

        public void Submit()
        {
            bBtnBusy = true;//
            spMain.Effect = null;
            WPopup.s.wpCb = null;
            bRunning = false;
            DisableAll();
            mState = NetCode.Submiting;
            thisExaminee.eStt = NeeStt.Submitting;
            thisExaminee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
            if (mClnt.ConnectWR(ref mCbMsg))
                bBtnBusy = false;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            WPopup.s.wpCb = Submit;
            spMain.Opacity = 0.5;
            WPopup.s.ShowDialog(Txt.s._((int)TxI.SUBMIT_CAUT),
                Txt.s._((int)TxI.SUBMIT), Txt.s._((int)TxI.BTN_CNCL), null);
            spMain.Opacity = 1;
        }

        public bool ClntBufHndl(byte[] buf)
        {
            int offs = 0;
            switch (mState)
            {
                case NetCode.Submiting:
                    int rs;
                    string msg = null;
                    int l = buf.Length - offs;
                    if(l < 4)
                    {
                        rs = -1;
                        msg = Txt.s._((int)TxI.RECV_DAT_ER);
                    }
                    else
                        rs = BitConverter.ToInt32(buf, offs);
                    l -= 4;
                    offs += 4;
                    if(rs == 0)
                    {
                        ExamineeC e = new ExamineeC();
                        if (!e.ReadBytes_FromS1(buf, ref offs))
                        {
                            thisExaminee.MergeWithS1(e);
                            btnSubmit.Content = thisExaminee.Grade;
                            msg = Txt.s._((int)TxI.RESULT) + thisExaminee.Grade;
                        }
                        else
                            msg = Txt.s._((int)TxI.RECV_DAT_ER);
                    }
                    else if (rs == (int)TxI.NEEID_NF)
                        msg = Txt.s._((int)TxI.NEEID_NF);
                    else if (rs == (int)TxI.RECV_DAT_ER)
                        msg = Txt.s._((int)TxI.RECV_DAT_ER);
                    else if(msg == null)
                    {
                        if(l < 4)
                            msg = Txt.s._((int)TxI.RECV_DAT_ER);
                        else
                        {
                            int sz = BitConverter.ToInt32(buf, offs);
                            l -= 4;
                            offs += 4;
                            if(l < sz)
                                msg = Txt.s._((int)TxI.RECV_DAT_ER);
                            else
                                msg = System.Text.Encoding.UTF8.GetString(buf, offs, sz);
                        }
                    }
                    Dispatcher.Invoke(() => {
                        spMain.Opacity = 0.5;
                        WPopup.s.ShowDialog(msg);
                        spMain.Opacity = 1;
                    });
                    break;
            }
            bBtnBusy = false;
            return false;
        }

        public byte[] ClntBufPrep()
        {
            byte[] outBuf;
            switch (mState)
            {
                case NetCode.Submiting:
                    List<byte[]> bytes = new List<byte[]>();
                    bytes.Add(BitConverter.GetBytes((int)mState));
                    bytes.AddRange(thisExaminee.GetBytes_SendingToS1());
                    outBuf = Utils.ToArray_FromListOfBytes(bytes);
                    break;
                default:
                    outBuf = null;
                    break;
            }
            return outBuf;
        }

        private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning)
            {
                if (0 < dtRemn.Ticks)
                {
                    dtRemn = thisExaminee.kDtDuration - (DateTime.Now - kDtStart);
                    if (thisExaminee.AnswerSheet.bChanged && kLogIntvl < DateTime.Now - dtLastLog)
                    {
                        dtLastLog = DateTime.Now;
                        thisExaminee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = dtRemn.Minutes.ToString() + " : " + dtRemn.Seconds;
                        if (!btnSubmit.IsEnabled && dtRemn.Minutes < SMT_OK_M
                                && dtRemn.Seconds < SMT_OK_S)
                            btnSubmit.IsEnabled = true;
                    });
                }
                else
                {
                    dtRemn = new TimeSpan(0, 0, 0);
                    bRunning = false;
                    Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = "0 : 0";
                        spMain.Opacity = 0.5;
                        WPopup.s.ShowDialog(Txt.s._((int)TxI.TIMEOUT));
                        spMain.Opacity = 1;
                        Submit();
                    });
                }
            }
        }

        private void DisableAll()
        {
            btnSubmit.IsEnabled = false;
            mTimer.Stop();
            btnExit.IsEnabled = true;
            QuestionSheetView qs = svwrQSh.Content as QuestionSheetView;
            if (qs == null)
                return;
            //foreach (object child in QuestionSheetContainer.Children)
            foreach (object child in qs.Children)
            {
                SingleQuestionView question = child as SingleQuestionView;
                if(question != null)
                    question.optionsView.IsEnabled = false;
            }
        }

        void Exit()
        {
            //WPopup.s.wpCb = null;
            //bBtnBusy = false;
            if (thisExaminee.AnswerSheet.bChanged)
                thisExaminee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
            Window.GetWindow(this).Close();
        }

        void WPCancel()
        {
            bBtnBusy = false;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            WPopup.s.wpCb = Exit;
            spMain.Opacity = 0.5;
            if (thisExaminee.eStt < NeeStt.Submitting)
                WPopup.s.ShowDialog(Txt.s._((int)TxI.EXIT_CAUT_1),
                    Txt.s._((int)TxI.EXIT), Txt.s._((int)TxI.BTN_CNCL), "exit");
            else
                WPopup.s.ShowDialog(Txt.s._((int)TxI.EXIT_CAUT_2),
                    Txt.s._((int)TxI.EXIT), Txt.s._((int)TxI.BTN_CNCL), null);
            spMain.Opacity = 1;
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            mClnt.Close();
            WPopup.s.Exit();
        }
    }
}
