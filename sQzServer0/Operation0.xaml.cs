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
        Dictionary<int, QuestPack> vQPack;
        Dictionary<int, AnsPack> vKeyPack;
        Dictionary<int, int> vuQSId;

        public Operation0()
        {
            InitializeComponent();
            ShowsNavigationUI = false;
            mServer = new Server2(SrvrBufHndl);
            mCbMsg = new UICbMsg();
            vGrade = new Dictionary<int, TextBlock>();
            vDt1 = new Dictionary<int, TextBlock>();
            vDt2 = new Dictionary<int, TextBlock>();
            vComp = new Dictionary<int, TextBlock>();
            vQPack = new Dictionary<int, QuestPack>();
            vKeyPack = new Dictionary<int, AnsPack>();
            mSl = new ExamSlot();
            vuQSId = new Dictionary<int, int>();

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
                    foreach (ExamineeA e in r.vExaminee.Values)
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
                        vGrade.Add(e.mLv + e.uId, t);
                        if(e.uGrade != ushort.MaxValue)
                            t.Text = e.uGrade.ToString();
                        Grid.SetRow(t, rid);
                        Grid.SetColumn(t, 4);
                        gNee.Children.Add(t);
                        t = new TextBlock();
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        vDt1.Add(e.mLv + e.uId, t);
                        if (e.dtTim1.Year != ExamSlot.INVALID)
                            t.Text = e.dtTim1.ToString("HH:mm");
                        Grid.SetRow(t, rid);
                        Grid.SetColumn(t, 5);
                        gNee.Children.Add(t);
                        t = new TextBlock();
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        vDt2.Add(e.mLv + e.uId, t);
                        if (e.dtTim2.Year != ExamSlot.INVALID)
                            t.Text = e.dtTim2.ToString("HH:mm");
                        Grid.SetRow(t, rid);
                        Grid.SetColumn(t, 6);
                        gNee.Children.Add(t);
                        t = new TextBlock();
                        if (dark)
                            t.Background = new SolidColorBrush(c);
                        vComp.Add(e.mLv + e.uId, t);
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
                    foreach (ExamineeA e in r.vExaminee.Values)
                    {
                        if(e.uGrade != ushort.MaxValue && vGrade.TryGetValue(e.mLv + e.uId, out t))
                            t.Text = e.uGrade.ToString();
                        if (e.dtTim1.Hour != ExamSlot.INVALID && vDt1.TryGetValue(e.mLv + e.uId, out t))
                            t.Text = e.dtTim1.ToString("HH:mm");
                        if (e.dtTim2.Hour != ExamSlot.INVALID && vDt2.TryGetValue(e.mLv + e.uId, out t))
                            t.Text = e.dtTim2.ToString("HH:mm");
                        if (e.tComp != null && vComp.TryGetValue(e.mLv + e.uId, out t))
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

            vuQSId.Add(-1, QuestPack.DBCurQSId(mSl.uId, ExamLv.A));
            vuQSId.Add(1, QuestPack.DBCurQSId(mSl.uId, ExamLv.B));

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

        private void btnQSGen_Click(object sender, RoutedEventArgs e)
        {
            if (mSl.uId == uint.MaxValue)
                return;
			TextBox t = FindName("tbxNqs") as TextBox;
            int n = int.Parse(t.Text);
            List<QuestSheet> l = new List<QuestSheet>();
            int i;
            if (rdoB.IsChecked.HasValue ? rdoB.IsChecked.Value : false)
                i = 0;
            else
                i = 1;
			while(0 < n) {
                QuestSheet qs = new QuestSheet();
                foreach (int j in QuestSheet.GetIUId(i))
                {
                    t = FindName("tbxIU" + j) as TextBox;
                    if (t != null)
                        qs.DBSelect((IUxx)j, int.Parse(t.Text));
                }
                if (0 < qs.vQuest.Count)
                {
                    qs.uId = (ushort) ++vuQSId[i];
                    vQPack[i].vSheet.Add(qs.uId, qs);
                    l.Add(qs);
                }
                --n;
            }
            vKeyPack[i].ExtractKey(vQPack[i]);
            vQPack[i].DBIns(mSl.uId, l);
            ShowQuest();
        }

        private void ShowQuest()
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
				tbcQuest.Items.Clear();
                foreach(QuestPack p in vQPack.Values)
				    foreach(QuestSheet qs in p.vSheet.Values) {
					    TabItem ti = new TabItem();
					    ti.Header = qs.uId * p.mLv;
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

        public bool SrvrBufHndl(byte[] buf, out byte[] outMsg)
        {
            int offs = 0;
            NetCode c = (NetCode)BitConverter.ToInt32(buf, offs);
            offs += 4;
            switch (c)
            {
                case NetCode.DateStudentRetriving:
                    int sz = 0;
                    if (buf.Length - offs < 4)
                    {
                        outMsg = null;
                        break;
                    }
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
                    return true;
                case NetCode.QuestRetrieving:
                    outMsg = null;//todo mQPack.ToByte();
                    return true;
                case NetCode.AnsKeyRetrieving:
                    outMsg = null; //todo new byte[mKeyPack.GetByteCount()];
                    offs = 0;
                    //todo mKeyPack.ToByte(ref outMsg, ref offs);
                    break;
                case NetCode.RequestQuestSheet:
                    if (buf.Length - offs == 4)
                    {
                        int qsId = BitConverter.ToInt32(buf, offs);
                        offs += 4;
                        QuestSheet qs = new QuestSheet();
                        if (qs.DBSelect(mSl.uId, -1, (ushort)qsId))
                        {
                            //todo mQPack.vSheet.Add(qs.uId, qs);
                            AnsSheet a = null;//todo mKeyPack.ExtractKey(qs);
                            List<byte[]> bs = qs.ToByte();
                            sz = 1;
                            if (a != null)
                                sz += a.GetByteCount();
                            foreach (byte[] b in bs)
                                sz += b.Length;
                            outMsg = new byte[sz];
                            Array.Copy(BitConverter.GetBytes(true), 0, outMsg, 0, 1);
                            offs = 1;
                            foreach (byte[] b in bs)
                            {
                                Array.Copy(b, 0, outMsg, offs, b.Length);
                                offs += b.Length;
                            }
                            if(a != null)
                                a.ToByte(ref outMsg, ref offs);
                            Dispatcher.Invoke(() => ShowQuest());
                        }
                        else
                            outMsg = BitConverter.GetBytes(false);
                    }
                    else
                        outMsg = BitConverter.GetBytes(false);
                    break;
                case NetCode.SrvrSubmitting:
                    mSl.ReadByteNee(buf, ref offs);
                    mSl.DBUpdateRs();
                    UpdateRsView();
                    outMsg = BitConverter.GetBytes(1);
                    mCbMsg += Txt.s._[(int)TxI.SRVR_DB_OK];
                    break;
                default:
                    outMsg = null;
                    break;
            }
            return false;
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
            tbxNqs.MaxLength = 2;
            tbxNqs.PreviewKeyDown += tbxIU_PrevwKeyDown;
            tbxNqs.TextChanged += tbxIU_TextChanged;
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
            TextBox t = FindName("tbxNqs") as TextBox;
            if(t == null || t.Text == null || t.Text.Length == 0 || int.Parse(t.Text) == 0)
            {
                btnQSGen.IsEnabled = false;
                return;
            }
            int n = 0, i;
            bool bG = true;
            if (rdoB.IsChecked.HasValue? rdoB.IsChecked.Value : false)
            {
                foreach(int j in QuestSheet.GetIUId(-1))
                {
                    t = FindName("tbxIU" + j) as TextBox;
                    if (t != null && t.Text != null && 0 < t.Text.Length && 0 < (i = int.Parse(t.Text)))
                        n += i;
                    else
                        bG = false;
                }
                tbxNq.Text = n.ToString();
                if (bG && n == 30)
                    btnQSGen.IsEnabled = true;
                else
                    btnQSGen.IsEnabled = false;
            }
            else
            {
                foreach (int j in QuestSheet.GetIUId(1))
                {
                    t = FindName("tbxIU" + j) as TextBox;
                    if (t != null && t.Text != null && 0 < t.Text.Length && 0 < (i = int.Parse(t.Text)))
                        n += i;
                    else
                        bG = false;
                }

                tbxNq.Text = n.ToString();
                if (bG && n == 30)
                    btnQSGen.IsEnabled = true;
                else
                    btnQSGen.IsEnabled = false;
            }
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            btnExit.Content = t._[(int)TxI.EXIT];
            btnStartSrvr.Content = t._[(int)TxI.STRT_SRVR];
            btnStopSrvr.Content = t._[(int)TxI.STOP_SRVR];
            btnPrep.Content = t._[(int)TxI.PREP];
            btnQSGen.Content = t._[(int)TxI.QS_GEN];
            btnExit.Content = t._[(int)TxI.EXIT];
            txtNqs.Text = t._[(int)TxI.QS_N];
            txtNq.Text = t._[(int)TxI.Q_N];
            txtBirdate.Text = t._[(int)TxI.BIRDATE];
            txtBirpl.Text = t._[(int)TxI.BIRPL];
            txtName.Text = t._[(int)TxI.NEE_NAME];
            txtId.Text = t._[(int)TxI.NEEID_S];
            txtGrade.Text = t._[(int)TxI.MARK];
        }

        private void rdo_Checked(object sender, RoutedEventArgs e)
        {
            tbxIU_TextChanged(null, null);
        }
    }
}
