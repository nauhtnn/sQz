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
        ExamSlotS1 Slot;
        int uRId;//todo change to enum
        List<SortedList<string, bool>> vfbLock;

        public Operation1()
        {
            InitializeComponent();

            mState = NetCode.Srvr1DatRetriving;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, true);
            mServer = new Server2(SrvrBufHndl);
            mServer.SrvrPort = 23821;
            mCbMsg = new UICbMsg();
            bRunning = true;

            Slot = new ExamSlotS1();

            if(!System.IO.File.Exists("Room.txt") ||
                !int.TryParse(System.IO.File.ReadAllText("Room.txt"), out uRId))
                uRId = 0;

            vfbLock = new List<SortedList<string, bool>>();

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
            WPopup.s.ShowDialog(Txt.s._((int)TxI.OP1_EXIT_CAUT),
                Txt.s._((int)TxI.EXIT), Txt.s._((int)TxI.BTN_CNCL), null);
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
            ExamineeS1 e;
            DateTime dt;
            switch (c)
            {
                case NetCode.Dating:
                    outMsg = DT.GetBytes(Slot.Dt);
                    return true;
                case NetCode.Authenticating:
                    e = new ExamineeS1();
                    e.ReadBytes_FromClient(buf, ref offs);
                    bool lck = true;
                    bool found = false;
                    foreach (SortedList<string, bool> l in vfbLock)
                        if (l.TryGetValue(e.ID, out lck))
                        {
                            found = true;
                            break;
                        }
                    if (!found)
                        lck = false;
                    if (!lck)
                    {
                        ExamineeS1 o = null;
                        dt = DateTime.Now;
                        if ((o = Slot.Signin(e)) == null)//why? check with the old
                        {
                            dt = Slot.Dt;
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
                                    if (vw.vComp.TryGetValue(o.ID, out t))
                                        t.Text = o.ComputerName;
                                    if (vw.vDt1.TryGetValue(o.ID, out t))
                                        t.Text = o.dtTim1.ToString("HH:mm");
                                    CheckBox cbx;
                                    if (vw.vLock.TryGetValue(o.ID, out cbx))
                                    {
                                        cbx.IsChecked = true;
                                        cbx.IsEnabled = true;
                                    }
                                    if (vw.vbLock.Keys.Contains(o.ID))
                                        vw.vbLock[o.ID] = true;
                                }
                            });
                            byte[] a = o.GetBytes_SendingToClient();
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
                        if ((o = Slot.Find(e.ID)) != null)
                            break;
                        if (o == null)
                            o = new ExamineeC();
                        if (o.ComputerName == null)
                            outMsg = new byte[16];
                        else
                            outMsg = new byte[16 + o.ComputerName.Length];
                        Buffer.BlockCopy(BitConverter.GetBytes((int)TxI.SIGNIN_AL), 0, outMsg, 0, 4);
                        if (o.ComputerName == null)
                        {
                            Buffer.BlockCopy(BitConverter.GetBytes(0), 0, outMsg, 4, 4);
                            offs = 8;
                        }
                        else
                        {
                            byte[] comp = Encoding.UTF8.GetBytes(o.ComputerName);
                            Buffer.BlockCopy(BitConverter.GetBytes(comp.Length), 0, outMsg, 4, 4);
                            offs = 8;
                            Buffer.BlockCopy(comp, 0, outMsg, offs, o.ComputerName.Length);
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
                    string nee_id = Utils.ReadBytesOfString(buf, ref offs);
                    bool nee_not_found = true;
                    foreach(ExamRoomS1 r in Slot.Rooms.Values)
                        if(r.Examinees.ContainsKey(nee_id))
                        {
                            nee_not_found = false;
                            break;
                        }
                    if(nee_not_found)
                    {
                        outMsg = new byte[4];
                        Array.Copy(BitConverter.GetBytes((int)TxI.NEEID_NF), 0, outMsg, 0, 4);
                        break;
                    }
                    int qsid = BitConverter.ToInt32(buf, offs);
                    if (qsid == ExamineeA.LV_CAP)
                    {
                        byte[] a = Slot.GetBytes_NextQSheet();
                        if (a != null)
                        {
                            outMsg = new byte[a.Length + 4];
                            Array.Copy(BitConverter.GetBytes(0), outMsg, 4);
                            Array.Copy(a, 0, outMsg, 4, a.Length);
                        }
                    }
                    else if (Slot.QuestionPack.vSheet.TryGetValue(qsid, out qs))
                    {
                        outMsg = new byte[qs.aQuest.Length + 4];
                        Array.Copy(BitConverter.GetBytes(0), outMsg, 4);
                        Array.Copy(qs.aQuest, 0, outMsg, 4, qs.aQuest.Length);
                    }
                    if (outMsg == null)
                    {
                        mCbMsg += Txt.s._((int)TxI.QS_NFOUND) + (qsid);
                        outMsg = new byte[8];
                        Array.Copy(BitConverter.GetBytes((int)TxI.QS_NFOUND), 0, outMsg, 0, 4);
                        Array.Copy(BitConverter.GetBytes(qsid), 0, outMsg, 4, 4);
                    }
                    break;
                case NetCode.Submiting:
                    e = new ExamineeS1();
                    if (!e.ReadBytes_FromClient(buf, ref offs))
                    {
                        AnsSheet keySh = null;
                        found = false;
                        if(Slot.mKeyPack.vSheet.TryGetValue(e.AnswerSheet.questSheetID, out keySh))
                        {
                            found = true;
                            break;
                        }
                        if (!found)
                        {
                            outMsg = BitConverter.GetBytes(101);//todo
                            break;
                        }
                        ExamineeS1 o = null;
                        found = false;
                        if ((o = Slot.Find(e.ID)) != null)
                            break;
                        if (o != null)
                        {
                            o.eStt = NeeStt.Finished;
                            o.AnswerSheet = e.AnswerSheet;
                            o.Grade = keySh.Grade(e.AnswerSheet.aAns);
                            o.dtTim2 = DateTime.Now;
                            foreach (SortedList<string, bool> sl in vfbLock)
                                if (sl.ContainsKey(e.ID))
                                    sl[e.ID] = true;
                            Dispatcher.InvokeAsync(() =>
                            {
                                bool toSubm = true;
                                foreach (Op1SlotView vw in tbcSl.Items.OfType<Op1SlotView>())
                                {
                                    TextBlock t = null;
                                    if (vw.vDt2.TryGetValue(e.ID, out t))
                                        t.Text = o.dtTim2.ToString("HH:mm");
                                    if (vw.vMark.TryGetValue(e.ID, out t))
                                        t.Text = o.Grade.ToString();
                                    CheckBox cbx;
                                    if (vw.vLock.TryGetValue(e.ID, out cbx))
                                    {
                                        cbx.IsChecked = true;
                                        cbx.IsEnabled = false;
                                    }
                                    if(vw.vAbsen.TryGetValue(e.ID, out cbx))
                                        cbx.IsChecked = cbx.IsEnabled = false;
                                    if (!vw.ToSubmit())
                                        toSubm = false;
                                }
                                if (toSubm)
                                    ToSubmit(true);
                            });
                            byte[] a = o.GetBytes_SendingToClient();
                            outMsg = new byte[4 + a.Length];
                            Buffer.BlockCopy(BitConverter.GetBytes(0), 0, outMsg, 0, 4);
                            Buffer.BlockCopy(a, 0, outMsg, 4, a.Length);
                        }
                        else
                        {
                            mCbMsg += Txt.s._((int)TxI.NEEID_NF) + ' ' + e.ID;
                            outMsg = BitConverter.GetBytes((int)TxI.NEEID_NF);
                        }
                    }
                    else
                    {
                        mCbMsg += Txt.s._((int)TxI.RECV_DAT_ER);
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
            int offs = 0;
            switch (mState)
            {
                case NetCode.Srvr1DatRetriving:
                    if (Slot.ReadBytes_FromS0(buf, ref offs))
                    {
                        Dispatcher.InvokeAsync(() =>
                            WPopup.s.ShowDialog(Txt.s._((int)TxI.OP1_DT_NOK)));
                        break;
                    }
                    Dispatcher.InvokeAsync(() => LoadSl());
                    mState = NetCode.QuestRetrieving;
                    return true;
                case NetCode.QuestRetrieving:
                    if(Slot.Dt != DT.ReadByte(buf, ref offs))
                    {
                        Dispatcher.InvokeAsync(() =>
                            WPopup.s.ShowDialog("QuestRetrieving: Date time not match!"));
                        break;
                    }
                    if (Slot.ReadBytes_QPacksNoDateTime(buf, ref offs))
                    {
                        Dispatcher.InvokeAsync(() => 
                            WPopup.s.ShowDialog(Txt.s._((int)TxI.OP1_Q_NOK)));
                        break;
                    }
                    mState = NetCode.AnsKeyRetrieving;
                    return true;
                case NetCode.AnsKeyRetrieving:
                    if (Slot.Dt != DT.ReadByte(buf, ref offs))
                    {
                        Dispatcher.InvokeAsync(() =>
                            WPopup.s.ShowDialog("AnsKeyRetrieving: Date time not match!"));
                        break;
                    }
                    if (Slot.ReadByteKey_NoDateTime(buf, ref offs))
                    {
                        Dispatcher.InvokeAsync(() =>
                            WPopup.s.ShowDialog(Txt.s._((int)TxI.OP1_KEY_NOK)));
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
                    List<byte[]> bytes = new List<byte[]>();
                    bytes.Add(BitConverter.GetBytes((int)NetCode.SrvrSubmitting));
                    bytes.InsertRange(bytes.Count, Slot.GetBytes_RoomSendingToS0());
                    outMsg = Utils.ToArray_FromListOfBytes(bytes);
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
            btnClose.Content = s._((int)TxI.EXIT);
            btnConn.Content = s._((int)TxI.CONN);
            btnStrt.Content = s._((int)TxI.STRT_SRVR);
            btnStop.Content = s._((int)TxI.STOP_SRVR);
            btnSubmit.Content = s._((int)TxI.SUBMIT);

            txtId.Text = s._((int)TxI.NEEID_S);
            txtName.Text = s._((int)TxI.NEE_NAME);
            txtBirdate.Text = s._((int)TxI.BIRDATE);
            txtBirpl.Text = s._((int)TxI.BIRPL);
            txtComp.Text = s._((int)TxI.COMP);
            txtT1.Text = s._((int)TxI.T1);
            txtT2.Text = s._((int)TxI.T2);
            txtGrade.Text = s._((int)TxI.MARK);
            txtLock.Text = s._((int)TxI.OP_LCK);
            txtAbsence.Text = s._((int)TxI.OP_ABSENCE);
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
            //todo: check Slot with i.Content

            Op1SlotView vw = new Op1SlotView();
            vw.mSl = Slot;
            vw.DeepCopyNee(tbiRefNee);
            vw.ShowExaminee();
            vfbLock.Add(vw.vbLock);
            vw.Name = "_" + (i.Content as string).Replace(':', '_');
            vw.Header = Slot.Dt.ToString(DT.hh);
            vw.toSubmCb = ToSubmit;
            tbcSl.Items.Add(vw);
            vw.Focus();
        }

        private void lbxSl_Unselected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            //mBrd.vSl.Remove(i.Content as string);
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
            List<DateTime> v = new List<DateTime>();
            v.Add(Slot.Dt);
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

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
                // where did you get this file name?
        string filePath = System.IO.Directory.GetCurrentDirectory() +
                "/sqz_server1_";

            if (!System.IO.File.Exists(filePath + "template.docx"))
            {
                MessageBox.Show("No template!");
                return;
            }

            int i;
            for (i = 0; i < 100; ++i)
                if (!System.IO.File.Exists(filePath + i + ".docx"))
                    break;
            if(i == 100)
            {
                MessageBox.Show("Out of index to print. 99 slots have been taken!");
                return;
            }
            var word = new Microsoft.Office.Interop.Word.Application { Visible = true };
            var doc = word.Documents.Open(filePath + "template.docx", ReadOnly: true, Visible: true);
            doc.SaveAs2(filePath + i + ".docx");
            DocxReplaceDate(doc);
        }

        private void DocxReplaceDate(Microsoft.Office.Interop.Word.Document doc)
        {
            foreach(Microsoft.Office.Interop.Word.Range i in doc.Words)
            {
                if(i.Text.Contains("DDMMYYYY"))
                {
                    i.Text = i.Text.Replace("DDMMYYYY", DateTime.Now.ToString("dd/MM/yyyy"));
                    return;
                }
            }
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
