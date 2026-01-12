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

        List<Label> SelectedLabels;

        const int SMT_OK_M = 20;
        const int SMT_OK_S = 60;

        public QuestSheet QuestionSheet;

        Client2 mClnt;
        NetCode mState;

        public static double qaWh;
        public ExamineeC thisExaminee;//reference to Auth.thisExaminee

        public TakeExam()
        {
            App.EnableHookKeys(true);
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
            w.FontSize = 15;

            double mrg = FontSize / 2;
            SingleAnswerMCQView.IdxWidth = 3 * mrg;
            SingleAnswerMCQView.staticMargin = new Thickness(mrg, mrg, 0, mrg);
            //SingleQuestionView.StemWidth = (svwrQSh.Width - SystemParameters.ScrollWidth) / 2 - mrg - mrg - SingleQuestionView.IdxWidth;
            OptionViewFactory.S().StemWidth = (svwrQSh.Width - SystemParameters.ScrollWidth) / 2 - mrg - mrg - SingleAnswerMCQView.IdxWidth;

            InitQuesttonSheetView();
            InitAnswerSheet();

            bBtnBusy = false;

            txtExamineeInfo.Text = thisExaminee.ToString();

            LoadTxt();

            WPopup.nwIns(w);

            InitRemainingTime();

            txtRTime.Text = "" + Utils.GetMinutes(dtRemn) + " : " + dtRemn.Seconds;
            kLogIntvl = new TimeSpan(0, 0, 30);

            System.Text.StringBuilder msg = new System.Text.StringBuilder();
            msg.Append(thisExaminee.ID + " (" + thisExaminee.Name + ")");
            if (thisExaminee.kDtDuration.Equals(thisExaminee.FullTestDuration))
                msg.AppendFormat(Txt.s._((int)TxI.EXAMING_MSG_1),
                    Utils.GetMinutes(thisExaminee.kDtDuration));
            else
                msg.AppendFormat(Txt.s._((int)TxI.EXAMING_MSG_2),
                    thisExaminee.kDtDuration.Minutes, thisExaminee.kDtDuration.Seconds);
            spMain.Opacity = 0.5;
            WPopup.s.ShowDialog(msg.ToString(), ShowQuestion);
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
            spMain.Effect = null;
            bBtnBusy = false;
            svwrQSh.Visibility = Visibility.Visible;

            mTimer = new System.Timers.Timer(1000);
            mTimer.Elapsed += UpdateSrvrMsg;
            mTimer.AutoReset = true;
            mTimer.Enabled = true;
            dtLastLog = kDtStart = DateTime.Now;
        }

        void AnswerSheet_NewCellAtBottomRow(string constLabel, int column, Brush border, Grid answerPart)
        {
            Label cell = new Label();
            int bottomRow = answerPart.RowDefinitions.Count - 1;
            Grid.SetRow(cell, bottomRow);
            Grid.SetColumn(cell, column);
            cell.BorderBrush = border;
            cell.HorizontalContentAlignment = HorizontalAlignment.Center;
            cell.FontWeight = FontWeights.Bold;
            cell.BorderThickness = Theme.s.l[(int)ThicknessId.RB];

            if (constLabel.Length > 0)
            {
                cell.Content = constLabel;
            }
            else
            {
                /*char labelFromSaveFile;
                if (LabelFromSaveFile(bottomRow, out labelFromSaveFile))
                    cell.Content = labelFromSaveFile;*/

                SelectedLabels.Add(cell);
            }

            answerPart.Children.Add(cell);
        }

        void AnswerSheetView_AddSingleAnswerHeader(Brush border)
        {
            AnswerSheetP1.RowDefinitions.Add(new RowDefinition());

            AnswerSheet_NewCellAtBottomRow("A", 1, border, AnswerSheetP1);
            AnswerSheet_NewCellAtBottomRow("B", 2, border, AnswerSheetP1);
            AnswerSheet_NewCellAtBottomRow("C", 3, border, AnswerSheetP1);
            AnswerSheet_NewCellAtBottomRow("D", 4, border, AnswerSheetP1);
        }

        void AnswerSheetView_AddSingleAnswer(string questionIdx, Brush border, bool isLastRow)
        {
            AnswerSheetP1.RowDefinitions.Add(new RowDefinition());

            AnswerSheet_NewCellAtBottomRow(questionIdx, 0, border, AnswerSheetP1);
            AnswerSheet_NewCellAtBottomRow("", 1, border, AnswerSheetP1);
            AnswerSheet_NewCellAtBottomRow("", 2, border, AnswerSheetP1);
            AnswerSheet_NewCellAtBottomRow("", 3, border, AnswerSheetP1);
            AnswerSheet_NewCellAtBottomRow("", 4, border, AnswerSheetP1);
        }

        void AnswerSheetView_AddMTFAnswerHeader(Brush border)
        {
            AnswerSheetP2.RowDefinitions.Add(new RowDefinition());
            AnswerSheet_NewCellAtBottomRow("Đúng", 1, border, AnswerSheetP2);
            AnswerSheet_NewCellAtBottomRow("Sai", 2, border, AnswerSheetP2);
        }

        void AnswerSheetView_AddMTFAnswer(string questionIdx1, Brush border, bool isLastRow)
        {
            char option = 'a';
            int i = 0;
            while(i < OptionSelectAnswer.OPTION_COUNT)
            {
                AnswerSheetP2.RowDefinitions.Add(new RowDefinition());

                AnswerSheet_NewCellAtBottomRow(questionIdx1 + '.' + option, 0, border, AnswerSheetP2);
                AnswerSheet_NewCellAtBottomRow("", 1, border, AnswerSheetP2);
                AnswerSheet_NewCellAtBottomRow("", 2, border, AnswerSheetP2);

                ++option;
                ++i;
            }
        }

        void AnswerSheetView_AddAtBottomRow(int questionIdx, Brush border, bool isLastRow)
        {
            Question question = QuestionSheet.ElementAt(questionIdx);
            if (question == null)
                return;

            if (question.QuestionType == AnswerType.SingleAnswer)
                AnswerSheetView_AddSingleAnswer((questionIdx + 1).ToString(), border, isLastRow);
            else if (question.QuestionType == AnswerType.MultipleTrueFalse)
                AnswerSheetView_AddMTFAnswer((questionIdx + 1).ToString(), border, isLastRow);
        }

        void InitAnswerSheet()
        {
            thisExaminee.AnswerSheet.Init(QuestionSheet);
            thisExaminee.AnswerSheet.bChanged = false;

            //left panel
            spLp.HorizontalAlignment = HorizontalAlignment.Left;
            spLp.Background = Theme.s._[(int)BrushId.LeftPanel_BG];

            AnswerSheetP1.Background = Theme.s._[(int)BrushId.Sheet_BG];
            int n = QuestionSheet.CountAllQuestions();
            SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);

            AnswerSheetView_AddMTFAnswerHeader(blackBrush);
            AnswerSheetView_AddSingleAnswerHeader(blackBrush);

            //next lines
            SelectedLabels = new List<Label>();
            
            for (int j = 0; j < n - 1; ++j)
                AnswerSheetView_AddAtBottomRow(j, blackBrush, false);

            //bottom lines
            AnswerSheetView_AddAtBottomRow(n -1, blackBrush, true);
        }

        private bool LabelFromSaveFile(int questionIdx, out char label)
        {
            label = ' ';
            return false;
            /*MUST DO IT LATER
            bool noChoice = true;
            label = 'A';
            for(int optionIdx = questionIdx * OptionSelectAnswer.OPTION_COUNT,
                end = optionIdx + OptionSelectAnswer.OPTION_COUNT;
                optionIdx < end; ++optionIdx)
            {
                if (thisExaminee.AnswerSheet.BytesOfAnswer[optionIdx] == QuestionAnswer.TRUE)
                {
                    noChoice = false;
                    break;
                }
                ++label;
                if (thisExaminee.AnswerSheet.BytesOfAnswer[optionIdx] == QuestionAnswer.FALSE)
                    noChoice = false;
            }

            if (noChoice)
                return false;

            return true;*/
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

        void SetOptionViewEventHandler(SingleAnswerMCQView questView)
        {
            ListBox listBoxOption = questView.optionsView as ListBox;
            if (listBoxOption != null)
            {
                listBoxOption.SelectionChanged += ListBoxOption_SelectionChanged;

                return;
            }

            StackPanel radioOption = questView.optionsView as StackPanel;
            if (radioOption != null)
            {
                if (radioOption.Children.Count != 12)
                    MessageBox.Show("TrueFalseOptions count is out of range: "
                        + radioOption.Children.Count);
                else
                {
                    foreach(UIElement elem in radioOption.Children)
                    {
                        RadioButton TF_option = elem as RadioButton;
                        if(TF_option != null)
                        {
                            TF_option.Checked += RadioOption_Checked;
                            /*Only need to handle checked event
                            TF_option.Unchecked += RadioOption_SelectionChanged;*/
                        }
                    }
                }
            }
        }

        void InitQuesttonSheetView()
        {
            QuestionSheetView qsheetView = new QuestionSheetView(QuestionSheet,
                thisExaminee.AnswerSheet.BytesOfAnswer, FontSize * 2,
                svwrQSh.Width - FontSize * 2 - SystemParameters.ScrollWidth * 4);
            foreach(object i in qsheetView.Children)
            {
                SingleAnswerMCQView q = i as SingleAnswerMCQView;
                if(q != null)
                    SetOptionViewEventHandler(q);
                else
                {
                    BasicPassageSectionView p = i as BasicPassageSectionView;
                    if(p != null)
                    {
                        foreach(SingleAnswerMCQView q_in_p in p.QuestionsViews)
                            SetOptionViewEventHandler(q_in_p);
                    }
                }
            }
            svwrQSh.Content = qsheetView;
        }

        int GetBytesOfAnswerIdxByQuesIdx(int questIdx, int optionIdx)
        {
            return questIdx * 4 + optionIdx;
        }

        private void RadioOption_Checked(object sender, RoutedEventArgs e)
        {
            thisExaminee.AnswerSheet.bChanged = true;

            RadioButton radio = sender as RadioButton;

            if (radio == null)
            {
                MessageBox.Show("Option is not radio button.");
                return;
            }

            int questIdx = int.Parse(radio.Name.Substring(1,
                radio.Name.IndexOf('_', 1) - 1));
            int x = radio.Name.IndexOf("__", 0) + 2;
            int optionIdx = int.Parse(radio.Name.Substring(x,
                radio.Name.IndexOf("___", 0) - x));

            int BOA_idx = GetBytesOfAnswerIdxByQuesIdx(questIdx, optionIdx);
            IEnumerator<Label> label = GetAnswerLabelIdx(questIdx, optionIdx);

            if(radio.Name.EndsWith("True") && radio.IsChecked == true)
            {
                thisExaminee.AnswerSheet.BytesOfAnswer[BOA_idx] = QuestionAnswer.TRUE;

                label.Current.Content = "X";
                label.MoveNext();
                label.Current.Content = "";
            }
            else if(radio.Name.EndsWith("False") && radio.IsChecked == true)
            {
                thisExaminee.AnswerSheet.BytesOfAnswer[BOA_idx] = QuestionAnswer.FALSE;

                label.Current.Content = "";
                label.MoveNext();
                label.Current.Content = "X";
            }
            else
            {
                /*MessageBox.Show("Unhandled radio name: " + radio.Name +
                    ". Event: " + radio.IsChecked);*/
            }
        }

        private void ListBoxOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            thisExaminee.AnswerSheet.bChanged = true;

            ListBox options = sender as ListBox;

            if (options == null)
            {
                MessageBox.Show("Option is not list box.");
                return;
            }

            if (options.SelectedItem == null)
            {
                MessageBox.Show("Option list box has no selected item.");
                return;
            }

            int questIdx = Convert.ToInt32(options.Name.Substring(1));
            int BOA_idx = GetBytesOfAnswerIdxByQuesIdx(questIdx, 0);
            IEnumerator<Label> label = GetAnswerLabelIdx(questIdx, 0);
            int i = 0;
            foreach (ListBoxItem li in options.Items)
            {
                if (li.IsSelected)
                {
                    thisExaminee.AnswerSheet.BytesOfAnswer[BOA_idx + i] = QuestionAnswer.TRUE;
                    label.Current.Content = "X";
                }
                else
                {
                    thisExaminee.AnswerSheet.BytesOfAnswer[BOA_idx + i] = QuestionAnswer.FALSE;
                    label.Current.Content = "";
                }

                ++i;
                label.MoveNext();
            }
        }

        IEnumerator<Label> GetAnswerLabelIdx(int questIdx, int optionIdx)
        {
            IEnumerator<Label> labelIdx = SelectedLabels.GetEnumerator();

            foreach (QSheetSection section in QuestionSheet.Sections)
            {
                foreach (Question quest in section.Questions)
                {
                    if (questIdx <= 0)
                    {
                        labelIdx.MoveNext();

                        if (quest.QuestionType == AnswerType.MultipleTrueFalse)
                        {
                            for (int i = optionIdx * 2; i > 0; --i)
                                labelIdx.MoveNext();
                        }
                            
                        return labelIdx;
                    }

                    int option_pass;
                    if(quest.QuestionType == AnswerType.MultipleTrueFalse)
                        option_pass = OptionSelectAnswer.OPTION_COUNT * 2;
                    else
                        option_pass = OptionSelectAnswer.OPTION_COUNT;

                    for (int i = option_pass; i > 0 ; --i)
                        labelIdx.MoveNext();

                    --questIdx;
                }
            }

            return labelIdx;
        }

        public void Submit()
        {
            bBtnBusy = true;//
            spMain.Effect = null;
            bRunning = false;
            DisableAll();
            mState = NetCode.Submiting;
            thisExaminee.eStt = NeeStt.Submitting;
            thisExaminee.ToLogFile(Utils.GetMinutes(dtRemn), dtRemn.Seconds);
            if (mClnt.ConnectWR(ref mCbMsg))
            {
                bBtnBusy = false;
                OnSubmitConnectionFail();
            }
        }

        private void OnSubmitConnectionFail()
        {
            App.EnableHookKeys(false);
            WPopup.s.ShowDialog(Txt.s._((int)TxI.ON_SUBMIT_NO_CONNECTION),
                Txt.s._((int)TxI.SUBMIT), Txt.s._((int)TxI.EXIT),
                string.Empty, ResubmitAfterConnectionFail, ShowExitDiaglogBox);
        }

        private void ResubmitAfterConnectionFail()
        {
            bBtnBusy = true;
            if (mClnt.ConnectWR(ref mCbMsg))
            {
                bBtnBusy = false;
                OnSubmitConnectionFail();
            }
        }

        private void ShowExitDiaglogBox()
        {
            if (thisExaminee.eStt < NeeStt.Submitting)
                WPopup.s.ShowDialog(Txt.s._((int)TxI.EXIT_CAUT_1),
                    Txt.s._((int)TxI.EXIT), Txt.s._((int)TxI.BTN_CNCL), "exit", Exit, WPopupCancel);
            else
                WPopup.s.ShowDialog(Txt.s._((int)TxI.EXIT_CAUT_2),
                    Txt.s._((int)TxI.EXIT), Txt.s._((int)TxI.BTN_CNCL), null, Exit, WPopupCancel);

        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            spMain.Opacity = 0.5;
            WPopup.s.ShowDialog(Txt.s._((int)TxI.SUBMIT_CAUT),
                Txt.s._((int)TxI.SUBMIT), Txt.s._((int)TxI.BTN_CNCL),
                string.Empty, Submit, WPopupCancel);
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
                            btnSubmit.Foreground = new SolidColorBrush(Colors.Black);
                            msg = Txt.s._((int)TxI.RESULT) + thisExaminee.Grade;
                        }
                        else
                            msg = Txt.s._((int)TxI.RECV_DAT_ER);
                    }
                    else if (rs == (int)TxI.NEEID_NF)
                        msg = Txt.s._((int)TxI.NEEID_NF);
                    else if (rs == (int)TxI.QS_NFOUND)
                        msg = Txt.s._((int)TxI.QS_NFOUND) + thisExaminee.AnswerSheet.QuestSheetID;
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
                        WPopup.s.ShowDialog(msg, WPopupCancel);
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
                        thisExaminee.ToLogFile(Utils.GetMinutes(dtRemn), dtRemn.Seconds);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = Utils.GetMinutes(dtRemn).ToString() + " : " + dtRemn.Seconds;
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
                        WPopup.s.ShowDialog(Txt.s._((int)TxI.TIMEOUT), WPopupCancel);
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
                SingleAnswerMCQView question = child as SingleAnswerMCQView;
                if(question != null)
                    question.optionsView.IsEnabled = false;
                BasicPassageSectionView passage = child as BasicPassageSectionView;
                if (passage != null)
                    foreach (SingleAnswerMCQView q_view in passage.QuestionsViews)
                        q_view.optionsView.IsEnabled = false;
            }
        }

        void Exit()
        {
            bBtnBusy = false;
            if (thisExaminee.AnswerSheet.bChanged)
                thisExaminee.ToLogFile(Utils.GetMinutes(dtRemn), dtRemn.Seconds);
            Window.GetWindow(this).Close();
        }

        void WPopupCancel()
        {
            bBtnBusy = false;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            spMain.Opacity = 0.5;
            ShowExitDiaglogBox();
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
