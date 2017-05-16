using System;
//using System.Threading.Tasks;
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
        BlurEffect mBlurEff;
        UICbMsg mCbMsg;
        System.Timers.Timer mTimer;

        public QuestSheet mQSh;

        Client2 mClnt;
        NetCode mState;

        public static double qaWh;
        double qiWh;
        Thickness qMrg;
        public Examinee mNee;//reference to Auth.mNee

        public TakeExam()
        {
            InitializeComponent();
            mState = NetCode.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;

            ShowsNavigationUI = false;

            mQSh = new QuestSheet();
        }

        private void LoadTxt()
        {
            txtAnsSh.Text = Txt.s._[(int)TxI.ANS_SHEET];
            btnSubmit.Content = Txt.s._[(int)TxI.SUBMIT];
            btnExit.Content = Txt.s._[(int)TxI.EXIT];
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
            qiWh = 3 * mrg;
            qMrg = new Thickness(mrg, mrg, 0, mrg);
            qaWh = (svwrQSh.Width - SystemParameters.ScrollWidth) / 2 - mrg - mrg - qiWh;

            InitLeftPanel();
            InitQuestPanel();

            mBlurEff = new BlurEffect();
            bBtnBusy = false;

            txtWelcome.Text = mNee.ToString();

            LoadTxt();

            double rt = spMain.RenderSize.Width / 1280;
            spMain.RenderTransform = new ScaleTransform(rt, rt);

            WPopup.nwIns(w);
            WPopup.s.wpCb2 = Deblur;

            int m = -1, s = -1;
            if (mNee.eStt < Examinee.eSUBMITTING)
            {
                string t = Utils.ReadFile("Duration.txt");
                if (t != null)
                {
                    string[] vt = t.Split('\t');
                    if (vt.Length == 2)
                    {
                        int.TryParse(vt[0], out m);
                        int.TryParse(vt[1], out s);
                    }
                    if (-1 < m && -1 < s)
                        dtRemn = mNee.kDtDuration = new TimeSpan(0, m, s);
                }
            }
            if (m < 0 || s < 0)
                dtRemn = mNee.kDtDuration;
            txtRTime.Text = "" + dtRemn.Minutes + " : " + dtRemn.Seconds;
            kLogIntvl = new TimeSpan(0, 0, 30);

            System.Text.StringBuilder msg = new System.Text.StringBuilder();
            msg.Append(mNee.tId + " (" + mNee.tName + ")");
            if (mNee.kDtDuration.Minutes == 30)
                msg.Append(Txt.s._[(int)TxI.EXAMING_MSG_1]);
            else
                msg.AppendFormat(Txt.s._[(int)TxI.EXAMING_MSG_2],
                    mNee.kDtDuration.Minutes, mNee.kDtDuration.Seconds);
            WPopup.s.wpCb = ShowQuestion;
            spMain.Effect = mBlurEff;
            WPopup.s.ShowDialog(msg.ToString());
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
            WPopup.s.wpCb = null;
            if (mNee.eStt < Examinee.eEXAMING)
                mNee.eStt = Examinee.eEXAMING;
            else if (mNee.eStt == Examinee.eSUBMITTING)
                Submit();
        }

        void InitLeftPanel()
        {
            //left panel
            spLp.HorizontalAlignment = HorizontalAlignment.Left;
            spLp.Background = Theme.s._[(int)BrushId.LeftPanel_BG];
            //title
            Label l = new Label();
            gAnsSh.Background = Theme.s._[(int)BrushId.Sheet_BG];
            int nAns = 4;//hardcode
            int i = 0, n = mQSh.vQuest.Count;
            AnsItem.SInit(Window.GetWindow(this).FontSize);
            mNee.mAnsSh.Init(mQSh, mNee.uId);
            mNee.mAnsSh.InitView(mQSh, qaWh, null);
            mNee.mAnsSh.bChanged = false;
            //top line
            gAnsSh.RowDefinitions.Add(new RowDefinition());
            l = new Label();
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, 0);
            gAnsSh.Children.Add(l);
            SolidColorBrush brBK = new SolidColorBrush(Colors.Black);
            for (i = 1; i < nAns; ++i)
            {
                l = new Label();
                l.Content = (char)('@' + i);
                l.BorderBrush = brBK;
                l.BorderThickness = Theme.s.l[(int)ThicknessId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.FontWeight = FontWeights.Bold;
                Grid.SetRow(l, 0);
                Grid.SetColumn(l, i);
                gAnsSh.Children.Add(l);

            }
            l = new Label();
            l.BorderBrush = brBK;
            l.BorderThickness = Theme.s.l[(int)ThicknessId.RT];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.Content = (char)('@' + i);
            l.FontWeight = FontWeights.Bold;
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, i);
            gAnsSh.Children.Add(l);
            //next lines
            //n -= 1;
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
                for (i = 1; i < nAns; ++i)
                {
                    l = mNee.mAnsSh.vAnsItem[j - 1][i - 1].lbl;
                    l.BorderBrush = brBK;
                    l.BorderThickness = Theme.s.l[(int)ThicknessId.MT];
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;
                    l.VerticalContentAlignment = VerticalAlignment.Top;
                    Grid.SetRow(l, j);
                    Grid.SetColumn(l, i);
                    gAnsSh.Children.Add(l);
                }
                l = l = mNee.mAnsSh.vAnsItem[j - 1][i - 1].lbl;
                l.BorderBrush = brBK;
                l.BorderThickness = Theme.s.l[(int)ThicknessId.RT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, i);
                gAnsSh.Children.Add(l);
            }
            //bottom lines
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
            for (i = 1; i < nAns; ++i)
            {
                l = mNee.mAnsSh.vAnsItem[j - 1][i - 1].lbl;
                l.BorderBrush = brBK;
                l.BorderThickness = Theme.s.l[(int)ThicknessId.MB];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, i);
                gAnsSh.Children.Add(l);
            }
            l = mNee.mAnsSh.vAnsItem[j - 1][i - 1].lbl;
            l.BorderBrush = brBK;
            l.BorderThickness = Theme.s.l[(int)ThicknessId.RB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            Grid.SetRow(l, j);
            Grid.SetColumn(l, i);
            gAnsSh.Children.Add(l);

            //for (j = Question.svQuest[0].Count; -1 < j; --j)
            //    gAnsSh.RowDefinitions[j].Height = new GridLength(32, GridUnitType.Pixel);
        }

        void InitQuestPanel()
        {
            gQuest.Background = Theme.s._[(int)BrushId.Q_BG];
            int n = mQSh.vQuest.Count;
            for (int i = 1, j = 0; i <= n; i += 2, ++j)
            {
                gQuest.RowDefinitions.Add(new RowDefinition());
                StackPanel q = CreateQuestion(i);
                Grid.SetRow(q, j);
                Grid.SetColumn(q, 0);
                gQuest.Children.Add(q);
            }
            for (int i = 2, j = 0; i <= n; i += 2, ++j)
            {
                StackPanel q = CreateQuestion(i);
                Grid.SetRow(q, j);
                Grid.SetColumn(q, 1);
                gQuest.Children.Add(q);
            }
            gQuest.Background = Theme.s._[(int)BrushId.BG];
        }

        StackPanel CreateQuestion(int idx)
        {
            StackPanel q = new StackPanel();
            q.Orientation = Orientation.Horizontal;
            q.Margin = qMrg;
            Label l = new Label();
            l.HorizontalAlignment = HorizontalAlignment.Left;
            l.VerticalAlignment = VerticalAlignment.Top;
            l.Content = idx;
            l.Background = Theme.s._[(int)BrushId.QID_BG];
            l.Foreground = Theme.s._[(int)BrushId.QID_Color];
            l.Width = qiWh;
            l.Height = qiWh;
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.VerticalContentAlignment = VerticalAlignment.Center;
            l.Padding = new Thickness(0);
            q.Children.Add(l);
            StackPanel con = new StackPanel();
            TextBlock stmt = new TextBlock();
            Question quest = mQSh.vQuest[idx - 1];
            stmt.Text = quest.mStmt;
            stmt.TextWrapping = TextWrapping.Wrap;
            stmt.Width = qaWh;
            stmt.Background = Theme.s._[(int)BrushId.Q_BG];
            Label stmtCon = new Label();
            stmtCon.Content = stmt;
            stmtCon.BorderBrush = Theme.s._[(int)BrushId.QID_BG];
            stmtCon.BorderThickness = new Thickness(0, 4, 0, 0);
            Thickness zero = new Thickness(0);
            stmtCon.Margin = stmtCon.Padding = zero;
            con.Children.Add(stmtCon);
            con.Children.Add(mNee.mAnsSh.vlbxAns[idx-1]);
            q.Children.Add(con);
            q.Background = Theme.s._[(int)BrushId.BG];
            return q;
        }

        public void Submit()
        {
            bBtnBusy = true;//
            spMain.Effect = null;
            WPopup.s.wpCb = null;
            bRunning = false;
            DisableAll();
            mState = NetCode.Submiting;
            mNee.eStt = Examinee.eSUBMITTING;
            mNee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
            mClnt.ConnectWR(ref mCbMsg);
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            WPopup.s.wpCb = Submit;
            spMain.Effect = mBlurEff;
            WPopup.s.ShowDialog(Txt.s._[(int)TxI.SUBMIT_CAUT],
                Txt.s._[(int)TxI.SUBMIT], Txt.s._[(int)TxI.BTN_CNCL], null);
        }

        public bool ClntBufHndl(byte[] buf, int offs)
        {
            switch (mState)
            {
                case NetCode.Submiting:
                    ushort grade = BitConverter.ToUInt16(buf, offs);
                    btnSubmit.Content = grade;
                    WPopup.s.wpCb = Deblur;
                    spMain.Effect = mBlurEff;
                    WPopup.s.ShowDialog(Txt.s._[(int)TxI.RESULT] + grade);
                    break;
            }
            bBtnBusy = false;
            return false;
        }

        public bool ClntBufPrep(ref byte[] outBuf)
        {
            switch (mState)
            {
                case NetCode.Submiting:
                    int sz = 4 + mNee.mAnsSh.GetByteCount();
                    int offs = 0;
                    outBuf = new byte[sz];
                    Buffer.BlockCopy(BitConverter.GetBytes((int)mState),
                        0, outBuf, offs, 4);
                    offs += 4;
                    mNee.mAnsSh.ToByte(ref outBuf, ref offs);
                    break;
                case NetCode.Resubmit:
                    break;
            }
            return true;
        }

        private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning)
            {
                if (0 < dtRemn.Ticks)
                {
                    dtRemn = mNee.kDtDuration - (DateTime.Now - kDtStart);
                    if (mNee.mAnsSh.bChanged && kLogIntvl < DateTime.Now - dtLastLog)
                    {
                        dtLastLog = DateTime.Now;
                        mNee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = dtRemn.Minutes.ToString() + " : " + dtRemn.Seconds;
                        if (!btnSubmit.IsEnabled && dtRemn.Minutes < 28)//hardcode
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
                        WPopup.s.wpCb = Deblur;
                        spMain.Effect = mBlurEff;
                        WPopup.s.ShowDialog(Txt.s._[(int)TxI.TIMEOUT]);
                        Submit();
                    });
                }
            }
        }

        private void DisableAll()
        {
            btnSubmit.IsEnabled = false;
            mTimer.Stop();
            foreach (ListBox l in mNee.mAnsSh.vlbxAns)
                l.IsEnabled = false;
            btnExit.IsEnabled = true;
        }

        void Exit()
        {
            //WPopup.s.wpCb = null;
            //bBtnBusy = false;
            if (mNee.mAnsSh.bChanged)
                mNee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
            Window.GetWindow(this).Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            WPopup.s.wpCb = Exit;
            spMain.Effect = mBlurEff;
            if (btnSubmit.IsEnabled)
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.EXIT_CAUT_1],
                    Txt.s._[(int)TxI.EXIT], Txt.s._[(int)TxI.BTN_CNCL], "exit");
            else
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.EXIT_CAUT_2],
                    Txt.s._[(int)TxI.EXIT], Txt.s._[(int)TxI.BTN_CNCL], null);
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            WPopup.s.cncl = false;
            mClnt.Close();
        }

        private void Deblur()
        {
            WPopup.s.wpCb = null;
            spMain.Effect = null;
            bBtnBusy = false;
        }
    }
}
