using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using sQzLib;

namespace sQzServer1
{
    /// <summary>
    /// Interaction logic for Operation1.xaml
    /// </summary>
    public partial class Operation1 : Page
    {
        Client2 mClnt;
        NetCode mState;
        Server2 mServer;
        UICbMsg mCbMsg;
        bool bRunning;
        ExamBoard mBrd;
        int uRId;//todo change to enum
        List<SortedList<int, bool>> vfbLock;

        public Operation1()
        {
            InitializeComponent();

            mState = NetCode.Srvr1DatRetriving;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, true);
            mServer = new Server2(SrvrBufHndl);
            mServer.SrvrPort = 23821;
            mCbMsg = new UICbMsg();
            bRunning = true;

            mBrd = new ExamBoard();

            if(!System.IO.File.Exists("Room.txt") ||
                !int.TryParse(System.IO.File.ReadAllText("Room.txt"), out uRId))
                uRId = 0;

            vfbLock = new List<SortedList<int, bool>>();

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 13;

            WPopup.nwIns(w);

            spMain.Background = Theme.s._[(int)BrushId.BG];

            LoadTxt();
        }

        private void btnConn_Click(object sender, RoutedEventArgs e)
        {
            btnStop_Click(null, null);
            //todo: check th state to return
            bool printerTest = true;
            if(printerTest)
            {
                mState = NetCode.Srvr1DatRetriving;
                mBrd = CreateFakeData();
                ClntBufHndl(null);
                return;
            }
            Task.Run(() => { mClnt.ConnectWR(ref mCbMsg); });
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { mServer.Start(ref mCbMsg); });
            btnStrt.IsEnabled = false;
            btnStrt.Foreground = Theme.s._[(int)BrushId.FG_Gray];
            btnStrt.Background = Theme.s._[(int)BrushId.BG_Gray];
            btnStop.IsEnabled = true;
            btnStop.Foreground = Theme.s._[(int)BrushId.FG];
            btnStop.Background = Theme.s._[(int)BrushId.mReconn];
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mServer.Stop(ref mCbMsg);
            btnStop.IsEnabled = false;
            btnStop.Foreground = Theme.s._[(int)BrushId.FG_Gray];
            btnStop.Background = Theme.s._[(int)BrushId.BG_Gray];
            btnStrt.IsEnabled = true;
            btnStrt.Foreground = Theme.s._[(int)BrushId.FG];
            btnStrt.Background = Theme.s._[(int)BrushId.mConn];
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            WPopup.s.wpCb = Exit;
            WPopup.s.ShowDialog(Txt.s._[(int)TxI.OP1_EXIT_CAUT],
                Txt.s._[(int)TxI.EXIT], Txt.s._[(int)TxI.BTN_CNCL], null);
        }

        private void Exit()
        {
            Window.GetWindow(this).Close();
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            UICbMsg dummy = new UICbMsg();
            mServer.Stop(ref dummy);
            WPopup.s.Exit();
        }

        public bool SrvrBufHndl(byte[] buf, out byte[] outMsg)
        {
            outMsg = null;
            int offs = 0;
			NetCode c = (NetCode)BitConverter.ToInt32(buf, offs);
            offs += 4;
            QuestSheet qs;
            int lvid;
            ExamineeA e;
            DateTime dt;
            switch (c)
            {
                case NetCode.Dating:
                    outMsg = new byte[DT.BYTE_COUNT];
                    offs = 0;
                    DT.ToByte(outMsg, ref offs, mBrd.mDt);
                    return true;
                case NetCode.Authenticating:
                    e = new ExamineeS1();
                    e.bFromC = true;
                    e.ReadByte(buf, ref offs);
                    bool lck = true;
                    bool found = false;
                    foreach (SortedList<int, bool> l in vfbLock)
                        if (l.TryGetValue(e.LvId, out lck))
                        {
                            found = true;
                            break;
                        }
                    if (!found)
                        lck = false;
                    if (!lck)
                    {
                        ExamineeA o = null;
                        dt = DateTime.Now;
                        foreach(ExamSlot sl in mBrd.vSl.Values)
                            if ((o = sl.Signin(e)) != null)
                            {
                                dt = sl.Dt;
                                break;
                            }
                        
                        if (o != null)
                        {
                            if (o.dtTim1.Hour == DT.INV)
                                o.dtTim1 = DateTime.Now;
                            Dispatcher.InvokeAsync(() =>
                            {
                                foreach(Op1SlotView vw in tbcSl.Items.OfType<Op1SlotView>())
                                {
                                    TextBlock t;
                                    lvid = o.LvId;
                                    if (vw.vComp.TryGetValue(lvid, out t))
                                        t.Text = o.tComp;
                                    if (vw.vDt1.TryGetValue(lvid, out t))
                                        t.Text = o.dtTim1.ToString("HH:mm");
                                    CheckBox cbx;
                                    if (vw.vLock.TryGetValue(lvid, out cbx))
                                    {
                                        cbx.IsChecked = true;
                                        cbx.IsEnabled = true;
                                    }
                                    if (vw.vbLock.Keys.Contains(lvid))
                                        vw.vbLock[lvid] = true;
                                }
                            });
                            byte[] a;
                            o.bFromC = true;
                            o.ToByte(out a);
                            outMsg = new byte[4 + a.Length];
                            Buffer.BlockCopy(BitConverter.GetBytes(0), 0, outMsg, 0, 4);
                            Buffer.BlockCopy(a, 0, outMsg, 4, a.Length);
                        }
                        else
                        {
                            outMsg = new byte[4];
                            Buffer.BlockCopy(BitConverter.GetBytes((int)TxI.SIGNIN_NOK), 0, outMsg, 0, 4);
                            return false;//close
                        }
                    }
                    else
                    {
                        ExamineeA o = null;
                        foreach (ExamSlot sl in mBrd.vSl.Values)
                            if ((o = sl.Find(e.LvId)) != null)
                                break;
                        if (o == null)
                            o = new ExamineeC();
                        if (o.tComp == null)
                            outMsg = new byte[16];
                        else
                            outMsg = new byte[16 + o.tComp.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes((int)TxI.SIGNIN_AL), 0, outMsg, 0, 4);
                        if (o.tComp == null)
                        {
                            Buffer.BlockCopy(BitConverter.GetBytes(0), 0, outMsg, 4, 4);
                            offs = 8;
                        }
                        else
                        {
                            byte[] comp = Encoding.UTF8.GetBytes(o.tComp);
                            Buffer.BlockCopy(BitConverter.GetBytes(comp.Length), 0, outMsg, 4, 4);
                            offs = 8;
                            Buffer.BlockCopy(comp, 0, outMsg, offs, o.tComp.Length);
                            offs += comp.Length;
                        }

                        Buffer.BlockCopy(BitConverter.GetBytes(o.dtTim1.Hour), 0, outMsg, offs, 4);
                        offs += 4;
                        Buffer.BlockCopy(BitConverter.GetBytes(o.dtTim1.Minute), 0, outMsg, offs, 4);
                        break;
                    }
                    return true;
                case NetCode.ExamRetrieving:
                    outMsg = null;
                    lvid = BitConverter.ToInt32(buf, offs);
                    ExamSlot slo = null;
                    foreach (ExamSlot s in mBrd.vSl.Values)
                        foreach(ExamRoom r in s.vRoom.Values)
                            if(r.vExaminee.ContainsKey(lvid))
                            {
                                slo = s;
                                break;
                            }
                    if(slo == null)
                    {
                        outMsg = new byte[4];
                        Array.Copy(BitConverter.GetBytes((int)TxI.NEEID_NF), 0, outMsg, 0, 4);
                        break;
                    }
                    ExamLv lv = (lvid < ExamineeA.LV_CAP) ? ExamLv.A : ExamLv.B;
                    offs += 4;
                    int qsid = BitConverter.ToInt32(buf, offs);
                    if (qsid == ExamineeA.LV_CAP)
                    {
                        byte[] a = slo.ToByteNextQS(lv);
                        if (a != null)
                        {
                            outMsg = new byte[a.Length + 4];
                            Array.Copy(BitConverter.GetBytes(0), outMsg, 4);
                            Array.Copy(a, 0, outMsg, 4, a.Length);
                        }
                    }
                    else if (slo.vQPack[lv].vSheet.TryGetValue(qsid, out qs))
                    {
                        outMsg = new byte[qs.aQuest.Length + 4];
                        Array.Copy(BitConverter.GetBytes(0), outMsg, 4);
                        Array.Copy(qs.aQuest, 0, outMsg, 4, qs.aQuest.Length);
                    }
                    if (outMsg == null)
                    {
                        mCbMsg += Txt.s._[(int)TxI.QS_NFOUND] + (qsid);
                        outMsg = new byte[8];
                        Array.Copy(BitConverter.GetBytes((int)TxI.QS_NFOUND), 0, outMsg, 0, 4);
                        Array.Copy(BitConverter.GetBytes(qsid), 0, outMsg, 4, 4);
                    }
                    break;
                case NetCode.Submiting:
                    e = new ExamineeS1();
                    e.bFromC = true;
                    if (!e.ReadByte(buf, ref offs))
                    {
                        AnsSheet keySh = null;
                        found = false;
                        foreach(ExamSlot sl in mBrd.vSl.Values)
                            if(sl.mKeyPack.vSheet.TryGetValue(e.mAnsSh.uQSLvId, out keySh))
                            {
                                found = true;
                                break;
                            }
                        if (!found)
                        {
                            outMsg = BitConverter.GetBytes(101);//todo
                            break;
                        }
                        ExamineeA o = null;
                        lvid = e.LvId;
                        found = false;
                        foreach (ExamSlot sl in mBrd.vSl.Values)
                            if ((o = sl.Find(lvid)) != null)
                                break;
                        if (o != null)
                        {
                            o.eStt = NeeStt.Finished;
                            o.mAnsSh = e.mAnsSh;
                            o.uGrade = keySh.Grade(e.mAnsSh.aAns);
                            o.dtTim2 = DateTime.Now;
                            foreach (SortedList<int, bool> sl in vfbLock)
                                if (sl.ContainsKey(lvid))
                                    sl[lvid] = true;
                            Dispatcher.InvokeAsync(() =>
                            {
                                bool toSubm = true;
                                foreach (Op1SlotView vw in tbcSl.Items.OfType<Op1SlotView>())
                                {
                                    TextBlock t = null;
                                    if (vw.vDt2.TryGetValue(lvid, out t))
                                        t.Text = o.dtTim2.ToString("HH:mm");
                                    if (vw.vMark.TryGetValue(lvid, out t))
                                        t.Text = o.Grade.ToString();
                                    CheckBox cbx;
                                    if (vw.vLock.TryGetValue(lvid, out cbx))
                                    {
                                        cbx.IsChecked = true;
                                        cbx.IsEnabled = false;
                                    }
                                    if(vw.vAbsen.TryGetValue(lvid, out cbx))
                                        cbx.IsChecked = cbx.IsEnabled = false;
                                    if (!vw.ToSubmit())
                                        toSubm = false;
                                }
                                if (toSubm)
                                    ToSubmit(true);
                            });
                            o.ToByte(out outMsg, 0);
                        }
                        else
                        {
                            mCbMsg += Txt.s._[(int)TxI.NEEID_NF] + ' ' + lvid;
                            outMsg = BitConverter.GetBytes((int)TxI.NEEID_NF);
                        }
                    }
                    else
                    {
                        mCbMsg += Txt.s._[(int)TxI.RECV_DAT_ER];
                        outMsg = BitConverter.GetBytes((int)TxI.RECV_DAT_ER);
                    }
                    break;
                default:
                    outMsg = null;
                    break;
            }
            return false;
        }

        public bool ClntBufHndl(byte[] buf)
        {
            bool isProduction = false;
            int offs = 0;
            switch (mState)
            {
                case NetCode.Srvr1DatRetriving:
                    if (isProduction && mBrd.ReadByteSl1(buf, ref offs))
                    {
                        Dispatcher.InvokeAsync(() =>
                            WPopup.s.ShowDialog(Txt.s._[(int)TxI.OP1_DT_NOK]));
                        break;
                    }
                    Dispatcher.InvokeAsync(() => LoadSl());
                    if(isProduction)
                        mState = NetCode.QuestRetrieving;
                    return true;
                case NetCode.QuestRetrieving:
                    if (mBrd.ReadByteQPack(buf, ref offs))
                    {
                        Dispatcher.InvokeAsync(() => 
                            WPopup.s.ShowDialog(Txt.s._[(int)TxI.OP1_Q_NOK]));
                        break;
                    }
                    mState = NetCode.AnsKeyRetrieving;
                    return true;
                case NetCode.AnsKeyRetrieving:
                    if (isProduction && mBrd.ReadByteKey(buf, ref offs))
                    {
                        Dispatcher.InvokeAsync(() =>
                            WPopup.s.ShowDialog(Txt.s._[(int)TxI.OP1_KEY_NOK]));
                        break;
                    }
                    else
                        Dispatcher.InvokeAsync(()=> {
                            btnStrt.IsEnabled = true;
                            btnStrt.Foreground = Theme.s._[(int)BrushId.FG];
                            btnStrt.Background = Theme.s._[(int)BrushId.mConn];
                            btnConn.IsEnabled = false;
                            btnConn.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                            btnConn.Background = Theme.s._[(int)BrushId.BG_Gray];
                        });
                    break;
                case NetCode.SrvrSubmitting:
                    if (buf.Length - offs == 4 && BitConverter.ToInt32(buf, offs) == 1)
                    {
                        //mCbMsg += Txt.s._[(int)TxI.SRVR_SUBMT_OK];
                        Dispatcher.InvokeAsync(() =>
                        {
                            btnSubmit.IsEnabled = false;
                            btnSubmit.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                            btnSubmit.Background = Theme.s._[(int)BrushId.BG_Gray];
                        });
                    }
                    break;
            }
            return false;
        }

        public byte[] ClntBufPrep()
        {
            byte[] outMsg = null;
            switch (mState)
            {
                case NetCode.Srvr1DatRetriving:
                    outMsg = new byte[8];
                    Array.Copy(BitConverter.GetBytes((int)mState), 0, outMsg, 0, 4);
                    Array.Copy(BitConverter.GetBytes(uRId), 0, outMsg, 4, 4);
                    break;
                case NetCode.QuestRetrieving:
                    outMsg = new byte[8];
                    Buffer.BlockCopy(BitConverter.GetBytes((int)mState), 0, outMsg, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(uRId), 0, outMsg, 4, 4);
                    break;
                case NetCode.AnsKeyRetrieving:
                    outMsg = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.SrvrSubmitting:
                    outMsg = mBrd.ToByteSl0(BitConverter.GetBytes((int)NetCode.SrvrSubmitting));
                    break;
            }
            return outMsg;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            //todo: check th state to return
            mState = NetCode.SrvrSubmitting;
            Task.Run(() => { mClnt.ConnectWR(ref mCbMsg); });
        }

        void LoadTxt()
        {
            Txt s = Txt.s;
            btnClose.Content = s._[(int)TxI.EXIT];
            btnConn.Content = s._[(int)TxI.CONN];
            btnStrt.Content = s._[(int)TxI.STRT_SRVR];
            btnStop.Content = s._[(int)TxI.STOP_SRVR];
            btnSubmit.Content = s._[(int)TxI.SUBMIT];

            txtId.Text = s._[(int)TxI.NEEID_S];
            txtName.Text = s._[(int)TxI.NEE_NAME];
            txtBirdate.Text = s._[(int)TxI.BIRDATE];
            txtBirpl.Text = s._[(int)TxI.BIRPL];
            txtComp.Text = s._[(int)TxI.COMP];
            txtT1.Text = s._[(int)TxI.T1];
            txtT2.Text = s._[(int)TxI.T2];
            txtGrade.Text = s._[(int)TxI.MARK];
            txtLock.Text = s._[(int)TxI.OP_LCK];
            txtAbsence.Text = s._[(int)TxI.OP_ABSENCE];
        }

        void ToSubmit(bool bEnable)
        {
            if(bEnable)
            {
                btnSubmit.IsEnabled = true;
                btnSubmit.Foreground = Theme.s._[(int)BrushId.FG];
                btnSubmit.Background = Theme.s._[(int)BrushId.mSubmit];
            }
            else
            {
                btnSubmit.IsEnabled = false;
                btnSubmit.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                btnSubmit.Background = Theme.s._[(int)BrushId.BG_Gray];
            }
        }

        private void lbxSl_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            foreach (TabItem t in tbcSl.Items)
                if (t.Name == "_" + (i.Content as string).Replace(':', '_'))
                    return;
            ExamSlot sl;
            if (!mBrd.vSl.TryGetValue(i.Content as string, out sl))
                return;

            Op1SlotView vw = new Op1SlotView();
            vw.mSl = sl;
            vw.DeepCopyNee(tbiRefNee);
            vw.ShowExaminee();
            vfbLock.Add(vw.vbLock);
            vw.Name = "_" + (i.Content as string).Replace(':', '_');
            vw.Header = sl.Dt.ToString(DT.hh);
            vw.toSubmCb = ToSubmit;
            tbcSl.Items.Add(vw);
            vw.Focus();
        }

        private void lbxSl_Unselected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            mBrd.vSl.Remove(i.Content as string);
            foreach (TabItem ti in tbcSl.Items)
                if (ti.Name == "_" + (i.Content as string).Replace(':', '_'))
                {
                    tbcSl.Items.Remove(ti);
                    Op1SlotView vw = ti as Op1SlotView;
                    if (vw != null)
                        vfbLock.Remove(vw.vbLock);
                    break;
                }
        }

        private void LoadSl()
        {
            List<DateTime> v = mBrd.ListSl();
            //bool dark = true;
            //Color c = new Color();
            //c.A = 0xff;
            //c.B = c.G = c.R = 0xf0;
            lbxSl.Items.Clear();
            foreach (DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DT.hh);
                it.Selected += lbxSl_Selected;
                it.Unselected += lbxSl_Unselected;
                //dark = !dark;
                //if (dark)
                //    it.Background = new SolidColorBrush(c);
                lbxSl.Items.Add(it);
            }
        }

        private void btnHck_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists("Hck.txt"))
            {
                string t = System.IO.File.ReadAllText("Hck.txt");
                if (t == "allButtons")
                {
                    btnConn.IsEnabled = btnStrt.IsEnabled =
                        btnStop.IsEnabled = btnSubmit.IsEnabled = true;
                    btnConn.Foreground = btnStrt.Foreground =
                        btnStop.Foreground = btnSubmit.Foreground =
                        Theme.s._[(int)BrushId.mSubmit];
                }
            }
        }

        private ExamBoard CreateFakeData()
        {
            var board = new ExamBoard();
            board.mDt = DateTime.Now;
            var slot = new ExamSlot();
            slot.Dt = board.mDt;
            board.vSl.Add(slot.mDt.ToString("HH:mm"), slot);
            for (int i = 0; i < 6; ++i)
            {
                ExamRoom r = new ExamRoom();
                r.uId = i;
                slot.vRoom.Add(r.uId, r);
            }
            slot.ReadF("fake.txt", ref slot);
            return board;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
                var word = new Microsoft.Office.Interop.Word.Application { Visible = false };
        // where did you get this file name?
        string fileName = "F:/projects/sqz009printer/sQzServer1/bin/Debug/sqz_server1_template.docx";

            // as you mentioned, you open your word document here
            var doc = word.Documents.Open(fileName, ReadOnly: true, Visible: true);

            doc.PrintOut();
        }

    private void PrintFlowDoc()
        {
            var document = CreateFlowDocument();
            // Clone the source document's content into a new FlowDocument.
            // This is because the pagination for the printer needs to be
            // done differently than the pagination for the displayed page.
            // We print the copy, rather that the original FlowDocument.
            System.IO.MemoryStream s = new System.IO.MemoryStream();
            TextRange source = new TextRange(document.ContentStart, document.ContentEnd);
            source.Save(s, DataFormats.Xaml);
            FlowDocument copy = new FlowDocument();
            TextRange dest = new TextRange(copy.ContentStart, copy.ContentEnd);
            dest.Load(s, DataFormats.Xaml);

            // Create a XpsDocumentWriter object, implicitly opening a Windows common print dialog,
            // and allowing the user to select a printer.

            // get information about the dimensions of the seleted printer+media.
            System.Printing.PrintDocumentImageableArea ia = null;
            System.Windows.Xps.XpsDocumentWriter docWriter = System.Printing.PrintQueue.CreateXpsDocumentWriter(ref ia);

            if (docWriter != null && ia != null)
            {
                DocumentPaginator paginator = ((IDocumentPaginatorSource)copy).DocumentPaginator;

                // Change the PageSize and PagePadding for the document to match the CanvasSize for the printer device.
                paginator.PageSize = new Size(ia.MediaSizeWidth, ia.MediaSizeHeight);
                Thickness t = new Thickness(72);  // copy.PagePadding;
                copy.PagePadding = new Thickness(
                                 Math.Max(ia.OriginWidth, t.Left),
                                   Math.Max(ia.OriginHeight, t.Top),
                                   Math.Max(ia.MediaSizeWidth - (ia.OriginWidth + ia.ExtentWidth), t.Right),
                                   Math.Max(ia.MediaSizeHeight - (ia.OriginHeight + ia.ExtentHeight), t.Bottom));

                copy.ColumnWidth = double.PositiveInfinity;
                //copy.PageWidth = 528; // allow the page to be the natural with of the output device

                // Send content to the printer.
                docWriter.Write(paginator);
            }
        }

        private FlowDocument CreateFlowDocument()
        {
            var table1 = new Table();
            // Create 6 columns and add them to the table's Columns collection.
            int numberOfColumns = 5;
            for (int x = 0; x < numberOfColumns; x++)
            {
                table1.Columns.Add(new TableColumn());

                // Set alternating background colors for the middle colums.
                if (x % 2 == 0)
                    table1.Columns[x].Background = Brushes.Beige;
                else
                    table1.Columns[x].Background = Brushes.LightSteelBlue;
            }

            // Create and add an empty TableRowGroup to hold the table's Rows.
            table1.RowGroups.Add(new TableRowGroup());

            // Add the first (title) row.
            table1.RowGroups[0].Rows.Add(new TableRow());

            // Alias the current working row for easy reference.
            TableRow currentRow = table1.RowGroups[0].Rows[0];

            // Global formatting for the title row.
            currentRow.Background = Brushes.Silver;
            currentRow.FontSize = 40;
            currentRow.FontWeight = System.Windows.FontWeights.Bold;

            // Add the header row with content,
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("2004 Sales Project"))));
            // and set the row to span all 6 columns.
            currentRow.Cells[0].ColumnSpan = 6;

            // Add the second (header) row.
            table1.RowGroups[0].Rows.Add(new TableRow());
            currentRow = table1.RowGroups[0].Rows[1];

            // Global formatting for the header row.
            currentRow.FontSize = 18;
            currentRow.FontWeight = FontWeights.Bold;

            // Add cells with content to the second row.
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Product"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Quarter 1"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Quarter 2"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Quarter 3"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Quarter 4"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("TOTAL"))));

            // Add the third row.
            table1.RowGroups[0].Rows.Add(new TableRow());
            currentRow = table1.RowGroups[0].Rows[2];

            // Global formatting for the row.
            currentRow.FontSize = 12;
            currentRow.FontWeight = FontWeights.Normal;

            // Add cells with content to the third row.
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Widgets"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("$50,000"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("$55,000"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("$60,000"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("$65,000"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("$230,000"))));

            // Bold the first cell.
            currentRow.Cells[0].FontWeight = FontWeights.Bold;

            table1.RowGroups[0].Rows.Add(new TableRow());
            currentRow = table1.RowGroups[0].Rows[3];

            // Global formatting for the footer row.
            currentRow.Background = Brushes.LightGray;
            currentRow.FontSize = 18;
            currentRow.FontWeight = System.Windows.FontWeights.Normal;

            // Add the header row with content,
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Projected 2004 Revenue: $810,000"))));
            // and set the row to span all 6 columns.
            currentRow.Cells[0].ColumnSpan = 6;


            // ...and add it to the FlowDocument Blocks collection.
            var flowDoc = new FlowDocument();
            flowDoc.Blocks.Add(table1);

            return flowDoc;
        }
    }
}
