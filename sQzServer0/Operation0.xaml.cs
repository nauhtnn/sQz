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
        Dictionary<uint, ExamSlot> vSl;
        Dictionary<uint, ExamSlotView> vSlVw;

        public Operation0()
        {
            InitializeComponent();
            ShowsNavigationUI = false;
            mServer = new Server2(SrvrBufHndl);
            mCbMsg = new UICbMsg();
            vSl = new Dictionary<uint, ExamSlot>();
            vSlVw = new Dictionary<uint, ExamSlotView>();

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
            if(i.IsSelected)
            {
                ExamSlot sl = new ExamSlot();
                sl.uId = uint.Parse(i.Name.Substring(1));
                if ((i.Content as string)[0] == '*')
                    ExamSlot.Parse((i.Content as string).Substring(1), ExamSlot.FORM_H, out sl.mDt);
                else
                    ExamSlot.Parse((i.Content as string).Substring(1), ExamSlot.FORM_H, out sl.mDt);
                sl.DBSelectNee();
                ExamSlotView vw = new ExamSlotView();
                vw.ShallowCopy(refSpSl);
                vw.mSl = sl;
                vw.ShowExaminee();
                TabItem ti = new TabItem();
                ti.Header = sl.mDt.ToString(ExamSlot.FORM_SH);
                ti.Content = vw;
                tbcSl.Items.Add(ti);
                QuestSheet.DBUpdateCurQSId(sl.uId);
                vSl.Add(sl.uId, sl);
                vSlVw.Add(sl.uId, vw);
            }
            else
                vSl.Remove(uint.Parse(i.Name.Substring(1)));
        }

        private void LoadDates()
        {
            Dictionary<uint, Tuple<DateTime, bool>> v = ExamSlot.DBSelect();
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
                        if(v[i].Item2)
                            it.Content = v[i].Item1.ToString(ExamSlot.FORM_H);
                        else
                            it.Content = "*" + v[i].Item1.ToString(ExamSlot.FORM_H);
                        it.Name = "_" + i;
                        dark = !dark;
                        if (dark)
                            it.Background = new SolidColorBrush(c);
                        lbxDate.Items.Add(it);
                    }
                });
            }
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
            //InitQPanel();

            double rt = spMain.RenderSize.Width / 1280;
            spMain.RenderTransform = new ScaleTransform(rt, rt);

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

        private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning && mCbMsg.ToUp())
                Dispatcher.Invoke(() => {
                    lblStatus.Text += mCbMsg.txt; });
        }

        private void btnStopSrvr_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref mCbMsg);
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
                    //sz += curSl.GetByteCountDt();
                    //List<byte[]> es = curSl.ToByteR1(rId);
                    //foreach(byte[] i in es)
                    //    sz += i.Length;
                    //outMsg = new byte[sz];
                    //sz = 0;
                    //ExamSlot.ToByteDt(outMsg, ref sz, curSl.mDt);
                    //foreach (byte[] i in es)
                    //{
                    //    Buffer.BlockCopy(i, 0, outMsg, sz, i.Length);
                    //    sz += i.Length;
                    //}
                    outMsg = null;
                    return true;
                case NetCode.QuestRetrieving:
                    outMsg = null;// curSl.ToByteQPack();
                    return true;
                case NetCode.AnsKeyRetrieving:
                    outMsg = null;// curSl.ToByteKey();
                    break;
                case NetCode.RequestQuestSheet:
                    if (buf.Length - offs == 4)
                    {
                        int qsId = BitConverter.ToInt32(buf, offs);
                        offs += 4;
                        QuestSheet qs = new QuestSheet();
                        ExamLv lv;
                        if (qsId < (int)ExamLv.B)
                            lv = ExamLv.A;
                        else
                        {
                            lv = ExamLv.B;
                            qsId -= (int)ExamLv.B;
                        }
                        outMsg = null;
                        //if (qs.DBSelect(curSl.uId, lv, qsId))
                        //    outMsg = BitConverter.GetBytes(-1);
                        //else
                        //{
                        //    curSl.vQPack[lv].vSheet.Add(qs.uId, qs);
                        //    AnsSheet a = curSl.mKeyPack.ExtractKey(qs);
                        //    List<byte[]> bs = qs.ToByte();
                        //    sz = 4;
                        //    if (a != null)
                        //        sz += a.GetByteCount();
                        //    foreach (byte[] b in bs)
                        //        sz += b.Length;
                        //    outMsg = new byte[sz];
                        //    Array.Copy(BitConverter.GetBytes(0), 0, outMsg, 0, 4);
                        //    offs = 4;
                        //    foreach (byte[] b in bs)
                        //    {
                        //        Array.Copy(b, 0, outMsg, offs, b.Length);
                        //        offs += b.Length;
                        //    }
                        //    if (a != null)
                        //        a.ToByte(ref outMsg, ref offs);
                        //    Dispatcher.Invoke(() => ShowQuest());
                        //}
                    }
                    else
                        outMsg = BitConverter.GetBytes(-1);
                    break;
                case NetCode.SrvrSubmitting:
                    //curSl.ReadByteR0(buf, ref offs);
                    //curSl.DBUpdateRs();
                    //vSlVw[curSl.uId].UpdateRsView(curSl.vRoom.Values.ToList());
                    //outMsg = BitConverter.GetBytes(1);
                    //mCbMsg += Txt.s._[(int)TxI.SRVR_DB_OK];
                    outMsg = null;
                    break;
                default:
                    outMsg = null;
                    break;
            }
            return false;
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
    }
}
