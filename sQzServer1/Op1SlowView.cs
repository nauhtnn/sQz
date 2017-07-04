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
    public class Op1SlotView : TabItem
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
            TabControl tbc = new TabControl();
            Content = tbc;
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
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(t, ++rid);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tName;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 1);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirdate;
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 2);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirthplace;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 3);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    if (e.tComp != null)
                        t.Text = e.tComp;
                    int lvid = e.LvId;
                    vComp.Add(lvid, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 4);
                    grdNee.Children.Add(t);
                    CheckBox cbx = new CheckBox();
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    cbx.Name = "_" + lvid;
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    cbx.Unchecked += cbxLock_Unchecked;
                    cbx.Checked += cbxLock_Checked;
                    cbx.IsEnabled = true;//default value empowers supervisors
                    Grid.SetRow(cbx, rid);
                    Grid.SetColumn(cbx, 8);
                    vLock.Add(lvid, cbx);
                    grdNee.Children.Add(cbx);
                    t = new TextBlock();
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    if (e.dtTim1.Hour != DT.INV)
                    {
                        t.Text = e.dtTim1.ToString("HH:mm");
                        vbLock.Add(lvid, true);
                        vLock[lvid].IsChecked = true;
                    }
                    else
                    {
                        vbLock.Add(lvid, false);
                        cbx.IsEnabled = false;
                    }
                    vDt1.Add(lvid, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 5);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    if (e.dtTim2.Hour != DT.INV)
                        t.Text = e.dtTim2.ToString("HH:mm");
                    vDt2.Add(lvid, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 6);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    if (e.uGrade != ExamineeA.LV_CAP)
                    {
                        t.Text = e.Grade;
                        cbx.IsEnabled = false;
                    }
                    vMark.Add(lvid, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 7);
                    grdNee.Children.Add(t);
                    //
                    cbx = new CheckBox();
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    cbx.Name = "b" + lvid;
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    //cbx.Unchecked += cbxLock_Unchecked;
                    //cbx.Checked += cbxLock_Checked;
                    cbx.IsEnabled = false;
                    Grid.SetRow(cbx, rid);
                    Grid.SetColumn(cbx, 9);
                    grdNee.Children.Add(cbx);
                }
        }

        public void UpdateRsView(List<ExamRoom> vRoom)
        {
            TextBlock t;
            foreach (ExamRoom r in vRoom)
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

        public TabItem DeepCopyNee(TabItem reftbi)
        {
            StackPanel refsp = reftbi.Content as StackPanel;
            StackPanel sp = new StackPanel();
            foreach (Grid refg in refsp.Children.OfType<Grid>())
            {
                Grid g = new Grid();
                g.ShowGridLines = true;
                foreach (ColumnDefinition cd in refg.ColumnDefinitions)
                {
                    ColumnDefinition d = new ColumnDefinition();
                    d.Width = cd.Width;
                    g.ColumnDefinitions.Add(d);
                }
                g.Width = refg.Width;
                g.HorizontalAlignment = refg.HorizontalAlignment;
                foreach (TextBlock txt in refg.Children)
                {
                    TextBlock t = new TextBlock();
                    t.Text = txt.Text;
                    t.Background = txt.Background;
                    t.Foreground = txt.Foreground;
                    t.HorizontalAlignment = txt.HorizontalAlignment;
                    Grid.SetColumn(t, Grid.GetColumn(txt));
                    Grid.SetRow(t, Grid.GetRow(txt));
                    g.Children.Add(t);
                }
                sp.Children.Add(g);
            }

            foreach (ScrollViewer refscrvwr in refsp.Children.OfType<ScrollViewer>())
            {
                ScrollViewer vwr = new ScrollViewer();
                Grid refg = refscrvwr.Content as Grid;
                if (refg == null)
                    continue;
                vwr.Width = refscrvwr.Width;
                vwr.Height = refscrvwr.Height;
                vwr.HorizontalAlignment = HorizontalAlignment.Left;
                grdNee = new Grid();
                grdNee.ShowGridLines = true;
                foreach (ColumnDefinition cd in refg.ColumnDefinitions)
                {
                    ColumnDefinition d = new ColumnDefinition();
                    d.Width = cd.Width;
                    grdNee.ColumnDefinitions.Add(d);
                }
                grdNee.Name = refg.Name;
                grdNee.HorizontalAlignment = refg.HorizontalAlignment;
                vwr.Content = grdNee;
                sp.Children.Add(vwr);
            }
            TabItem tbi = new TabItem();
            tbi.Content = sp;
            tbi.Header = Txt.s._[(int)TxI.OP_NEE];
            return tbi;
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
