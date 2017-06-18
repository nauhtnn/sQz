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
        StackPanel spContent;
        StackPanel spNee;
        TabControl tbcQ;
        public SortedList<int, TextBlock> vGrade;
        public SortedList<int, TextBlock> vDt1;
        public SortedList<int, TextBlock> vDt2;
        public SortedList<int, TextBlock> vComp;
        Grid grdNee;
        public ExamSlot mSl;
        int[][] vNBoth, vNDiff;

        public Op0SlotView()
        {
            vGrade = new SortedList<int, TextBlock>();
            vDt1 = new SortedList<int, TextBlock>();
            vDt2 = new SortedList<int, TextBlock>();
            vComp = new SortedList<int, TextBlock>();
        }

        public Op0SlotView(ExamSlot sl)
        {
            vGrade = new SortedList<int, TextBlock>();
            vDt1 = new SortedList<int, TextBlock>();
            vDt2 = new SortedList<int, TextBlock>();
            vComp = new SortedList<int, TextBlock>();
            //
            mSl = sl;
            Header = mSl.Dt.ToString(DT.hh);
            Name = "_" + (Header as string).Replace(':', '_');
        }

        public void ShowExaminee()
        {
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            bool dark = false;
            int rid = -1;
            foreach (ExamRoom r in mSl.vRoom.Values)
                foreach (ExamineeA e in r.vExaminee.Values)
                {
                    rid++;
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(20);
                    grdNee.RowDefinitions.Add(rd);
                    TextBlock t = new TextBlock();
                    t.Text = e.tId;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tName;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 1);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirdate;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 2);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirthplace;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 3);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    int lvid = e.LvId;
                    vGrade.Add(lvid, t);
                    if (e.uGrade != ushort.MaxValue)
                        t.Text = e.Grade;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 4);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vDt1.Add(lvid, t);
                    if (e.dtTim1.Year != DT.INV)
                        t.Text = e.dtTim1.ToString("HH:mm");
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 5);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vDt2.Add(lvid, t);
                    if (e.dtTim2.Year != DT.INV)
                        t.Text = e.dtTim2.ToString("HH:mm");
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 6);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vComp.Add(lvid, t);
                    if (e.tComp != null)
                        t.Text = e.tComp;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 7);
                    grdNee.Children.Add(t);
                    dark = !dark;
                }
        }

        public void UpdateRsView()
        {
            TextBlock t;
            foreach (ExamRoom r in mSl.vRoom.Values)
                foreach (ExamineeA e in r.vExaminee.Values)
                {
                    int lvid = e.LvId;
                    if (e.uGrade != ushort.MaxValue && vGrade.TryGetValue(lvid, out t))
                        t.Text = e.Grade;
                    if (e.dtTim1.Hour != DT.INV && vDt1.TryGetValue(lvid, out t))
                        t.Text = e.dtTim1.ToString("HH:mm");
                    if (e.dtTim2.Hour != DT.INV && vDt2.TryGetValue(lvid, out t))
                        t.Text = e.dtTim2.ToString("HH:mm");
                    if (e.tComp != null && vComp.TryGetValue(lvid, out t))
                        t.Text = e.tComp;
                }
        }

        public void DeepCopy(TabItem refTbi)
        {
            spContent = new StackPanel();
            StackPanel refsp = refTbi.Content as StackPanel;

            foreach (StackPanel refp in refsp.Children.OfType<StackPanel>())
                DeepCopy(refp);

            tbcQ = new TabControl();
            List<string> qstIds = QuestPack.DBSelectQStId(mSl.mDt);
            foreach(string t in qstIds)
            {
                TabItem ti = new TabItem();
                ti.Header = t;
                tbcQ.Items.Add(ti);
            }

            foreach(Grid refg in refsp.Children.OfType<Grid>())
            {
                Grid g = new Grid();
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
                foreach (RadioButton refrdo in refg.Children.OfType<RadioButton>())
                {
                    RadioButton rdo = new RadioButton();
                    Grid.SetColumn(rdo, Grid.GetColumn(refrdo));
                    rdo.Name = refrdo.Name;
                    rdo.Checked += vwMode_Check;
                    if (rdo.Name == "Q")
                    {
                        rdo.IsChecked = true;
                        rdo.Content = Txt.s._[(int)TxI.RDO_Q];
                    }
                    else
                        rdo.Content = Txt.s._[(int)TxI.RDO_NEE];
                    g.Children.Add(rdo);
                }
                spContent.Children.Add(g);
            }

            spContent.Children.Add(tbcQ);
            spContent.Children.Add(spNee);
            Content = spContent;
        }

        void DeepCopy(StackPanel refSp)
        {
            spNee = new StackPanel();
            foreach (Grid refg in refSp.Children.OfType<Grid>())
            {
                Grid g = new Grid();
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
                spNee.Children.Add(g);
            }

            foreach (ScrollViewer refscrvwr in refSp.Children.OfType<ScrollViewer>())
            {
                ScrollViewer vwr = new ScrollViewer();
                Grid refg = refscrvwr.Content as Grid;
                if (refg == null)
                    continue;
                vwr.Width = refscrvwr.Width;
                vwr.Height = refscrvwr.Height;
                vwr.HorizontalAlignment = HorizontalAlignment.Left;
                grdNee = new Grid();
                foreach (ColumnDefinition cd in refg.ColumnDefinitions)
                {
                    ColumnDefinition d = new ColumnDefinition();
                    d.Width = cd.Width;
                    grdNee.ColumnDefinitions.Add(d);
                }
                vwr.Content = grdNee;
                spNee.Children.Add(vwr);
            }

            spNee.Visibility = Visibility.Collapsed;
            //InitQPanel();
        }

        private void vwMode_Check(object sender, RoutedEventArgs e)
        {
            RadioButton rdo = sender as RadioButton;
            if (rdo == null)
                return;
            if(rdo.Name == "Q")
            {
                spNee.Visibility = Visibility.Collapsed;
                tbcQ.Visibility = Visibility.Visible;
            }
            else
            {
                tbcQ.Visibility = Visibility.Collapsed;
                spNee.Visibility = Visibility.Visible;
            }
        }

        private void InitQTabItem()
        {
            tbcQ.Items.Clear();
            foreach(QuestSheet qs in mSl.vQPack[ExamLv.A].vSheet.Values)
            {
                TabItem i = new TabItem();
                i.Header = qs.tId;
                i.GotFocus += tbiQ_GotFocus;
                tbcQ.Items.Add(i);
            }
        }

        private void tbiQ_GotFocus(object sender, RoutedEventArgs e)
        {
            TabItem tbi = sender as TabItem;
            if (tbi == null || tbi.Content != null)
                return;
            ExamLv lv;
            int id;
            if (QuestSheet.ParseLvId(tbi.Header as string, out lv, out id))
                return;
            QuestSheet qs = mSl.vQPack[lv].vSheet[id];
            ScrollViewer svwr = new ScrollViewer();
            svwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            StackPanel sp = new StackPanel();
            int x = 0;
            bool dark = false;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            foreach (Question q in qs.vQuest)
            {
                TextBlock i = new TextBlock();
                i.Text = ++x + ". " + q.Stmt;
                dark = !dark;
                i.Background = (dark) ? new SolidColorBrush(c) :
                    Theme.s._[(int)BrushId.LeftPanel_BG];
                sp.Children.Add(i);
                for (int idx = 0; idx < q.nAns; ++idx)
                {
                    TextBlock j = new TextBlock();
                    j.Text = ('A' + idx).ToString() + q.vAns[idx];
                    if (q.vKeys[idx])
                        j.FontWeight = FontWeights.Bold;
                    sp.Children.Add(j);
                }
            }
            svwr.Content = sp;
            tbi.Content = svwr;
        }

        public void SetNMod(bool basic)
        {
            vNBoth = new int[2][];
            vNDiff = new int[2][];
            vNBoth[0] = new int[6];
            vNDiff[0] = new int[6];
            vNBoth[1] = new int[3];
            vNDiff[1] = new int[3];
            if(0 < mSl.vQPack[ExamLv.A].vSheet.Count)
            {
                QuestSheet qs = mSl.vQPack[ExamLv.A].vSheet.Values.First();
                foreach(Question q in qs.vQuest)
                {
                    ++vNBoth[0][(int)q.mIU];
                    if(q.bDiff)
                        ++vNDiff[0][(int)q.mIU];
                }
            }
            int[] vnboth = new int[4];
            int[] vndiff = new int[4];
            if (0 < mSl.vQPack[ExamLv.A].vSheet.Count)
            {
                QuestSheet qs = mSl.vQPack[ExamLv.A].vSheet.Values.First();
                foreach (Question q in qs.vQuest)
                {
                    int idx = (int)q.mIU - (int)IUx._7;
                    ++vnboth[idx];
                    if (q.bDiff)
                        ++vndiff[idx];
                }
            }
            vNBoth[1][0] = vnboth[0];
            vNBoth[1][1] = vnboth[1];
            vNBoth[1][2] = vnboth[4];
            vNDiff[1][0] = vndiff[0];
            vNDiff[1][1] = vndiff[1];
            vNDiff[1][2] = vndiff[4];
        }

        public List<int[]> GetNMod(bool basic)
        {
            int idx = (basic) ? 0 : 1;
            List<int[]> rv = new List<int[]>();
            rv.Add(vNBoth[idx]);
            rv.Add(vNDiff[idx]);
            return rv;
        }

        private void GenQPack(bool basic, int[] vboth, int[] vdiff)
        {
            ExamLv lv;
            int idx;
            if (basic)
            {
                lv = ExamLv.A;
                idx = 0;
            }
            else
            {
                lv = ExamLv.B;
                idx = 1;
            }
            vNBoth[idx] = vboth;
            vNDiff[idx] = vdiff;
            mSl.GenQPack(mSl.CountQSByRoom(), lv, vboth);

            ShowQuest();
        }

        private void ShowQuest()
        {
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
                tbcQ.Items.Clear();
                foreach (QuestPack p in mSl.vQPack.Values)
                    foreach (QuestSheet qs in p.vSheet.Values)
                    {
                        TabItem ti = new TabItem();
                        ti.Header = qs.eLv.ToString() + qs.uId;
                        
                        tbcQ.Items.Add(ti);
                    }
            });
        }
    }
}
