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
        RadioButton rdoQ;
        public SortedList<int, TextBlock> vGrade;
        public SortedList<int, TextBlock> vDt1;
        public SortedList<int, TextBlock> vDt2;
        public SortedList<int, TextBlock> vComp;
        Grid grdNee;
        public ExamSlot mSl;
        Dictionary<ExamLv, int[]> vNEsyDif, vNDiff;
        bool bInitNMod;

        public Op0SlotView()
        {
            Init();
        }

        public Op0SlotView(ExamSlot sl)
        {
            Init();
            //
            mSl = sl;
            Header = mSl.Dt.ToString(DT.hh);
            Name = "_" + (Header as string).Replace(':', '_');
        }

        void Init()
        {
            vGrade = new SortedList<int, TextBlock>();
            vDt1 = new SortedList<int, TextBlock>();
            vDt2 = new SortedList<int, TextBlock>();
            vComp = new SortedList<int, TextBlock>();
            vNEsyDif = new Dictionary<ExamLv, int[]>();
            vNDiff = new Dictionary<ExamLv, int[]>();
            vNEsyDif.Add(ExamLv.A, null);
            vNDiff.Add(ExamLv.A, null);
            vNEsyDif.Add(ExamLv.B, null);
            vNDiff.Add(ExamLv.B, null);
            bInitNMod = false;
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
                    if (e.uGrade != ExamineeA.LV_CAP)
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
                    if (e.uGrade != ExamineeA.LV_CAP && vGrade.TryGetValue(lvid, out t))
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
            List<string> qstIds = mSl.vQPack[ExamLv.A].SelectQStId();
            qstIds.InsertRange(qstIds.Count, mSl.vQPack[ExamLv.B].SelectQStId());
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
                        rdo.Content = Txt.s._[(int)TxI.RDO_Q];
                        rdoQ = rdo;
                    }
                    else
                    {
                        rdo.Content = Txt.s._[(int)TxI.RDO_NEE];
                        rdo.IsChecked = true;
                    }
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
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            SolidColorBrush lightbg = new SolidColorBrush(c);
            SolidColorBrush bg = lightbg;
            foreach (Question q in qs.ShallowCopy())
            {
                TextBlock i = new TextBlock();
                i.Text = ++x + ". " + q.Stmt;
                if (bg == lightbg)
                    bg = Theme.s._[(int)BrushId.LeftPanel_BG];
                else
                    bg = lightbg;
                i.Background = bg;
                sp.Children.Add(i);
                for (int idx = 0; idx < Question.N_ANS; ++idx)
                {
                    TextBlock j = new TextBlock();
                    j.Text = ((char)('A' + idx)).ToString() + ") " + q.vAns[idx];
                    j.Background = bg;
                    if (q.vKeys[idx])
                        j.FontWeight = FontWeights.Bold;
                    sp.Children.Add(j);
                }
            }
            svwr.Content = sp;
            svwr.Height = 620;
            tbi.Content = svwr;

            InitNMod();
        }

        public void InitNMod()
        {
            if (bInitNMod)
                return;
            bInitNMod = true;
            foreach (QuestPack p in mSl.vQPack.Values)
            {
                List<int[]> l = p.GetNMod();
                if(l != null)
                {
                    vNEsyDif[p.eLv] = l[0];
                    vNDiff[p.eLv] = l[1];
                }
            }
        }

        public List<int[]> GetNMod(ExamLv lv)
        {
            if (vNEsyDif[lv] == null)
                return null;
            List<int[]> rv = new List<int[]>();
            rv.Add(vNEsyDif[lv]);
            rv.Add(vNDiff[lv]);
            return rv;
        }

        public void GenQ(ExamLv lv, int[] vnesydif, int[] vndiff)
        {
            vNEsyDif[lv] = vnesydif;
            vNDiff[lv] = vndiff;
            mSl.GenQ(mSl.CountQSByRoom(lv), lv, vnesydif);

            ShowQSHeader();
        }

        public void ShowQSHeader()
        {
            tbcQ.Items.Clear();
            foreach (QuestPack p in mSl.vQPack.Values)
                foreach (QuestSheet qs in p.vSheet.Values)
                {
                    TabItem ti = new TabItem();
                    ti.Header = qs.eLv.ToString() + qs.uId.ToString("d3");
                    ti.GotFocus += tbiQ_GotFocus;
                        
                    tbcQ.Items.Add(ti);
                }
            if (0 < tbcQ.Items.Count)
                tbiQ_GotFocus(tbcQ.Items[0], null);
        }
    }
}
