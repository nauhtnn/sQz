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
using sQzLib;

namespace sQzServer0
{
    /// <summary>
    /// Interaction logic for Operation.xaml
    /// </summary>
    public partial class Operation0 : Page
    {
        Server2 mServer;
        UICbMsg mCbMsg;
        byte[] vQuestAnsKey;
        Dictionary<int, TextBlock> vMark;
        private Txt mTxt;

        public Operation0()
        {
            InitializeComponent();
            ShowsNavigationUI = false;
            mServer = new Server2(SrvrCodeHndl);
            mCbMsg = new UICbMsg();
            vMark = new Dictionary<int, TextBlock>();

            lbxDate.SelectionMode = SelectionMode.Single;
            lbxDate.SelectionChanged += lbxDate_SelectionChanged;
            Theme.InitBrush();
            LoadDates();

            vQuestAnsKey = null;
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UICbMsg dummy = new UICbMsg();
            mServer.Stop(ref dummy);
        }

        private void lbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            ListBoxItem i = (ListBoxItem)l.SelectedItem;
            if (i == null)
                return;
            Date.Select((string)i.Content);
            Examinee.DBSelect(Date.sDBIdx);
            LoadStudents();
        }

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

        private void LoadStudents() //same as Prep0.xaml
        {
            Dispatcher.Invoke(() => {
                Color c = new Color();
                c.A = 0xff;
                c.B = c.G = c.R = 0xf0;
                bool dark = false;
                int rid = 0;
                vMark.Clear();
                gNee.Children.Clear();
                foreach (Examinee st in Examinee.svExaminee)
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(20);
                    gNee.RowDefinitions.Add(rd);
                    TextBlock t = new TextBlock();
                    t.Text = st.ID;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    gNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = st.mName;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 1);
                    gNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = st.mBirthdate;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 2);
                    gNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = st.mBirthplace;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 3);
                    gNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vMark.Add((int)st.mLvl * st.mId, t);
                    Grid.SetRow(t, rid++);
                    Grid.SetColumn(t, 4);
                    gNee.Children.Add(t);
                    dark = !dark;
                }
            });
        }

        private void LoadMarks()
        {
            Dispatcher.Invoke(() => {
                TextBlock t;
                foreach (Examinee st in Examinee.svExaminee)
                {
                    if(vMark.TryGetValue((int)st.mLvl * st.mId, out t))
                        t.Text = "" + st.mMark;
                }
            });
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 13;

            spMain.Background = Theme.vBrush[(int)BrushId.Ans_Highlight];

            LoadTxt();

            LoadDates();
            InitQPanel();

            double rt = spMain.RenderSize.Width / 1280;
            spMain.RenderTransform = new ScaleTransform(rt, rt);

            FirewallHandler fwHndl = new FirewallHandler(0);
            string msg = fwHndl.OpenFirewall();
            lblStatus.Text = msg;

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void btnStartSrvr_Click(object sender, RoutedEventArgs e)
        {
            Thread th = new Thread(() => {mServer.Start(ref mCbMsg);});
            th.Start();
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (mCbMsg.ToUp())
                Dispatcher.Invoke(() => {
                    lblStatus.Text += mCbMsg.txt; });
        }

        private void btnStopSrvr_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref mCbMsg);
        }

        private void btnExGen_Click(object sender, RoutedEventArgs e)
        {
			TextBox t = (TextBox)FindName("tbxNe");
			int n = 1;
            if (t != null && 0 < t.Text.Length && !int.TryParse(t.Text, out n))
                n = 1;
			Question.svvQuest = new List<Question>[n];
			while(0 < n) {
				int v;
				List<Question> l = new List<Question>();
				for (int i = 1; i < 16; ++i)
				{
					t = (TextBox)FindName("tbxIU" + i);
					if (t != null && 0 < t.Text.Length)
					{
						if (int.TryParse(t.Text, out v) && 0 < v)
						{
							IUxx iu = (IUxx)i;
							Question.DBSelect(iu, v, ref l);
						}
					}
				}
				--n;
				if(0 < l.Count)
					Question.svvQuest[n] = l;
			}
            if (0 < Question.svvQuest.Length)
                Question.ToByteArr(false);
            LoadQuest();
        }

        private void LoadQuest()
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
				tbcQuest.Items.Clear();
				int e = 0;
				foreach(List<Question> l in Question.svvQuest) {
					TabItem ti = new TabItem();
					ti.Header = ++e;
                    ScrollViewer svwr = new ScrollViewer();
                    svwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
					StackPanel sp = new StackPanel();
                    int x = 0;
                    foreach (Question q in l)
					{
						TextBlock i = new TextBlock();
						i.Text = ++x + ") " + q.ToString();
						dark = !dark;
						if (dark)
							i.Background = new SolidColorBrush(c);
						else
							i.Background = Theme.vBrush[(int)BrushId.LeftPanel_BG];
						sp.Children.Add(i);
					}
                    svwr.Content = sp;
                    ti.Content = svwr;
					tbcQuest.Items.Add(ti);
				}
            });
        }

        private void btnPrep_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                NavigationService.Navigate(new Uri("Prep0.xaml", UriKind.Relative));
            });
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        public bool SrvrCodeHndl(NetCode c, byte[] dat, int offs, ref byte[] outMsg)
        {
            switch (c)
            {
                case NetCode.DateStudentRetriving:
                    int sz = 0;
                    if (Date.sbArr != null)
                        sz += Date.sbArr.Length;
                    if (Examinee.sbArr != null)
                        sz += Examinee.sbArr.Length;
                    outMsg = new byte[sz];
                    sz = 0;
                    if (Date.sbArr != null)
                    {
                        sz = Date.sbArr.Length;
                        Buffer.BlockCopy(Date.sbArr, 0, outMsg, 0, sz);
                    }
                    if (Examinee.sbArr != null)
                        Buffer.BlockCopy(Examinee.sbArr, 0, outMsg, sz, Examinee.sbArr.Length);
                    break;
                case NetCode.QuestAnsKeyRetrieving:
                    //outMsg = Question.sbArr;
                    outMsg = Question.sbArrwKey;
                    return false;
                    //break;
                case NetCode.SrvrSubmitting:
                    Examinee.ReadMarkArr(dat, ref offs);
                    LoadMarks();
                    break;
                default:
                    outMsg = BitConverter.GetBytes((Int32)NetCode.Unknown);
                    break;
            }
            return true;
        }

        public void InitQPanel()
        {
            for(int i = 1; i < 16; ++i)
            {
                TextBox t = (TextBox)FindName("tbxIU" + i);
                if(t != null)
                {
                    t.Text = "0";
                    t.MaxLength = 2;
                    t.KeyDown += txtIU_KeyDown;
                    t.TextChanged += tbxIU_TextChanged;
                }
            }
            tbxNq.Text = "0";
        }

        private void tbxIU_TextChanged(object sender, TextChangedEventArgs e)
        {
            int n = 0, v;
            for (int i = 1; i < 16; ++i)
            {
                TextBox t = (TextBox)FindName("tbxIU" + i);
                if (t != null && 0 < t.Text.Length)
                {
                    if (int.TryParse(t.Text, out v))
                        n += v;
                    else
                        t.Text = "0";
                }
            }
            tbxNq.Text = "" + n;
        }

        private void txtIU_KeyDown(object sender, KeyEventArgs e)
        {
            //TextBox t = sender as TextBox;
            //short v;
            //if(!short.TryParse(t.Text, out v))
            //    e.Handled = true;
        }

        private void LoadTxt()
        {
            mTxt = new Txt();
            mTxt.ReadByte(Txt.sRPath + "samples/GUI-vi.bin");
            btnExit.Content = mTxt._[(int)TxI.EXIT];
            btnStartSrvr.Content = mTxt._[(int)TxI.STRT_SRVR];
            btnStopSrvr.Content = mTxt._[(int)TxI.STOP_SRVR];
            btnPrep.Content = mTxt._[(int)TxI.PREP];
            btnExGen.Content = mTxt._[(int)TxI.EX_GEN];
            btnExit.Content = mTxt._[(int)TxI.EXIT];
            txtNe.Text = mTxt._[(int)TxI.EX_SH_N];
            txtNq.Text = mTxt._[(int)TxI.Q_N];
            txtBirdate.Text = mTxt._[(int)TxI.BIRDATE];
            txtBirpl.Text = mTxt._[(int)TxI.BIRPL];
            txtName.Text = mTxt._[(int)TxI.NEE_NAME];
            txtId.Text = mTxt._[(int)TxI.NEEID_S];
            txtMark.Text = mTxt._[(int)TxI.MARK];
        }
    }
}
