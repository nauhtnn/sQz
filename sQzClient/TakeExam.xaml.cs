using System;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Navigation;
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
        TimeSpan kDtDuration;
        bool bRunning;
        UICbMsg mCbMsg;
        System.Timers.Timer mTimer;

        public QuestSheet mQSh;

        Client2 mClnt;
        NetCode mState;

        public static double qaWh;
        double qiWh;
        Thickness qMrg;

        AnsSheet mAnsSh;
        public Examinee mNee;//reference to Auth.mNee

        public TakeExam()
        {
            InitializeComponent();
            mState = NetCode.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;

            //txtWelcome.Text = Examinee.sAuthNee.ToString();

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

            LoadTxt();

            //double rt = spMain.RenderSize.Width / 1280;
            //spMain.RenderTransform = new ScaleTransform(rt, rt);

            WPopup.nwIns(w);

            string msg = mNee.tId + " (" + mNee.tName +
                ")" + Txt.s._[(int)TxI.AUTH_MSG];
            //Dispatcher.Invoke(() => {
                WPopup.s.wpCb = ShowQuestion;
                WPopup.s.ShowDialog(msg);
            //});
        }

        void ShowQuestion()
        {
            Dispatcher.Invoke(()=> { svwrQSh.Visibility = Visibility.Visible; });

            mTimer = new System.Timers.Timer(1000);
            mTimer.Elapsed += UpdateSrvrMsg;
            mTimer.AutoReset = true;
            mTimer.Enabled = true;
            kDtStart = DateTime.Now;
            string t = Utils.ReadFile("Duration.txt");
            int m = -1, s = -1;
            if (t != null)
            {
                string[] vt = t.Split('\t');
                if (vt.Length == 2)
                {
                    int.TryParse(vt[0], out m);
                    int.TryParse(vt[1], out s);
                }
                if (-1 < m && -1 < s)
                    dtRemn = kDtDuration = new TimeSpan(0, m, s);
            }
            if (m < 0 || s < 0)
                dtRemn = kDtDuration = new TimeSpan(0, 30, 0);
            WPopup.s.wpCb = null;
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
            mAnsSh = new AnsSheet();
            mAnsSh.Init(mQSh, mNee.uId);
            mAnsSh.InitView(mQSh, qaWh);
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
            n -= 2;
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
                    l = mAnsSh.vAnsItem[j - 1][i - 1].lbl;
                    l.BorderBrush = brBK;
                    l.BorderThickness = Theme.s.l[(int)ThicknessId.MT];
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;
                    l.VerticalContentAlignment = VerticalAlignment.Top;
                    Grid.SetRow(l, j);
                    Grid.SetColumn(l, i);
                    gAnsSh.Children.Add(l);
                }
                l = new Label();
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
                l = mAnsSh.vAnsItem[j - 1][i - 1].lbl;
                l.BorderBrush = brBK;
                l.BorderThickness = Theme.s.l[(int)ThicknessId.MB];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, i);
                gAnsSh.Children.Add(l);
            }
            l = new Label();
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
            Grid qs = new Grid();
            qs.Background = Theme.s._[(int)BrushId.Q_BG];
            qs.ColumnDefinitions.Add(new ColumnDefinition());
            qs.ColumnDefinitions.Add(new ColumnDefinition());
            int n = mQSh.vQuest.Count;
            for (int i = 1, j = 0; i <= n; i += 2, ++j)
            {
                qs.RowDefinitions.Add(new RowDefinition());
                StackPanel q = CreateQuestion(i);
                Grid.SetRow(q, j);
                Grid.SetColumn(q, 0);
                qs.Children.Add(q);
            }
            for (int i = 2, j = 0; i <= n; i += 2, ++j)
            {
                StackPanel q = CreateQuestion(i);
                Grid.SetRow(q, j);
                Grid.SetColumn(q, 1);
                qs.Children.Add(q);
            }
            qs.Background = Theme.s._[(int)BrushId.BG];
            svwrQSh.Content = qs;
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
            con.Children.Add(mAnsSh.vlbxAns[idx-1]);
            q.Children.Add(con);
            q.Background = Theme.s._[(int)BrushId.BG];
            return q;
        }

        public void Submit()
        {
            mState = NetCode.Submiting;
            mClnt.ConnectWR(ref mCbMsg);
            DisableAll();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            WPopup.s.wpCb = Submit;
            WPopup.s.ShowDialog(Txt.s._[(int)TxI.SUBMIT_CAUT],
                Txt.s._[(int)TxI.SUBMIT], Txt.s._[(int)TxI.BTN_CNCL], null);
        }

        public bool ClntBufHndl(byte[] buf, int offs)
        {
            switch (mState)
            {
                case NetCode.Submiting:
                    ushort mark = BitConverter.ToUInt16(buf, offs);
                    //txtRs.Text = Txt.s._[(int)TxI.RESULT] + mark;
                    WPopup.s.ShowDialog(Txt.s._[(int)TxI.RESULT] + mark);
                    return false;
            }
            return true;
        }

        public bool ClntBufPrep(ref byte[] outBuf)
        {
            switch (mState)
            {
                case NetCode.Submiting:
                    int sz = 4 + mAnsSh.GetByteCount();
                    int offs = 0;
                    outBuf = new byte[sz];
                    Buffer.BlockCopy(BitConverter.GetBytes((int)mState),
                        0, outBuf, offs, 4);
                    offs += 4;
                    mAnsSh.ToByte(ref outBuf, ref offs);
                    break;
                case NetCode.Resubmit:
                    break;
            }
            return true;
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning)
                Dispatcher.Invoke(() =>
                {
                    if (0 < dtRemn.Ticks)
                    {
                        dtRemn = kDtDuration - (DateTime.Now - kDtStart);
                        txtRTime.Text = "" + dtRemn.Minutes + " : " + dtRemn.Seconds;
                    }
                    else
                    {
                        txtRTime.Text = "0:0";
                        btnSubmit_Click(null, null);
                        System.Threading.Thread th = new System.Threading.Thread(() => {
                            Dispatcher.Invoke(() => {
                                WPopup.s.ShowDialog(Txt.s._[(int)TxI.TIMEOUT]);
                            });
                        });
                        th.Start();
                    }
                });
        }

        private void DisableAll()
        {
            btnSubmit.IsEnabled = false;
            mTimer.Stop();
            btnExit.IsEnabled = true;
        }

        void Exit()
        {
            Window.GetWindow(this).Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            WPopup.s.wpCb = Exit;
            if(btnSubmit.IsEnabled)
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
    }
}
