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
    public partial class TakeExam : System.Windows.Controls.Page
    {
        public static double em = 14;
        Label[][] vlblAnsSh;
        bool[][] vbAns;
        double[] vWidth;
        byte[] mbAns;
        DateTime kDtStart;
        TimeSpan dtRemn;
        TimeSpan kDtDuration;
        List<ListBox> vLbx;
        System.Timers.Timer mTimer;
        Txt mTxt;

        Client2 mClient;
        NetCode mState;

        UICbMsg mCbMsg;

        static bool bBrushReady = false;

        Label dmsg = new Label();

        public TakeExam()
        {
            InitializeComponent();
            InitBrush();
            InitThickness();
            mState = NetCode.Dating;
            mClient = new Client2(CliBufHndl, CliBufPrep);
            mCbMsg = new UICbMsg();
            vLbx = new List<ListBox>();

            txtWelcome.Text = Examinee.sAuthNee.ToString();

            ShowsNavigationUI = false;
        }

        private void LoadTxt()
        {
            mTxt = new Txt();
            mTxt.ReadByte(Txt.sRPath + "samples/GUI-vi.bin");
            txtAnsSh.Text = mTxt._[(int)TxI.ANS_SHEET];
            btnSubmit.Content = mTxt._[(int)TxI.SUBMIT];
            btnExit.Content = mTxt._[(int)TxI.EXIT];
        }

        public static SolidColorBrush[] vBrush;
        public static SolidColorBrush[][] vTheme;
        Thickness[] vThickness;

        public static void InitBrush()
        {
            if (bBrushReady)
                return;
            bBrushReady = true;
            vTheme = new SolidColorBrush[(int)ThemeId.Count][];
            //Harvard theme
            SolidColorBrush[] br = new SolidColorBrush[(int)BrushId.Count];
            Color c = new Color();
            c.A = 0xff;
            c.R = 0xc3;
            c.G = 0xd7;
            c.B = 0xa4;
            br[(int)BrushId.BG] = new SolidColorBrush(c);
            c.R = 0xa5;
            c.G = 0x1c;
            c.B = 0x30;
            br[(int)BrushId.QID_BG] = new SolidColorBrush(c);
            c.R = c.G = c.B = 0xff;
            br[(int)BrushId.QID_Color] = new SolidColorBrush(c);
            c.R = c.G = c.B = 0xee;
            br[(int)BrushId.Q_BG] = new SolidColorBrush(c);
            c.R = 0xd8;
            c.G = 0x70;
            c.B = 0xb8;
            br[(int)BrushId.Ans_TopLine] = new SolidColorBrush(c);
            c.R = 0x58;
            c.G = 0xa9;
            c.B = 0xb4;
            br[(int)BrushId.Ans_Highlight] = new SolidColorBrush(c);
            c.R = c.G = 0xff;
            c.B = 0xbb;
            br[(int)BrushId.LeftPanel_BG] = new SolidColorBrush(c);
            c.B = 0xdd;
            br[(int)BrushId.Sheet_BG] = new SolidColorBrush(c);
            c.R = 0xf1;
            c.G = 0x5a;
            c.B = 0x23;
            br[(int)BrushId.Button_Hover] = new SolidColorBrush(c);
            vTheme[(int)ThemeId.Harvard] = br;
            vBrush = vTheme[(int)ThemeId.Harvard];
        }

        void InitThickness()
        {
            vThickness = new Thickness[(int)ThicknessId.Count];
            vThickness[(int)ThicknessId.LT] = new Thickness(0, 0, 0, 0);
            vThickness[(int)ThicknessId.MT] = new Thickness(1, 1, 0, 0);
            vThickness[(int)ThicknessId.RT] = new Thickness(1, 1, 1, 0);
            vThickness[(int)ThicknessId.LB] = new Thickness(1, 1, 0, 1);
            vThickness[(int)ThicknessId.MB] = new Thickness(1, 1, 0, 1);
            vThickness[(int)ThicknessId.RB] = new Thickness(1, 1, 1, 1);
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 16;

            Theme.InitBrush();
            vWidth = new double[5];
            vWidth[0] = 1280;// spMain.RenderSize.Width;
            vWidth[1] = 20;// 2 * em;
            vWidth[2] = 5 * vWidth[1];
            vWidth[3] = 5;// 8;
            vWidth[4] = (vWidth[0] - vWidth[2]) / 2 - SystemParameters.ScrollWidth - 2 * vWidth[3] - vWidth[1];
            
            InitLeftPanel();
            InitQuestPanel();
            dmsg.Background = vBrush[(int)BrushId.LeftPanel_BG];
            dmsg.Width = (int)spMain.RenderSize.Width / 2;
            dmsg.Height = (int)spMain.RenderSize.Height / 4;

            LoadTxt();

            double rt = spMain.RenderSize.Width / 1280;
            spMain.RenderTransform = new ScaleTransform(rt, rt);

            string msg = Examinee.sAuthNee.ID + " (" + Examinee.sAuthNee.mName +
                ") has signed in successfully. Press ok and start answering.";
            MessageBox.Show(msg);

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
                if(-1 < m && -1 < s)
                    dtRemn = kDtDuration = new TimeSpan(0, m, s);
            }
            if(m < 0 || s < 0)
                dtRemn = kDtDuration = new TimeSpan(0, 30, 2);
        }

        void InitLeftPanel()
        {
            //left panel
            spLp.HorizontalAlignment = HorizontalAlignment.Left;
            spLp.Background = vBrush[(int)BrushId.LeftPanel_BG];
            //title
            Label l = new Label();
            gAnsSh.Background = vBrush[(int)BrushId.Sheet_BG];
            int nAns = 4;//hardcode
            int i = 0, n = Question.svQuest[0].Count;
            vlblAnsSh = new Label[n][];
            vbAns = new bool[n][];
            //top line
            gAnsSh.RowDefinitions.Add(new RowDefinition());
            l = new Label();
            l.Height = 28;
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, 0);
            gAnsSh.Children.Add(l);
            SolidColorBrush brBK = new SolidColorBrush(Colors.Black);
            for (i = 1; i < nAns; ++i)
            {
                l = new Label();
                l.Content = (char)('@' + i);
                l.BorderBrush = brBK;
                l.BorderThickness = vThickness[(int)ThicknessId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.FontWeight = FontWeights.Bold;
                Grid.SetRow(l, 0);
                Grid.SetColumn(l, i);
                gAnsSh.Children.Add(l);

            }
            l = new Label();
            l.BorderBrush = brBK;
            l.BorderThickness = vThickness[(int)ThicknessId.RT];
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
                l.BorderThickness = vThickness[(int)ThicknessId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.FontWeight = FontWeights.Bold;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, 0);
                gAnsSh.Children.Add(l);
                for (i = 1; i < nAns; ++i)
                {
                    l = new Label();
                    l.BorderBrush = brBK;
                    l.BorderThickness = vThickness[(int)ThicknessId.MT];
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
                l.BorderThickness = vThickness[(int)ThicknessId.RT];
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
            l.BorderThickness = vThickness[(int)ThicknessId.LB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.FontWeight = FontWeights.Bold;
            //l.Height = vWidth[1];
            Grid.SetRow(l, j);
            Grid.SetColumn(l, 0);
            gAnsSh.Children.Add(l);
            for (i = 1; i < nAns; ++i)
            {
                l = new Label();
                l.BorderBrush = brBK;
                l.BorderThickness = vThickness[(int)ThicknessId.MB];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, i);
                gAnsSh.Children.Add(l);
                vlblAnsSh[j - 1][i - 1] = l;
                vbAns[j - 1][i - 1] = false;
            }
            l = new Label();
            l.BorderBrush = brBK;
            l.BorderThickness = vThickness[(int)ThicknessId.RB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            Grid.SetRow(l, j);
            Grid.SetColumn(l, i);
            gAnsSh.Children.Add(l);
            vlblAnsSh[j - 1][nAns - 1] = l;
            vbAns[j - 1][nAns - 1] = false;

            for (j = Question.svQuest[0].Count; -1 < j; --j)
                gAnsSh.RowDefinitions[j].Height = new GridLength(26, GridUnitType.Pixel);
        }

        void InitQuestPanel()
        {
            Grid qs = new Grid();
            qs.Background = vBrush[(int)BrushId.Q_BG];
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
            qs.Background = vBrush[(int)BrushId.BG];
            svwrQSh.Content = qs;
        }

        StackPanel CreateQuestion(int idx)
        {
            StackPanel q = new StackPanel();
            q.Orientation = Orientation.Horizontal;
            q.Margin = new Thickness(vWidth[3], vWidth[3], 0, vWidth[3]);
            Label l = new Label();
            l.HorizontalAlignment = HorizontalAlignment.Left;
            l.VerticalAlignment = VerticalAlignment.Top;
            l.Content = idx;
            l.Background = vBrush[(int)BrushId.QID_BG];
            l.Foreground = vBrush[(int)BrushId.QID_Color];
            l.Width = vWidth[1];
            l.Height = 1.5f * em;
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.VerticalContentAlignment = VerticalAlignment.Center;
            l.Padding = new Thickness(0);
            q.Children.Add(l);
            StackPanel con = new StackPanel();
            TextBlock stmt = new TextBlock();
            Question quest = Question.svQuest[0][idx - 1];
            stmt.Text = quest.mStmt;
            stmt.TextWrapping = TextWrapping.Wrap;
            // dmsg.Content += "_" + idx + stmt.Text + vQuest.Count + "\n";
            stmt.Width = 484;// vWidth[4];
            stmt.Background = vBrush[(int)BrushId.Q_BG];
            Label stmtCon = new Label();
            stmtCon.Content = stmt;
            stmtCon.BorderBrush = vBrush[(int)BrushId.QID_BG];
            stmtCon.BorderThickness = new Thickness(0, 4, 0, 0);
            Thickness zero = new Thickness(0);
            stmtCon.Margin = stmtCon.Padding = zero;
            con.Children.Add(stmtCon);
            ListBox answers = new ListBox();
            answers.Width = 484;
            answers.Name = "_" + idx;
            answers.HorizontalContentAlignment = HorizontalAlignment.Stretch;
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
            answers.BorderBrush = vBrush[(int)BrushId.Ans_TopLine];
            answers.BorderThickness = new Thickness(0, 4, 0, 0);
            vLbx.Add(answers);
            con.Children.Add(answers);
            q.Children.Add(con);
            q.Background = vBrush[(int)BrushId.BG];
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
            mClient.ConnectWR(ref mCbMsg);
            DisableAll();
        }

        public bool CliBufHndl(byte[] buf, int offs)
        {
            switch (mState)
            {
                case NetCode.Submiting:
                    ushort mark = BitConverter.ToUInt16(buf, offs);
                    txtRs.Text = "The number of correct answer: " + mark;
                    return false;
            }
            return true;
        }

        public bool CliBufPrep(ref byte[] outBuf)
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
                            MessageBox.Show("Time's up! Your answers have been submitted automatically.");
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
            mClient.Close();
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
            mB.Background = Theme.vBrush[(int)BrushId.Q_BG];
            TextBlock tb = new TextBlock();
            tb.Text = i;
            tb.Foreground = Theme.vBrush[(int)BrushId.QID_BG];
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            mB.Child = tb;
            Children.Add(mB);
            TextBlock ansTxt = new TextBlock();
            ansTxt.Text = t;
            ansTxt.TextWrapping = TextWrapping.Wrap;
            ansTxt.Width = 470;// SystemParameters.sCrollWidth;//minus is a trick
            ansTxt.VerticalAlignment = VerticalAlignment.Center;
            Children.Add(ansTxt);
        }

        public void Selected()
        {
            mB.Background = Theme.vBrush[(int)BrushId.QID_BG];
            TextBlock t = (TextBlock)mB.Child;
            t.Foreground = Theme.vBrush[(int)BrushId.QID_Color];
        }

        public void Unselected()
        {
            mB.Background = Theme.vBrush[(int)BrushId.Q_BG];
            TextBlock t = (TextBlock)mB.Child;
            t.Foreground = Theme.vBrush[(int)BrushId.QID_BG];
        }
    }
}
