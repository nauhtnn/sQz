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
    public delegate void ToSubmitCb(bool bEnable);
    public class Op1SlotView : TabItem
    {
        public Dictionary<string, TextBlock> vGrade;
        public Dictionary<string, TextBlock> vDt1;
        public Dictionary<string, TextBlock> vDt2;
        public Dictionary<string, TextBlock> vMark;
        public Dictionary<string, TextBlock> vComp;
        public SortedList<string, CheckBox> vLock;
        public SortedList<string, bool> vbLock;
        public SortedList<string, CheckBox> vAbsen;
        Grid grdNee;
        public ExamSlotA mSl;
        bool bQShowed;
        bool bNeeShowed;
        public ToSubmitCb toSubmCb;

        public Op1SlotView()
        {
            vComp = new Dictionary<string, TextBlock>();
            vDt1 = new Dictionary<string, TextBlock>();
            vDt2 = new Dictionary<string, TextBlock>();
            vMark = new Dictionary<string, TextBlock>();
            vLock = new SortedList<string, CheckBox>();
            vbLock = new SortedList<string, bool>();
            vAbsen = new SortedList<string, CheckBox>();
            bQShowed = bNeeShowed = false;
            TabControl tbc = new TabControl();
            Content = tbc;
            toSubmCb = null;
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
            foreach (ExamRoomA r in mSl.Rooms.Values)
                foreach (ExamineeA e in r.Examinees.Values)
                {
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(20);
                    grdNee.RowDefinitions.Add(rd);
                    TextBlock t = new TextBlock();
                    t.Text = e.ID;
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(t, ++rid);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.Name;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 1);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.Birthdate;
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 2);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.Birthplace;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 3);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    if (e.ComputerName != null)
                        t.Text = e.ComputerName;
                    vComp.Add(e.ID, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 4);
                    grdNee.Children.Add(t);
                    CheckBox cbx = new CheckBox();
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    cbx.Name = e.ID;
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    cbx.Unchecked += cbxLock_Unchecked;
                    cbx.Checked += cbxLock_Checked;
                    cbx.IsEnabled = true;//default value empowers supervisors
                    Grid.SetRow(cbx, rid);
                    Grid.SetColumn(cbx, 8);
                    vLock.Add(e.ID, cbx);
                    grdNee.Children.Add(cbx);
                    t = new TextBlock();
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    if (e.dtTim1.Hour != DT.INV)
                    {
                        t.Text = e.dtTim1.ToString("HH:mm");
                        vbLock.Add(e.ID, true);
                        cbx.IsChecked = true;
                    }
                    else
                    {
                        vbLock.Add(e.ID, false);
                        cbx.IsEnabled = false;
                    }
                    vDt1.Add(e.ID, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 5);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    if (e.dtTim2.Hour != DT.INV)
                        t.Text = e.dtTim2.ToString("HH:mm");
                    vDt2.Add(e.ID, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 6);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    if (e.Grade != ExamineeA.LV_CAP)
                    {
                        t.Text = e.Grade.ToString();
                        cbx.IsEnabled = false;
                    }
                    vMark.Add(e.ID, t);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 7);
                    grdNee.Children.Add(t);
                    //
                    cbx = new CheckBox();
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    cbx.Name = "b" + e.ID;
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    cbx.Unchecked += cbxAbsen_Unchecked;
                    cbx.Checked += cbxAbsen_Checked;
                    if (e.eStt == NeeStt.Finished)
                        cbx.IsEnabled = false;
                    Grid.SetRow(cbx, rid);
                    Grid.SetColumn(cbx, 9);
                    vAbsen.Add(e.ID, cbx);
                    grdNee.Children.Add(cbx);
                }
        }

        public void UpdateRsView(List<ExamRoomA> vRoom)
        {
            TextBlock t;
            foreach (ExamRoomA r in vRoom)
                foreach (ExamineeA e in r.Examinees.Values)
                {
                    if (e.Grade != ExamineeA.LV_CAP && vGrade.TryGetValue(e.ID, out t))
                        t.Text = e.Grade.ToString();
                    if (e.dtTim1.Hour != DT.INV && vDt1.TryGetValue(e.ID, out t))
                        t.Text = e.dtTim1.ToString("HH:mm");
                    if (e.dtTim2.Hour != DT.INV && vDt2.TryGetValue(e.ID, out t))
                        t.Text = e.dtTim2.ToString("HH:mm");
                    if (e.ComputerName != null && vComp.TryGetValue(e.ID, out t))
                        t.Text = e.ComputerName;
                }
        }

        public void DeepCopyNee(TabItem reftbi)
        {
            StackPanel refsp = reftbi.Content as StackPanel;
            StackPanel sp = new StackPanel();
            Grid g = null;
            foreach (Grid refg in refsp.Children.OfType<Grid>())
            {
                g = new Grid();
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

            if (g == null)
                return;

            foreach (ScrollViewer refscrvwr in refsp.Children.OfType<ScrollViewer>())
            {
                ScrollViewer vwr = new ScrollViewer();
                vwr.Width = refscrvwr.Width;
                vwr.Height = refscrvwr.Height;
                vwr.HorizontalAlignment = HorizontalAlignment.Left;
                grdNee = new Grid();
                grdNee.ShowGridLines = true;
                foreach (ColumnDefinition cd in g.ColumnDefinitions)
                {
                    ColumnDefinition d = new ColumnDefinition();
                    d.Width = cd.Width;
                    grdNee.ColumnDefinitions.Add(d);
                }
                grdNee.HorizontalAlignment = g.HorizontalAlignment;
                vwr.Content = grdNee;
                sp.Children.Add(vwr);
            }
            Content = sp;
        }

        private void cbxLock_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cbx = sender as CheckBox;
            if (vbLock.ContainsKey(cbx.Name))
                vbLock[cbx.Name] = false;
        }

        private void cbxLock_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cbx = sender as CheckBox;
            if (vbLock.ContainsKey(cbx.Name))
                vbLock[cbx.Name] = true;
        }

        private void cbxAbsen_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cbx = sender as CheckBox;
            ExamineeA nee;
            foreach (ExamRoomA r in mSl.Rooms.Values)
                if(r.Examinees.TryGetValue(cbx.Name, out nee) &&
                    nee.eStt != NeeStt.Finished)
                {
                    toSubmCb?.Invoke(false);
                    return;
                }
            toSubmCb?.Invoke(ToSubmit());
        }

        private void cbxAbsen_Checked(object sender, RoutedEventArgs e)
        {
            toSubmCb?.Invoke(ToSubmit());
        }

        public bool ToSubmit()
        {
            CheckBox cbx;
            foreach (ExamRoomA r in mSl.Rooms.Values)
                foreach (ExamineeA nee in r.Examinees.Values)
                    if (nee.eStt != NeeStt.Finished &&
                        (!vAbsen.TryGetValue(nee.ID, out cbx) ||
                        !cbx.IsChecked.HasValue || !cbx.IsChecked.Value))
                        return false;
            return true;
        }
    }
}
