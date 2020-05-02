using System;
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

        const int SMT_OK_M = 20;
        const int SMT_OK_S = 60;

        public QuestSheet mQuestSheet;

        Client2 mClnt;
        NetPhase mState;

        public static double qaWh;
        double qiWh;
        Thickness qMrg;
        public ExamineeA mExaminee;//reference to Auth.mNee

        public TakeExam()
        {
            InitializeComponent();
            mState = NetPhase.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;

            mExaminee = new ExamineeC();

            mQuestSheet = new QuestSheet();
        }

        private void LoadTxt()
        {
            AnswerTitle.Text = Txt.s._[(int)TxI.ANS_SHEET];
            btnSubmit.Content = Txt.s._[(int)TxI.SUBMIT];
            btnExit.Content = Txt.s._[(int)TxI.EXIT];
        }

        private void SetWindowFullScreen()
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 16;

            PopupMgr.SetParentWindow(w);
            PopupMgr.Singleton.CbNOK = WPCancel;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowFullScreen();

            double mrg = FontSize / 2;
            qiWh = 3 * mrg;
            qMrg = new Thickness(mrg, mrg, 0, mrg);
            qaWh = (QuestSheetView.Width - SystemParameters.ScrollWidth) / 2 - mrg - mrg - qiWh;

            SetAnswerSheetView();
            SetQuestSheetView();

            bBtnBusy = false;

            txtWelcome.Text = mExaminee.ToString();

            LoadTxt();

            

            int m = -1, s = -1;
            if (mExaminee.mPhase < ExamineePhase.Submitting)
            {
                string t = null;
                if(System.IO.File.Exists("Duration.txt"))
                {
                    string[] lines = System.IO.File.ReadAllLines("Duration.txt");
                    if (lines.Length > 0)
                        t = lines[0];
                }
                if (t != null)
                {
                    string[] vt = t.Split('\t');
                    if (vt.Length == 2)
                    {
                        int.TryParse(vt[0], out m);
                        int.TryParse(vt[1], out s);
                    }
                    if (-1 < m && -1 < s)
                        dtRemn = mExaminee.kDtDuration = new TimeSpan(0, m, s);
                }
            }
            if (m < 0 || s < 0)
                dtRemn = mExaminee.kDtDuration;
            txtRTime.Text = "" + dtRemn.Minutes + " : " + dtRemn.Seconds;
            kLogIntvl = new TimeSpan(0, 0, 30);

            System.Text.StringBuilder msg = new System.Text.StringBuilder();
            msg.Append(mExaminee.tId + " (" + mExaminee.tName + ")");
            if (mExaminee.kDtDuration.Minutes == 30)
                msg.Append(Txt.s._[(int)TxI.EXAMING_MSG_1]);
            else
                msg.AppendFormat(Txt.s._[(int)TxI.EXAMING_MSG_2],
                    mExaminee.kDtDuration.Minutes, mExaminee.kDtDuration.Seconds);
            PopupMgr.Singleton.CbOK = ShowQuestion;
            AppView.Opacity = 0.5;
            PopupMgr.Singleton.ShowDialog(msg.ToString());
            AppView.Opacity = 1;
            if (mExaminee.mPhase < ExamineePhase.Examing)
                mExaminee.mPhase = ExamineePhase.Examing;
            else if (mExaminee.mPhase == ExamineePhase.Submitting)
                Submit();
        }

        void ShowQuestion()
        {
            return;
            PopupMgr.Singleton.CbOK = null;
            AppView.Effect = null;
            bBtnBusy = false;
            QuestSheetView.Visibility = Visibility.Visible;

            mTimer = new System.Timers.Timer(1000);
            mTimer.Elapsed += UpdateSrvrMsg;
            mTimer.AutoReset = true;
            mTimer.Enabled = true;
            dtLastLog = kDtStart = DateTime.Now;
        }

        void SetAnswerTableHeader()
        {
            AnswerTable.RowDefinitions.Add(new RowDefinition());
            Label cell = new Label();
            Grid.SetRow(cell, 0);
            Grid.SetColumn(cell, 0);
            AnswerTable.Children.Add(cell);
            SolidColorBrush black = new SolidColorBrush(Colors.Black);
            for (int i = 1; i <= MultiChoiceItem.N_OPTIONS; ++i)
            {
                cell = new Label();
                cell.Content = (char)('@' + i);
                cell.BorderBrush = black;
                cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.MiddleTop];
                cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                cell.FontWeight = FontWeights.Bold;
                Grid.SetRow(cell, 0);
                Grid.SetColumn(cell, i);
                AnswerTable.Children.Add(cell);
            }
            cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.RightTop];
        }

        void SetAnswerTableBottomRow()
        {
            SolidColorBrush black = new SolidColorBrush(Colors.Black);
            //bottom lines
            AnswerTable.RowDefinitions.Add(new RowDefinition());
            Label cell = new Label();
            int lastRowIdx = mQuestSheet.Questions.Count;
            cell.Content = lastRowIdx;
            cell.BorderBrush = black;
            cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.LeftBottom];
            cell.HorizontalContentAlignment = HorizontalAlignment.Center;
            cell.FontWeight = FontWeights.Bold;
            Grid.SetRow(cell, lastRowIdx);
            Grid.SetColumn(cell, 0);
            AnswerTable.Children.Add(cell);
            for (int i = 1; i <= MultiChoiceItem.N_OPTIONS; ++i)
            {
                cell = new Label();cell.Content = "x";// mExaminee.mAnsSheet.vAnsItem[lastRowIdx - 1][i - 1].lbl;
                cell.BorderBrush = black;
                cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.MiddleBottom];
                cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(cell, lastRowIdx);
                Grid.SetColumn(cell, i);
                AnswerTable.Children.Add(cell);
            }
            cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.RightBottom];
        }

        void SetAnswerTableMiddleRows()
        {
            SolidColorBrush black = new SolidColorBrush(Colors.Black);
            Label cell = new Label();
            for (int j = 1, n = mQuestSheet.Questions.Count; j < n; ++j)
            {
                AnswerTable.RowDefinitions.Add(new RowDefinition());
                cell = new Label();
                cell.Content = j;
                cell.BorderBrush = black;
                cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.MiddleTop];
                cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                cell.FontWeight = FontWeights.Bold;
                Grid.SetRow(cell, j);
                Grid.SetColumn(cell, 0);
                AnswerTable.Children.Add(cell);
                for (int i = 1; i <= MultiChoiceItem.N_OPTIONS; ++i)
                {
                    cell = mExaminee.mAnsSheet.vAnsItem[j - 1][i - 1].lbl;
                    cell.BorderBrush = black;
                    cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.MiddleTop];
                    cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                    cell.VerticalContentAlignment = VerticalAlignment.Top;
                    Grid.SetRow(cell, j);
                    Grid.SetColumn(cell, i);
                    AnswerTable.Children.Add(cell);
                }
                cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.RightTop];
            }


            //for (j = Question.svQuest[0].Count; -1 < j; --j)
            //    AnswerTable.RowDefinitions[j].Height = new GridLength(32, GridUnitType.Pixel);
        }

        void SetAnswerSheetView()
        {
            AnswerSheetBG.Background = Theme.Singleton.DefinedColors[(int)BrushId.AnswerSheet_BG];
            
            AnswerTable.Background = Theme.Singleton.DefinedColors[(int)BrushId.Sheet_BG];
            
            //AnswerSheetCellView.SInit(Window.GetWindow(this).FontSize);
            //mExaminee.mAnsSheet.Init(mQuestSheet.LvId);
            //mExaminee.mAnsSheet.InitView(mQuestSheet, qaWh, null);
            //mExaminee.mAnsSheet.bChanged = false;

            SetAnswerTableHeader();
            SetAnswerTableMiddleRows();
            SetAnswerTableBottomRow();
        }

        void SetQuestSheetView()
        {
            //QuestStackView.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            //int n = mQuestSheet.Questions.Count;
            //for (int i = 1, j = 0; i <= n; i += 2, ++j)
            //{
            //    gQuest.RowDefinitions.Add(new RowDefinition());
            //    StackPanel q = CreateQuestBox(i);
            //    Grid.SetRow(q, j);
            //    Grid.SetColumn(q, 0);
            //    gQuest.Children.Add(q);
            //}
            //for (int i = 2, j = 0; i <= n; i += 2, ++j)
            //{
            //    StackPanel q = CreateQuestBox(i);
            //    Grid.SetRow(q, j);
            //    Grid.SetColumn(q, 1);
            //    gQuest.Children.Add(q);
            //}
            //gQuest.Background = Theme.Singleton.DefinedColors[(int)BrushId.BG];
        }

        Label CreateIndexInsideQuestBox(int idx)
        {
            Label idxBox = new Label();
            idxBox.HorizontalAlignment = HorizontalAlignment.Left;
            idxBox.VerticalAlignment = VerticalAlignment.Top;
            idxBox.Content = idx;
            idxBox.Background = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            idxBox.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_Color];
            idxBox.Width = qiWh;
            idxBox.Height = qiWh;
            idxBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            idxBox.VerticalContentAlignment = VerticalAlignment.Center;
            idxBox.Padding = new Thickness(0);
            return idxBox;
        }

        Label CreateStmtInsideQuestBox(MultiChoiceItem question)
        {
            TextBlock stmt = new TextBlock();
            stmt.Text = "xx";// question.Stem;
            stmt.TextWrapping = TextWrapping.Wrap;
            stmt.Width = qaWh;
            stmt.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            Label stmtBox = new Label();
            stmtBox.Content = stmt;
            stmtBox.BorderBrush = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            stmtBox.BorderThickness = new Thickness(0, 4, 0, 0);
            Thickness zero = new Thickness(0);
            stmtBox.Margin = stmtBox.Padding = zero;

            return stmtBox;
        }

        StackPanel CreateQuestBox(int idx)
        {
            StackPanel questBox = new StackPanel();
            questBox.Orientation = Orientation.Horizontal;
            questBox.Margin = qMrg;
            questBox.Background = Theme.Singleton.DefinedColors[(int)BrushId.BG];
            
            questBox.Children.Add(CreateIndexInsideQuestBox(idx));

            StackPanel questBoxInside = new StackPanel();

            MultiChoiceItem question = mQuestSheet.Q(idx - 1);

            questBoxInside.Children.Add(CreateStmtInsideQuestBox(question));

            questBoxInside.Children.Add(mExaminee.mAnsSheet.vlbxAns[idx-1]);

            questBox.Children.Add(questBoxInside);

            return questBox;
        }

        public void Submit()
        {
            bBtnBusy = true;//
            AppView.Effect = null;
            PopupMgr.Singleton.CbOK = null;
            bRunning = false;
            DisableAll();
            mState = NetPhase.Submiting;
            mExaminee.mPhase = ExamineePhase.Submitting;
            mExaminee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
            if (mClnt.ConnectWR(ref mCbMsg))
                bBtnBusy = false;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            PopupMgr.Singleton.CbOK = Submit;
            AppView.Opacity = 0.5;
            PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.SUBMIT_CAUT],
                Txt.s._[(int)TxI.SUBMIT], Txt.s._[(int)TxI.BTN_CNCL], null);
            AppView.Opacity = 1;
        }

        public bool ClntBufHndl(byte[] buf)
        {
            int offs = 0;
            switch (mState)
            {
                case NetPhase.Submiting:
                    int rs;
                    string msg = null;
                    int l = buf.Length - offs;
                    if(l < 4)
                    {
                        rs = -1;
                        msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                    }
                    else
                        rs = BitConverter.ToInt32(buf, offs);
                    l -= 4;
                    offs += 4;
                    if(rs == 0)
                    {
                        ExamineeC e = new ExamineeC();
                        if (!e.ReadByte(buf, ref offs))
                        {
                            mExaminee.Merge(e);
                            btnSubmit.Content = mExaminee.Grade;
                            msg = Txt.s._[(int)TxI.RESULT] + mExaminee.Grade;
                        }
                        else
                            msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                    }
                    else if (rs == (int)TxI.NEEID_NF)
                        msg = Txt.s._[(int)TxI.NEEID_NF];
                    else if (rs == (int)TxI.RECV_DAT_ER)
                        msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                    else if(msg == null)
                    {
                        if(l < 4)
                            msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                        else
                        {
                            int sz = BitConverter.ToInt32(buf, offs);
                            l -= 4;
                            offs += 4;
                            if(l < sz)
                                msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                            else
                                msg = System.Text.Encoding.UTF8.GetString(buf, offs, sz);
                        }
                    }
                    Dispatcher.Invoke(() => {
                        AppView.Opacity = 0.5;
                        PopupMgr.Singleton.ShowDialog(msg);
                        AppView.Opacity = 1;
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
                case NetPhase.Submiting:
                    mExaminee.ToByte(out outBuf, (int)mState);
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
                    dtRemn = mExaminee.kDtDuration - (DateTime.Now - kDtStart);
                    if (mExaminee.mAnsSheet.bChanged && kLogIntvl < DateTime.Now - dtLastLog)
                    {
                        dtLastLog = DateTime.Now;
                        mExaminee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
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
                        AppView.Opacity = 0.5;
                        PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.TIMEOUT]);
                        AppView.Opacity = 1;
                        Submit();
                    });
                }
            }
        }

        private void DisableAll()
        {
            btnSubmit.IsEnabled = false;
            mTimer.Stop();
            foreach (ListBox l in mExaminee.mAnsSheet.vlbxAns)
                l.IsEnabled = false;
            btnExit.IsEnabled = true;
        }

        void Exit()
        {
            return;
            //WPopup.s.wpCb = null;
            //bBtnBusy = false;
            if (mExaminee.mAnsSheet.bChanged)
                mExaminee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
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
            PopupMgr.Singleton.CbOK = Exit;
            AppView.Opacity = 0.5;
            if (mExaminee.mPhase < ExamineePhase.Submitting)
                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.EXIT_CAUT_1],
                    Txt.s._[(int)TxI.EXIT], Txt.s._[(int)TxI.BTN_CNCL], "exit");
            else
                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.EXIT_CAUT_2],
                    Txt.s._[(int)TxI.EXIT], Txt.s._[(int)TxI.BTN_CNCL], null);
            AppView.Opacity = 1;
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            mClnt.Close();
            PopupMgr.Singleton.Exit();
        }
    }
}
