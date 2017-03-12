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
        public static double em = 8;//16 * 1.2;
        Label[][] vlblAnsSh;
        bool[][] vbAns;
        double[] vWidth;
        byte[] mbAns;

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
            vFontFml = new FontFamily[2];
            vFontFml[0] = new FontFamily("Arial");
            vFontFml[1] = new FontFamily("Arial");
            mState = NetCode.Dating;
            mClient = new Client2(CliBufHndl, CliBufPrep);
            mCbMsg = new UICbMsg();

            txtWelcome.Text = Examinee.sAuthNee.ToString();

            ShowsNavigationUI = false;

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        public static SolidColorBrush[] vBrush;
        public static SolidColorBrush[][] vTheme;
        Thickness[] vThickness;
        FontFamily[] vFontFml;

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
            vWidth = new double[5];
            vWidth[0] = 1280;// spMain.RenderSize.Width;
            vWidth[1] = 20;// 2 * em;
            vWidth[2] = 5 * vWidth[1];
            vWidth[3] = 5;// 8;
            vWidth[4] = (vWidth[0] - vWidth[2]) / 2 - SystemParameters.ScrollWidth - 2 * vWidth[3] - vWidth[1];
            //spMain.ColumnDefinitions.Add(new ColumnDefinition());
            //spMain.ColumnDefinitions.Add(new ColumnDefinition());
            InitLeftPanel();
            InitQuestPanel();
            dmsg.Background = vBrush[(int)BrushId.LeftPanel_BG];
            dmsg.Width = (int)spMain.RenderSize.Width / 2;
            dmsg.Height = (int)spMain.RenderSize.Height / 4;
            // spMain.Children.Add(dmsg);

            double rt = spMain.RenderSize.Width / 640; //d:DesignWidth
            double scaleH = spMain.RenderSize.Height / 360; //d:DesignHeight
            ScaleTransform st = new ScaleTransform(rt, rt);
            //svwrQSh.Width = svwrQSh.Width * rt;
            //svwrQSh.Height = svwrQSh.Height * rt;
            //Grid g = (Grid)svwrQSh.Content;
            //g.RenderTransform = st;
            //svwrQSh.RenderTransform = st;
            //spLp.RenderTransform = st;
            spMain.RenderTransform = st;
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
            int i = 0, n = Question.svQuest.Count;
            vlblAnsSh = new Label[n][];
            vbAns = new bool[n][];
            //top line
            gAnsSh.RowDefinitions.Add(new RowDefinition());
            l = new Label();
            l.Height = vWidth[1];
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
            //l.FontFamily = vFontFml[1];
            l.FontWeight = FontWeights.Bold;
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, i);
            gAnsSh.Children.Add(l);
            //next lines
            int j = 0;
            for (j = 1, i = 0; j < Question.svQuest.Count; ++j)
            {
                gAnsSh.RowDefinitions.Add(new RowDefinition());
                vlblAnsSh[j - 1] = new Label[nAns];
                vbAns[j - 1] = new bool[nAns];
                l = new Label();
                l.Content = j;
                l.BorderBrush = brBK;
                l.BorderThickness = vThickness[(int)ThicknessId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.VerticalContentAlignment = VerticalAlignment.Top;
                l.FontFamily = vFontFml[1];
                l.FontWeight = FontWeights.Bold;
                l.Height = vWidth[1];
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
            l.FontFamily = vFontFml[1];
            l.FontWeight = FontWeights.Bold;
            l.Height = vWidth[1];
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

            for (j = 0; j <= Question.svQuest.Count; ++j)
                gAnsSh.RowDefinitions[j].Height = new GridLength(26, GridUnitType.Pixel);
        }

        void InitQuestPanel()
        {
            Grid qs = new Grid();
            qs.Background = vBrush[(int)BrushId.Q_BG];
            qs.ColumnDefinitions.Add(new ColumnDefinition());
            qs.ColumnDefinitions.Add(new ColumnDefinition());
            int nc = Question.svQuest.Count / 2;
            for (int i = 0; i < nc; ++i)
            {
                qs.RowDefinitions.Add(new RowDefinition());
                StackPanel q = CreateQuestion(2 * i + 1);
                Grid.SetRow(q, i);
                Grid.SetColumn(q, 0);
                qs.Children.Add(q);
                q = CreateQuestion(2 * i + 2);
                Grid.SetRow(q, i);
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
            l.FontSize = em;
            l.FontFamily = vFontFml[0];
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
            stmt.FontSize = em;
            Question quest = Question.svQuest[idx - 1];
            stmt.Text = quest.mStmt;
            stmt.TextWrapping = TextWrapping.Wrap;
            // dmsg.Content += "_" + idx + stmt.Text + vQuest.Count + "\n";
            stmt.Width = vWidth[4];
            stmt.Background = vBrush[(int)BrushId.Q_BG];
            Label stmtCon = new Label();
            stmtCon.Content = stmt;
            stmtCon.BorderBrush = vBrush[(int)BrushId.QID_BG];
            stmtCon.BorderThickness = new Thickness(0, 4, 0, 0);
            Thickness zero = new Thickness(0);
            stmtCon.Margin = stmtCon.Padding = zero;
            con.Children.Add(stmtCon);
            ListBox answers = new ListBox();
            answers.Width = vWidth[4];
            answers.Name = "_" + idx;
            answers.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            answers.SelectionChanged += Ans_SelectionChanged;
            for (int i = 0; i < quest.vAns.Length; ++i)
            {
                ListBoxItem ans = new ListBoxItem();
                TextBlock ansTxt = new TextBlock();
                ansTxt.FontSize = em;
                ansTxt.Text = quest.vAns[i];
                ansTxt.TextWrapping = TextWrapping.Wrap;
                ansTxt.Width = vWidth[4] - SystemParameters.ScrollWidth;//minus is a trick
                ans.Content = ansTxt;
                ans.Name = "_" + i;
                answers.Items.Add(ans);
            }
            answers.BorderBrush = vBrush[(int)BrushId.Ans_TopLine];
            answers.BorderThickness = new Thickness(0, 4, 0, 0);
            //answers.Margin = answers.Padding = zero;
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
                }
                else
                {
                    vlblAnsSh[qid - 1][i].Content = string.Empty;
                    vbAns[qid - 1][i] = false;
                }
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            int n = vbAns.Length * 4, i = 0, k = 0; //hardcode
            mbAns = new byte[n];
            for (i = 0, k = 0, n = Question.svQuest.Count; i < n; ++i)
                for (int j = 0; j < 4; ++j, ++k)//hardcode
                    mbAns[k] = Convert.ToByte(vbAns[i][j]);
            mState = NetCode.Submiting;
            mClient.ConnectWR(ref mCbMsg);
        }

        public bool CliBufHndl(byte[] buf, int offs)
        {
            switch (mState)
            {
                case NetCode.Submiting:
                    int mark = BitConverter.ToInt32(buf, offs);
                    btnSubmit.Content = mark;
                    return false;
            }
            return true;
        }

        public bool CliBufPrep(ref byte[] outBuf)
        {
            switch (mState)
            {
                case NetCode.Submiting:
                    byte[] x = BitConverter.GetBytes((int)mState);
                    int sz = x.Length + mbAns.Length;
                    outBuf = new byte[sz];
                    Buffer.BlockCopy(x, 0, outBuf, 0, x.Length);
                    Buffer.BlockCopy(mbAns, 0, outBuf, x.Length, mbAns.Length);
                    break;
                case NetCode.Resubmit:
                    break;
            }
            return true;
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (mCbMsg.ToUp())
                Dispatcher.Invoke(() => {
                    Console.WriteLine("txtabc" + mCbMsg.txt);
                });
        }
    }
}
