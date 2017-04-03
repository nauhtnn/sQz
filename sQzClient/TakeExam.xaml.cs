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
        Label[][] vlblAnsSh;
        bool[][] vbAns;
        byte[] mbAns;
        DateTime kDtStart;
        TimeSpan dtRemn;
        TimeSpan kDtDuration;
        bool bRunning;
        UICbMsg mCbMsg;
        System.Timers.Timer mTimer;

        List<ListBox> vLbx;

        Client2 mClnt;
        NetCode mState;

        public static double qaWh;
        double qiWh;
        Thickness qMrg;

        public TakeExam()
        {
            InitializeComponent();
            mState = NetCode.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;
            vLbx = new List<ListBox>();

            txtWelcome.Text = Examinee.sAuthNee.ToString();

            ShowsNavigationUI = false;
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

            string msg = Examinee.sAuthNee.ID + " (" + Examinee.sAuthNee.mName +
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
            int i = 0, n = Question.svQuest[0].Count;
            vlblAnsSh = new Label[n][];
            vbAns = new bool[n][];
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
            int j = 0;
            for (j = 1, i = 0; j < Question.svQuest[0].Count; ++j)
            {
                gAnsSh.RowDefinitions.Add(new RowDefinition());
                vlblAnsSh[j - 1] = new Label[nAns];
                vbAns[j - 1] = new bool[nAns];
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
                    l = new Label();
                    l.BorderBrush = brBK;
                    l.BorderThickness = Theme.s.l[(int)ThicknessId.MT];
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;
                    l.VerticalContentAlignment = VerticalAlignment.Top;
                    Grid.SetRow(l, j);
                    Grid.SetColumn(l, i);
                    gAnsSh.Children.Add(l);
                    vlblAnsSh[j - 1][i - 1] = l;
                    vbAns[j - 1][i - 1] = false;
                }
                l = new Label();
                l.BorderBrush = brBK;
                l.BorderThickness = Theme.s.l[(int)ThicknessId.RT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, i);
                gAnsSh.Children.Add(l);
                vlblAnsSh[j - 1][nAns - 1] = l;
                vbAns[j - 1][nAns - 1] = false;
            }
            //bottom lines
            gAnsSh.RowDefinitions.Add(new RowDefinition());
            vlblAnsSh[j - 1] = new Label[nAns];
            vbAns[j - 1] = new bool[nAns];
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
                l = new Label();
                l.BorderBrush = brBK;
                l.BorderThickness = Theme.s.l[(int)ThicknessId.MB];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, i);
                gAnsSh.Children.Add(l);
                vlblAnsSh[j - 1][i - 1] = l;
                vbAns[j - 1][i - 1] = false;
            }
            l = new Label();
            l.BorderBrush = brBK;
            l.BorderThickness = Theme.s.l[(int)ThicknessId.RB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            Grid.SetRow(l, j);
            Grid.SetColumn(l, i);
            gAnsSh.Children.Add(l);
            vlblAnsSh[j - 1][nAns - 1] = l;
            vbAns[j - 1][nAns - 1] = false;

            //for (j = Question.svQuest[0].Count; -1 < j; --j)
            //    gAnsSh.RowDefinitions[j].Height = new GridLength(32, GridUnitType.Pixel);
        }

        void InitQuestPanel()
        {
            Grid qs = new Grid();
            qs.Background = Theme.s._[(int)BrushId.Q_BG];
            qs.ColumnDefinitions.Add(new ColumnDefinition());
            qs.ColumnDefinitions.Add(new ColumnDefinition());
            int n = Question.svQuest[0].Count;
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
            Question quest = Question.svQuest[0][idx - 1];
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
            ListBox answers = new ListBox();
            answers.Width = qaWh;
            answers.Name = "_" + idx;
            //answers.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            answers.SelectionChanged += Ans_SelectionChanged;
            for (int i = 0; i < quest.vAns.Length; ++i)
            {
                ListBoxItem ans = new ListBoxItem();
                char aa = (char)('A' + i);
                AnsItem ai = new AnsItem(quest.vAns[i], "" + aa);
                ans.Content = ai;
                ans.Name = "_" + i;
                answers.Items.Add(ans);
            }
            answers.BorderBrush = Theme.s._[(int)BrushId.Ans_TopLine];
            answers.BorderThickness = new Thickness(0, 4, 0, 0);
            vLbx.Add(answers);
            con.Children.Add(answers);
            q.Children.Add(con);
            q.Background = Theme.s._[(int)BrushId.BG];
            return q;
        }

        private void Ans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            if (l.SelectedItem == null)
                return;
            int qid = Convert.ToInt32(l.Name.Substring(1));
            for(int i = 0, n = l.Items.Count; i < n; ++i)
            {
                ListBoxItem li = (ListBoxItem)l.Items[i];
                if (li.IsSelected)
                {
                    vlblAnsSh[qid - 1][i].Content = 'X';
                    vbAns[qid - 1][i] = true;
                    AnsItem ai = (AnsItem)li.Content;
                    ai.Selected();
                }
                else
                {
                    vlblAnsSh[qid - 1][i].Content = string.Empty;
                    vbAns[qid - 1][i] = false;
                    AnsItem ai = (AnsItem)li.Content;
                    ai.Unselected();
                }
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            int n = vbAns.Length * 4, i = 0, k = 0; //hardcode
            mbAns = new byte[n];
            for (i = 0, k = 0, n = Question.svQuest[0].Count; i < n; ++i)
                for (int j = 0; j < 4; ++j, ++k)//hardcode
                    mbAns[k] = Convert.ToByte(vbAns[i][j]);
            mState = NetCode.Submiting;
            mClnt.ConnectWR(ref mCbMsg);
            DisableAll();
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
                    int sz = 14 + mbAns.Length;
                    int offs = 0;
                    outBuf = new byte[sz];
                    Buffer.BlockCopy(BitConverter.GetBytes((int)mState),
                        0, outBuf, offs, 4);
                    offs += 4;
                    Buffer.BlockCopy(BitConverter.GetBytes((int)Examinee.sAuthNee.mLvl),
                        0, outBuf, offs, 4);
                    offs += 4;
                    Buffer.BlockCopy(BitConverter.GetBytes(Examinee.sAuthNee.mId),
                        0, outBuf, offs, 2);
                    offs += 2;
                    Buffer.BlockCopy(BitConverter.GetBytes(Question.siArr),
                        0, outBuf, offs, 4);
                    offs += 4;
                    Buffer.BlockCopy(mbAns, 0, outBuf, offs, mbAns.Length);
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
            foreach(ListBox lbx in vLbx)
                lbx.IsEnabled = false;
            btnSubmit.IsEnabled = false;
            mTimer.Stop();
            btnExit.IsEnabled = true;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            WPopup.s.cncl = false;
            mClnt.Close();
        }
    }

    public class AnsItem : StackPanel
    {
        public static CornerRadius sCr = new CornerRadius();
        //public static Thickness sTh = new Thickness(4);
        Border mB;
        static bool sI = false;
        public void SInit()
        {
            if(!sI)
                sCr.BottomLeft = sCr.BottomRight = sCr.TopLeft = sCr.TopRight = 50;
        }
        
        public AnsItem(string t, string i)
        {
            SInit();
            Orientation = Orientation.Horizontal;
            mB = new Border();
            mB.Width = mB.Height = 30;
            mB.CornerRadius = sCr;
            //mB.BorderThickness = sTh;
            mB.Background = Theme.s._[(int)BrushId.Q_BG];
            TextBlock tb = new TextBlock();
            tb.Text = i;
            tb.Foreground = Theme.s._[(int)BrushId.QID_BG];
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            mB.Child = tb;
            Children.Add(mB);
            TextBlock ansTxt = new TextBlock();
            ansTxt.Text = t;
            ansTxt.TextWrapping = TextWrapping.Wrap;
            ansTxt.Width = TakeExam.qaWh - mB.Width;//hardcode
            ansTxt.VerticalAlignment = VerticalAlignment.Center;
            Children.Add(ansTxt);
        }

        public void Selected()
        {
            mB.Background = Theme.s._[(int)BrushId.QID_BG];
            TextBlock t = (TextBlock)mB.Child;
            t.Foreground = Theme.s._[(int)BrushId.QID_Color];
        }

        public void Unselected()
        {
            mB.Background = Theme.s._[(int)BrushId.Q_BG];
            TextBlock t = (TextBlock)mB.Child;
            t.Foreground = Theme.s._[(int)BrushId.QID_BG];
        }
    }
}
