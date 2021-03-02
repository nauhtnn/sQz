using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using sQzLib;

namespace sQzServer0
{
    public class Op0SlotView: TabItem
    {
        TabControl tbcQ;
        //TabControl tbcQAlt;
        public SortedList<string, TextBlock> vGrade;
        public SortedList<string, TextBlock> vDt1;
        public SortedList<string, TextBlock> vDt2;
        public SortedList<string, TextBlock> vComp;
        public Dictionary<int, TextBlock> vRT1;
        public Dictionary<int, TextBlock> vRT2;
        Dictionary<int, TextBlock> vRPw;
        Grid grdNee;
        public ExamSlotS0 mSl;

        public Op0SlotView()
        {
            Init();
        }

        public Op0SlotView(ExamSlotS0 sl)
        {
            Init();
            //
            mSl = sl;
            Header = mSl.Dt.ToString(DT._);
            Name = DT.CreateNameFromDateTime(Header as string);
        }

        void Init()
        {
            vGrade = new SortedList<string, TextBlock>();
            vDt1 = new SortedList<string, TextBlock>();
            vDt2 = new SortedList<string, TextBlock>();
            vComp = new SortedList<string, TextBlock>();
        }

        public void ShowExaminee()
        {
            SolidColorBrush evenbg = Theme.s._[(int)BrushId.BG];
            SolidColorBrush oddbg = Theme.s._[(int)BrushId.Q_BG];
            SolidColorBrush bg;
            bool even = false;
            int rid = -1;
            GridLength rh = new GridLength(26);
            foreach (ExamRoomS0 r in mSl.Rooms.Values)
                foreach (ExamineeS0 e in r.Examinees.Values)
                {
                    rid++;
                    if (even)
                        bg = evenbg;
                    else
                        bg = oddbg;
                    even = !even;
                    RowDefinition rd = new RowDefinition();
                    rd.Height = rh;
                    grdNee.RowDefinitions.Add(rd);
                    //
                    TextBlock t = new TextBlock();
                    t.Text = e.ID;
                    t.Background = bg;
                    Grid.SetRow(t, rid);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.Name;
                    t.Background = bg;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 1);
                    grdNee.Children.Add(t);
                    //
                    t = new TextBlock();
                    t.Text = e.Birthdate;
                    t.Background = bg;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 2);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.Birthplace;
                    t.Background = bg;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 3);
                    grdNee.Children.Add(t);
                    //
                    t = new TextBlock();
                    t.Background = bg;
                    t.Text = r.uId.ToString();
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 4);
                    grdNee.Children.Add(t);
                    //
                    t = new TextBlock();
                    t.Background = bg;
                    vGrade.Add(e.ID, t);
                    if (e.CorrectCount != ExamineeA.LV_CAP)
                        t.Text = e.Grade;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 5);
                    grdNee.Children.Add(t);
                    //
                    t = new TextBlock();
                    t.Background = bg;
                    vDt1.Add(e.ID, t);
                    if (e.dtTim1.Year != DT.INV)
                        t.Text = e.dtTim1.ToString(DT.hh);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 6);
                    grdNee.Children.Add(t);
                    //
                    t = new TextBlock();
                    t.Background = bg;
                    vDt2.Add(e.ID, t);
                    if (e.dtTim2.Year != DT.INV)
                        t.Text = e.dtTim2.ToString(DT.hh);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 7);
                    grdNee.Children.Add(t);
                    //
                    t = new TextBlock();
                    t.Background = bg;
                    vComp.Add(e.ID, t);
                    if (e.ComputerName != null)
                        t.Text = e.ComputerName;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 8);
                    grdNee.Children.Add(t);
                }
        }

        public void UpdateRsView(int rid)
        {
            ExamRoomS0 r;
            if (!mSl.Rooms.TryGetValue(rid, out r))
                return;
            if (vRT2.ContainsKey(rid))
                vRT2[rid].Text = DateTime.Now.ToString(DT.hh);
            foreach (ExamineeS0 e in r.Examinees.Values)
                if(e.bToVw)
                {
                    e.bToVw = false;
                    TextBlock t;
                    if (e.CorrectCount != ExamineeA.LV_CAP && vGrade.TryGetValue(e.ID, out t))
                        t.Text = e.CorrectCount.ToString();
                    if (e.dtTim1.Hour != DT.INV && vDt1.TryGetValue(e.ID, out t))
                        t.Text = e.dtTim1.ToString(DT.hh);
                    if (e.dtTim2.Hour != DT.INV && vDt2.TryGetValue(e.ID, out t))
                        t.Text = e.dtTim2.ToString(DT.hh);
                    if (e.ComputerName != null && vComp.TryGetValue(e.ID, out t))
                        t.Text = e.ComputerName;
                }
        }

        public void DeepCopy(TabControl refTbc)
        {
            TabControl tbc = new TabControl();
            foreach(TabItem tbi in refTbc.Items)
            {
                if (tbi.Name == "tbiStat")
                    tbc.Items.Add(DeepCopyStat(tbi));
                else
                    tbc.Items.Add(DeepCopyNee(tbi));
            }
            tbcQ = new TabControl();
            tbcQ.Width = refTbc.Width;
            TabItem i = new TabItem();
            i.Header = Txt.s._((int)TxI.OP_Q_PRI);
            i.Content = tbcQ;
            tbc.Items.Add(i);
            //tbcQAlt = new TabControl();
            //tbcQAlt.Width = refTbc.Width;
            //i = new TabItem();
            //i.Header = Txt.s._((int)TxI.OP_Q_ALT);
            //i.Content = tbcQAlt;
            //tbc.Items.Add(i);
            Content = tbc;
        }

        TabItem DeepCopyStat(TabItem refTbi)
        {
            Grid refg = refTbi.Content as Grid;
            if (refg == null)
                return new TabItem();
            Grid g = new Grid();
            g.ShowGridLines = true;
            foreach (ColumnDefinition cd in refg.ColumnDefinitions)
            {
                ColumnDefinition d = new ColumnDefinition();
                d.Width = cd.Width;
                g.ColumnDefinitions.Add(d);
            }
            foreach (RowDefinition rd in refg.RowDefinitions)
            {
                RowDefinition d = new RowDefinition();
                d.Height = rd.Height;
                g.RowDefinitions.Add(d);
            }
            foreach (TextBlock txt in refg.Children)
            {
                TextBlock t = new TextBlock();
                t.Text = txt.Text;
                t.Background = txt.Background;
                t.Foreground = txt.Foreground;
                t.TextAlignment = txt.TextAlignment;
                Grid.SetColumn(t, Grid.GetColumn(txt));
                Grid.SetColumnSpan(t, Grid.GetColumnSpan(txt));
                Grid.SetRow(t, Grid.GetRow(txt));
                g.Children.Add(t);
            }
            //
            vRT1 = new Dictionary<int, TextBlock>();
            vRT2 = new Dictionary<int, TextBlock>();
            vRPw = new Dictionary<int, TextBlock>();
            GridLength h = new GridLength(40);
            int i = 1;
            SolidColorBrush br = new SolidColorBrush(Colors.Black);
            Thickness th = new Thickness(0, 0, 0, 1);
            foreach (ExamRoomS0 r in mSl.Rooms.Values)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = h;
                g.RowDefinitions.Add(rd);
                Border bor = new Border();
                bor.BorderBrush = br;
                bor.BorderThickness = th;
                Grid.SetColumnSpan(bor, g.ColumnDefinitions.Count);
                Grid.SetRow(bor, i);
                g.Children.Add(bor);

                TextBlock t = new TextBlock();
                t.TextAlignment = TextAlignment.Center;
                t.Text = r.uId.ToString();
                Grid.SetRow(t, ++i);
                Grid.SetColumn(t, 0);
                g.Children.Add(t);

                t = new TextBlock();
                t.TextAlignment = TextAlignment.Center;
                t.Text = r.Examinees.Count.ToString();
                Grid.SetRow(t, i);
                Grid.SetColumn(t, 1);
                g.Children.Add(t);

                t = new TextBlock();
                vRT1.Add(r.uId, t);
                if (r.t1.Hour != DT.INV)
                    t.Text = r.t1.ToString(DT.hh);
                t.TextAlignment = TextAlignment.Center;
                Grid.SetRow(t, i);
                Grid.SetColumn(t, 2);
                g.Children.Add(t);

                t = new TextBlock();
                vRT2.Add(r.uId, t);
                if (r.t2.Hour != DT.INV)
                    t.Text = r.t2.ToString(DT.hh);
                t.TextAlignment = TextAlignment.Center;
                Grid.SetRow(t, i);
                Grid.SetColumn(t, 3);
                g.Children.Add(t);

                t = new TextBlock();
                vRPw.Add(r.uId, t);
                t.Text = r.tPw;
                Grid.SetRow(t, i);
                Grid.SetColumn(t, 6);
                g.Children.Add(t);

                Button btn = new Button();
                btn.Content = Txt.s._((int)TxI.OP_GEN_PW);
                btn.Name = "b" + r.uId;
                btn.Click += btnGenPw_Click;
                btn.Background = Theme.s._[(int)BrushId.Button_Hover];
                btn.Foreground = Theme.s._[(int)BrushId.QID_Color];
                Grid.SetRow(btn, i);
                Grid.SetColumn(btn, 7);
                g.Children.Add(btn);
            }
            //
            TabItem tbi = new TabItem();
            tbi.Content = g;
            tbi.Header = Txt.s._((int)TxI.OP_STT);
            return tbi;
        }

        private void btnGenPw_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int rid = int.Parse(btn.Name.Substring(1));
            if(mSl.Rooms.ContainsKey(rid) && vRPw.ContainsKey(rid)
                && !mSl.Rooms[rid].RegenPw())
                    vRPw[rid].Text = mSl.Rooms[rid].tPw;
        }

        TabItem DeepCopyNee(TabItem refTbi)
        {
            StackPanel refSp = refTbi.Content as StackPanel;
            if (refSp == null)
                return new TabItem();
            StackPanel sp = new StackPanel();
            Grid g = null;
            foreach (Grid refg in refSp.Children.OfType<Grid>())
            {
                g = new Grid();
                foreach (ColumnDefinition cd in refg.ColumnDefinitions)
                {
                    ColumnDefinition d = new ColumnDefinition();
                    d.Width = cd.Width;
                    g.ColumnDefinitions.Add(d);
                }
                foreach (TextBlock txt in refg.Children)
                {
                    TextBlock t = new TextBlock();
                    t.Text = txt.Text;
                    t.Background = txt.Background;
                    t.Foreground = txt.Foreground;
                    Grid.SetColumn(t, Grid.GetColumn(txt));
                    Grid.SetRow(t, Grid.GetRow(txt));
                    g.Children.Add(t);
                }
                sp.Children.Add(g);
            }

            if (g == null)
                return new TabItem();

            foreach (ScrollViewer refscrvwr in refSp.Children.OfType<ScrollViewer>())
            {
                ScrollViewer vwr = new ScrollViewer();
                vwr.Width = refscrvwr.Width;
                vwr.Height = refscrvwr.Height;
                vwr.HorizontalAlignment = HorizontalAlignment.Left;
                grdNee = new Grid();
                foreach (ColumnDefinition cd in g.ColumnDefinitions)
                {
                    ColumnDefinition d = new ColumnDefinition();
                    d.Width = cd.Width;
                    grdNee.ColumnDefinitions.Add(d);
                }
                vwr.Content = grdNee;
                sp.Children.Add(vwr);
            }
            TabItem tbi = new TabItem();
            tbi.Content = sp;
            tbi.Header = Txt.s._((int)TxI.OP_NEE);
            return tbi;
        }

        private void tbiQ_GotFocus(object sender, RoutedEventArgs e)
        {;
            TabItem tbi = sender as TabItem;
            if (tbi == null || tbi.Content != null)
                return;
            TabControl tbc = tbi.Parent as TabControl;
            if (tbc == null)
                return;
            int id = int.Parse((tbi.Header as TextBlock).Text);
            QuestSheet qs = null;
            if (mSl.QuestionPack.vSheet.ContainsKey(id))
                qs = mSl.QuestionPack.vSheet[id];
            if (qs == null)
                return;
            ScrollViewer svwr = new ScrollViewer();
            svwr.Content = new QuestionSheetView(qs, null, FontSize * 2, tbc.Width - FontSize * 2 - SystemParameters.ScrollWidth);
            svwr.Height = 560;
            tbi.Content = svwr;
        }

        public void GenQ()
        {
            mSl.GenQ(mSl.CountQSByRoom());

            ShowQSHeader();
        }

        public void ShowQSHeader()
        {
            tbcQ.Items.Clear();
            foreach (QuestSheet qs in mSl.QuestionPack.vSheet.Values)
            {
                TabItem ti = new TabItem();
                TextBlock t = new TextBlock();
                t.Text = qs.ID.ToString("d3");
                t.FontSize = 12;
                ti.Header = t;
                ti.GotFocus += tbiQ_GotFocus;
                        
                tbcQ.Items.Add(ti);
            }
            if (0 < tbcQ.Items.Count)
                tbiQ_GotFocus(tbcQ.Items[0], null);
        }

        public void UpRT1(int rid)
        {
            if (vRT1.ContainsKey(rid))
                vRT1[rid].Text = DateTime.Now.ToString(DT.hh);
            string emsg;
            mSl.DBUpT1(rid, out emsg);
        }
    }
}
