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
        bool bRunning;
        ExamSlot mSl;
        Dictionary<int, TextBlock> vGrade;
        Dictionary<int, TextBlock> vDt1;
        Dictionary<int, TextBlock> vDt2;
        Dictionary<int, TextBlock> vComp;
        QuestPack mQPack;
        AnsPack mKeyPack;
        ushort[] uQSId;

        public Operation0()
        {
            InitializeComponent();
            ShowsNavigationUI = false;
            mServer = new Server2(SrvrCodeHndl);
            mCbMsg = new UICbMsg();
            vGrade = new Dictionary<int, TextBlock>();
            vDt1 = new Dictionary<int, TextBlock>();
            vDt2 = new Dictionary<int, TextBlock>();
            vComp = new Dictionary<int, TextBlock>();
            mQPack = new QuestPack();
            mKeyPack = new AnsPack();
            mSl = new ExamSlot();
            uQSId = new ushort[2];

            lbxDate.SelectionMode = SelectionMode.Single;
            lbxDate.SelectionChanged += lbxDate_SelectionChanged;

            bRunning = true;
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            UICbMsg dummy = new UICbMsg();
            mServer.Stop(ref dummy);
        }

        private void lbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            ListBoxItem i = (ListBoxItem)l.SelectedItem;
            if (i == null)
                return;
            if (uint.TryParse(i.Name.Substring(1), out mSl.uId))
            {
                ExamSlot.Parse(i.Content as string, ExamSlot.FORM_H, out mSl.mDt);
                mSl.DBSelectNee();
                LoadExaminees();
                uQSId[0] = mQPack.DBCurQSId(mSl.uId, ExamLvl.Basis);
                uQSId[1] = mQPack.DBCurQSId(mSl.uId, ExamLvl.Advance);
            }
            else
                mSl.uId = uint.MaxValue;
        }

        private void LoadDates()
        {
            Dictionary<uint, DateTime> v = mSl.DBSelect();
            if (0 < v.Keys.Count)
            {
                bool dark = true;
                Color c = new Color();
                c.A = 0xff;
                c.B = c.G = c.R = 0xf0;
                Dispatcher.Invoke(() => {
                    lbxDate.Items.Clear();
                    foreach (uint i in v.Keys)
                    {
                        ListBoxItem it = new ListBoxItem();
                        it.Content = v[i].ToString(ExamSlot.FORM_H);
                        it.Name = "_" + i;
                        dark = !dark;
                        if (dark)
                            it.Background = new SolidColorBrush(c);
                        lbxDate.Items.Add(it);
                    }
                });
            }
        }

        private void LoadExaminees() //same as Prep0.xaml
        {
            Dispatcher.Invoke(() => {
                Color c = new Color();
                c.A = 0xff;
                c.B = c.G = c.R = 0xf0;
                bool dark = false;
                int rid = -1;
                vGrade.Clear();
                vDt1.Clear();
                vDt2.Clear();
                vComp.Clear();
                gNee.Children.Clear();
                foreach(ExamRoom r in mSl.vRoom.Values)
                    foreach (Examinee e in r.vExaminee.Values)
                    {
                        rid++;
                        RowDefinition rd = new RowDefinition();
                        rd.Height = new GridLength(20);
                        gNee.RowDefinitions.Add(rd);
                        TextBlock t = new TextBlock();
                        t.Text = e.tId;
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        Grid.SetRow(t, rid);
                        gNee.Children.Add(t);
                        t = new TextBlock();
                        t.Text = e.tName;
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        Grid.SetRow(t, rid);
                        Grid.SetColumn(t, 1);
                        gNee.Children.Add(t);
                        t = new TextBlock();
                        t.Text = e.tBirdate;
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        Grid.SetRow(t, rid);
                        Grid.SetColumn(t, 2);
                        gNee.Children.Add(t);
                        t = new TextBlock();
                        t.Text = e.tBirthplace;
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        Grid.SetRow(t, rid);
                        Grid.SetColumn(t, 3);
                        gNee.Children.Add(t);
                        t = new TextBlock();
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        vGrade.Add(e.Lv * e.uId, t);
                        if(e.uGrade != ushort.MaxValue)
                            t.Text = e.uGrade.ToString();
                        Grid.SetRow(t, rid);
                        Grid.SetColumn(t, 4);
                        gNee.Children.Add(t);
                        t = new TextBlock();
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        vDt1.Add(e.Lv * e.uId, t);
                        if (e.dtTim1.Year != ExamSlot.INVALID)
                            t.Text = e.dtTim1.ToString("HH:mm");
                        Grid.SetRow(t, rid);
                        Grid.SetColumn(t, 5);
                        gNee.Children.Add(t);
                        t = new TextBlock();
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        vDt2.Add(e.Lv * e.uId, t);
                        if (e.dtTim2.Year != ExamSlot.INVALID)
                            t.Text = e.dtTim2.ToString("HH:mm");
                        Grid.SetRow(t, rid);
                        Grid.SetColumn(t, 6);
                        gNee.Children.Add(t);
                        t = new TextBlock();
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        vComp.Add(e.Lv * e.uId, t);
                        if (e.tComp != null)
                            t.Text = e.tComp;
                        Grid.SetRow(t, rid);
                        Grid.SetColumn(t, 7);
                        gNee.Children.Add(t);
                        dark = !dark;
                    }
            });
        }

        private void UpdateRsView()
        {
            Dispatcher.Invoke(() => {
                TextBlock t;
                foreach(ExamRoom r in mSl.vRoom.Values)
                    foreach (Examinee e in r.vExaminee.Values)
                    {
                        if(e.uGrade != ushort.MaxValue && vGrade.TryGetValue(e.Lv * e.uId, out t))
                            t.Text = e.uGrade.ToString();
                        if (e.dtTim1.Hour != ExamSlot.INVALID && vDt1.TryGetValue(e.Lv * e.uId, out t))
                            t.Text = e.dtTim1.ToString("HH:mm");
                        if (e.dtTim2.Hour != ExamSlot.INVALID && vDt2.TryGetValue(e.Lv * e.uId, out t))
                            t.Text = e.dtTim2.ToString("HH:mm");
                        if (e.tComp != null && vComp.TryGetValue(e.Lv * e.uId, out t))
                            t.Text = e.tComp;
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

            spMain.Background = Theme.s._[(int)BrushId.Ans_Highlight];

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
            if (bRunning && mCbMsg.ToUp())
                Dispatcher.Invoke(() => {
                    lblStatus.Text += mCbMsg.txt; });
        }

        private void btnStopSrvr_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref mCbMsg);
        }

        private void btnExGen_Click(object sender, RoutedEventArgs e)
        {
            if (mSl.uId == uint.MaxValue)
                return;
			TextBox t = (TextBox)FindName("tbxNe");
			int n = 1;
            if (t != null && 0 < t.Text.Length && !int.TryParse(t.Text, out n))
                n = 1;
            int[][] x = new int[2][];
            x[0] = QuestSheet.GetBasicIU();
            x[1] = QuestSheet.GetAdvanceIU();
            List<QuestSheet> l = new List<QuestSheet>();
			while(0 < n) {
				for(int i = 0; i < 2; ++i)
				{
                    QuestSheet qs = new QuestSheet();
                    foreach (int j in x[i])
                    {
                        t = FindName("tbxIU" + j) as TextBox;
                        if (t != null && 0 < t.Text.Length)
                        {
                            int v;
                            if (int.TryParse(t.Text, out v) && 0 < v)
                            {
                                IUxx iu = (IUxx)j;
                                qs.DBSelect(iu, v);
                            }
                        }
                    }
                    if (0 < qs.vQuest.Count)
                    {
                        qs.uId = ++uQSId[i];
                        mQPack.vSheet.Add(qs.uId, qs);
                        l.Add(qs);
                    }
                }
                --n;
            }
            foreach (QuestSheet qs in mQPack.vSheet.Values)
                qs.ToByte();
            mKeyPack.ExtractKey(mQPack);
            mQPack.DBIns(mSl.uId, l);
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
				foreach(QuestSheet qs in mQPack.vSheet.Values) {
					TabItem ti = new TabItem();
					ti.Header = qs.uId;
                    ScrollViewer svwr = new ScrollViewer();
                    svwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
					StackPanel sp = new StackPanel();
                    int x = 0;
                    foreach (Question q in qs.vQuest)
					{
						TextBlock i = new TextBlock();
						i.Text = ++x + ") " + q.ToString();
						dark = !dark;
						if (dark)
							i.Background = new SolidColorBrush(c);
						else
							i.Background = Theme.s._[(int)BrushId.LeftPanel_BG];
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

        public bool SrvrCodeHndl(NetCode c, byte[] buf, int offs, ref byte[] outMsg)
        {
            switch (c)
            {
                case NetCode.DateStudentRetriving:
                    int sz = 0;
                    if (mSl.uId == uint.MaxValue ||
                        buf.Length - offs < 4)
                        return false;
                    int rId = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    sz += mSl.GetByteCountDt();
                    List<byte[]> es = mSl.ToByteR(rId);
                    foreach(byte[] i in es)
                        sz += i.Length;
                    outMsg = new byte[sz];
                    sz = 0;
                    ExamSlot.ToByteDt(outMsg, ref sz, mSl.mDt);
                    foreach (byte[] i in es)
                    {
                        Buffer.BlockCopy(i, 0, outMsg, sz, i.Length);
                        sz += i.Length;
                    }
                    break;
                case NetCode.QuestRetrieving:
                    outMsg = mQPack.ToByte();
                    break;
                case NetCode.AnsKeyRetrieving:
                    outMsg = new byte[mKeyPack.GetByteCount()];
                    offs = 0;
                    mKeyPack.ToByte(ref outMsg, ref offs);
                    return false;
                case NetCode.SrvrSubmitting:
                    mSl.ReadByteGrade(buf, ref offs);
                    mSl.DBUpdateRs();
                    UpdateRsView();
                    outMsg = BitConverter.GetBytes(1);
                    mCbMsg += Txt.s._[(int)TxI.SRVR_DB_OK];
                    break;
                default:
                    outMsg = BitConverter.GetBytes((int)NetCode.Unknown);
                    break;
            }
            return true;
        }

        public void InitQPanel()
        {
            for(int i = 1; i < 16; ++i)
            {
                TextBox t = FindName("tbxIU" + i) as TextBox;
                if(t != null)
                {
                    t.MaxLength = 2;
                    t.PreviewKeyDown += tbxIU_PrevwKeyDown;
                    t.TextChanged += tbxIU_TextChanged;
                    
                }
            }
            tbxNq.Text = "0";
        }

        private void tbxIU_PrevwKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete && e.Key != Key.Back && e.Key != Key.Tab &&
                ((int)e.Key < (int)Key.Left || (int)Key.Down < (int)e.Key) &&
                ((int)e.Key < (int)Key.D0 || (int)Key.D9 < (int)e.Key))
                e.Handled = true;
        }

        private void tbxIU_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool bas = true, adv = false;//todo
            int n = 0, i;
            for(int j = 1; j < 7; ++j)
            {
                TextBox t = FindName("tbxIU" + j) as TextBox;
                if (t != null && int.TryParse(t.Text, out i) && 0 < i)
                    n += i;
                else
                    bas = false;
            }
            tbxNq.Text = n.ToString();
            if ((bas && adv) || n != 30)
                btnExGen.IsEnabled = false;
            else
                btnExGen.IsEnabled = true;
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            btnExit.Content = t._[(int)TxI.EXIT];
            btnStartSrvr.Content = t._[(int)TxI.STRT_SRVR];
            btnStopSrvr.Content = t._[(int)TxI.STOP_SRVR];
            btnPrep.Content = t._[(int)TxI.PREP];
            btnExGen.Content = t._[(int)TxI.EX_GEN];
            btnExit.Content = t._[(int)TxI.EXIT];
            txtNe.Text = t._[(int)TxI.EX_SH_N];
            txtNq.Text = t._[(int)TxI.Q_N];
            txtBirdate.Text = t._[(int)TxI.BIRDATE];
            txtBirpl.Text = t._[(int)TxI.BIRPL];
            txtName.Text = t._[(int)TxI.NEE_NAME];
            txtId.Text = t._[(int)TxI.NEEID_S];
            txtGrade.Text = t._[(int)TxI.MARK];
        }
    }
}
