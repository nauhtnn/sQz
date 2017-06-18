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
        RadioButton rdoQ;
        RadioButton rdoNee;
        StackPanel spNee;
        TabControl tbcQ;
        public Dictionary<int, TextBlock> vGrade;
        public Dictionary<int, TextBlock> vDt1;
        public Dictionary<int, TextBlock> vDt2;
        public Dictionary<int, TextBlock> vComp;
        Grid grdNee;
        Grid grdQCtrl;
        TextBox[] vTbx;
        TextBox tbxNqs;
        TextBlock txtNq, tbxNq;
        Button btnQSGen;
        public ExamSlot mSl;

        public Op0SlotView()
        {
            vGrade = new Dictionary<int, TextBlock>();
            vDt1 = new Dictionary<int, TextBlock>();
            vDt2 = new Dictionary<int, TextBlock>();
            vComp = new Dictionary<int, TextBlock>();
        }

        public Op0SlotView(ExamSlot sl)
        {
            vGrade = new Dictionary<int, TextBlock>();
            vDt1 = new Dictionary<int, TextBlock>();
            vDt2 = new Dictionary<int, TextBlock>();
            vComp = new Dictionary<int, TextBlock>();
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
            spContent.Children.Add(tbcQ);
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
            spContent.Children.Add(spNee);
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

        private void btnQSGen_Click(object sender, RoutedEventArgs e)
        {
            //TextBox t = tbxNqs;
            //int n = int.Parse(t.Text);
            //ExamLv lv;
            //if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
            //    lv = ExamLv.A;
            //else
            //    lv = ExamLv.B;
            //List<int> vn = new List<int>();
            //foreach (IUx i in QuestSheet.GetIUs(lv))
            //{
            //    t = vTbx[(int)i];
            //    if (t != null)
            //        vn.Add(int.Parse(t.Text));
            //}
            //mSl.GenQPack(n, lv, vn.ToArray());

            //ShowQuest();
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

        public void InitQPanel()
        {
            foreach (IUx i in QuestSheet.GetAllIUs())
            {
                TextBox t = vTbx[(int)i];
                if (t != null)
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

        private void rdo_Checked(object sender, RoutedEventArgs e)
        {
            //TextBox t;
            //if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
            //{
            //    foreach (IUx j in QuestSheet.GetIUs(ExamLv.A))
            //        if ((t = vTbx[(int)j]) != null)
            //            t.IsEnabled = true;
            //    foreach (IUx j in QuestSheet.GetIUs(ExamLv.B))
            //        if ((t = vTbx[(int)j]) != null)
            //            t.IsEnabled = false;
            //}
            //else
            //{
            //    foreach (IUx j in QuestSheet.GetIUs(ExamLv.B))
            //        if ((t = vTbx[(int)j]) != null)
            //            t.IsEnabled = true;
            //    foreach (IUx j in QuestSheet.GetIUs(ExamLv.A))
            //        if ((t = vTbx[(int)j]) != null)
            //            t.IsEnabled = false;
            //}
            //tbxIU_TextChanged(null, null);
        }

        private void tbxIU_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TextBox t = tbxNqs;
            //if (t == null || t.Text == null || t.Text.Length == 0 || int.Parse(t.Text) <= 0)
            //{
            //    btnQSGen.IsEnabled = false;
            //    return;
            //}
            //int n = 0, i;
            //bool bG = true;
            //if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
            //{
            //    foreach (IUx j in QuestSheet.GetIUs(ExamLv.A))
            //        if ((t = vTbx[(int)j]) != null)
            //        {
            //            if (t.Text != null && 0 < t.Text.Length && 0 < (i = int.Parse(t.Text)))
            //                n += i;
            //            else
            //                bG = false;
            //        }
            //        else
            //            bG = false;
            //    tbxNq.Text = n.ToString();
            //    if (bG && n == 30)
            //        btnQSGen.IsEnabled = true;
            //    else
            //        btnQSGen.IsEnabled = false;
            //}
            //else
            //{
            //    foreach (IUx j in QuestSheet.GetIUs(ExamLv.B))
            //        if ((t = vTbx[(int)j]) != null)
            //        {
            //            t.IsEnabled = true;
            //            if (t.Text != null && 0 < t.Text.Length && 0 < (i = int.Parse(t.Text)))
            //                n += i;
            //            else
            //                bG = false;
            //        }
            //        else
            //            bG = false;
            //    tbxNq.Text = n.ToString();
            //    if (bG && n == 30)
            //        btnQSGen.IsEnabled = true;
            //    else
            //        btnQSGen.IsEnabled = false;
            //}
        }
    }
}
