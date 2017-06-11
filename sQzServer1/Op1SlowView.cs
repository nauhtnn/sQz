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

namespace sQzServer1
{
    public class Op1SlotView : StackPanel
    {
        public Dictionary<int, TextBlock> vGrade;
        public Dictionary<int, TextBlock> vDt1;
        public Dictionary<int, TextBlock> vDt2;
        public Dictionary<int, TextBlock> vMark;
        public Dictionary<int, TextBlock> vComp;
        public SortedList<int, CheckBox> vLock;
        public SortedList<int, bool> vbLock;
        Grid grdNee;
        public TabControl tbcQuest;
        public ExamSlot mSl;
        bool bQShowed;
        bool bNeeShowed;

        public Op1SlotView()
        {
            vComp = new Dictionary<int, TextBlock>();
            vDt1 = new Dictionary<int, TextBlock>();
            vDt2 = new Dictionary<int, TextBlock>();
            vMark = new Dictionary<int, TextBlock>();
            vLock = new SortedList<int, CheckBox>();
            vbLock = new SortedList<int, bool>();
            bQShowed = bNeeShowed = false;
        }

        public void ShowExaminee()
        {
            if (bNeeShowed)
                return;
            bNeeShowed = true;

            vComp.Clear();
            vMark.Clear();
            vDt1.Clear();
            vDt2.Clear();
            int rid = -1;
            foreach (ExamRoom r in mSl.vRoom.Values)
                foreach (ExamineeA e in r.vExaminee.Values)
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(20);
                    grdNee.RowDefinitions.Add(rd);
                    TextBlock t = new TextBlock();
                    t.Text = e.tId;
                    Grid.SetRow(t, ++rid);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tName;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 1);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirdate;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 2);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    int lvid = e.mLv + e.uId;
                    vComp.Add(lvid, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 3);
                    grdNee.Children.Add(t);
                    CheckBox cbx = new CheckBox();
                    cbx.Name = "_" + lvid;
                    cbx.Unchecked += cbxLock_Unchecked;
                    cbx.Checked += cbxLock_Checked;
                    cbx.IsEnabled = true;//default value empowers supervisors
                    Grid.SetRow(cbx, rid);
                    Grid.SetColumn(cbx, 7);
                    vLock.Add(lvid, cbx);
                    grdNee.Children.Add(cbx);
                    t = new TextBlock();
                    if (e.dtTim1.Hour != DT.INV)
                    {
                        t.Text = e.dtTim1.ToString("HH:mm");
                        vbLock.Add(lvid, true);
                    }
                    else
                    {
                        vbLock.Add(lvid, false);
                        cbx.IsEnabled = false;
                    }
                    vDt1.Add(lvid, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 4);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (e.dtTim2.Hour != DT.INV)
                        t.Text = e.dtTim2.ToString("HH:mm");
                    vDt2.Add(lvid, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 5);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (e.uGrade != ushort.MaxValue)
                    {
                        t.Text = e.uGrade.ToString();
                        cbx.IsEnabled = false;
                    }
                    vMark.Add(lvid, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 6);
                    grdNee.Children.Add(t);
                }
        }

        public void UpdateRsView(List<ExamRoom> vRoom)
        {
            TextBlock t;
            foreach (ExamRoom r in vRoom)
                foreach (ExamineeA e in r.vExaminee.Values)
                {
                    if (e.uGrade != ushort.MaxValue && vGrade.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.uGrade.ToString();
                    if (e.dtTim1.Hour != DT.INV && vDt1.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.dtTim1.ToString("HH:mm");
                    if (e.dtTim2.Hour != DT.INV && vDt2.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.dtTim2.ToString("HH:mm");
                    if (e.tComp != null && vComp.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.tComp;
                }
        }

        public void ShallowCopy(StackPanel refSp)
        {
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
                Children.Add(g);
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
                grdNee.Name = refg.Name;
                vwr.Content = grdNee;
                Children.Add(vwr);
            }
            foreach (TabControl tbc in refSp.Children.OfType<TabControl>())
            {
                tbcQuest = new TabControl();
                tbcQuest.Width = tbc.Width;
                tbcQuest.Height = tbc.Height;
                Children.Add(tbcQuest);
            }
        }

        public void ShowQuest()
        {
            if (bQShowed)
                return;
            bQShowed = true;

            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            tbcQuest.Items.Clear();
            foreach (QuestPack p in mSl.vQPack.Values)
                foreach (QuestSheet qs in p.vSheet.Values)
                {
                    TabItem ti = new TabItem();
                    ti.Header = qs.eLv.ToString() + qs.uId;
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
        }

        private void cbxLock_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cbx = sender as CheckBox;
            int key;
            if (int.TryParse(cbx.Name.Substring(1), out key))
                vbLock[key] = false;
        }

        private void cbxLock_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cbx = sender as CheckBox;
            int key;
            if (int.TryParse(cbx.Name.Substring(1), out key))
                vbLock[key] = false;
        }
    }
}
