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
    public class Op0SlotView: StackPanel
    {
        public Dictionary<int, TextBlock> vGrade;
        public Dictionary<int, TextBlock> vDt1;
        public Dictionary<int, TextBlock> vDt2;
        public Dictionary<int, TextBlock> vComp;
        Grid grdNee;
        Grid grdQCtrl;
        TabControl tbcQuest;
        RadioButton rdoA, rdoB;
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
            vTbx = new TextBox[15];//hardcode
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
                    vGrade.Add(e.mLv + e.uId, t);
                    if (e.uGrade != ushort.MaxValue)
                        t.Text = e.uGrade.ToString();
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 4);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vDt1.Add(e.mLv + e.uId, t);
                    if (e.dtTim1.Year != DtFmt.INV)
                        t.Text = e.dtTim1.ToString("HH:mm");
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 5);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vDt2.Add(e.mLv + e.uId, t);
                    if (e.dtTim2.Year != DtFmt.INV)
                        t.Text = e.dtTim2.ToString("HH:mm");
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 6);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vComp.Add(e.mLv + e.uId, t);
                    if (e.tComp != null)
                        t.Text = e.tComp;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 7);
                    grdNee.Children.Add(t);
                    dark = !dark;
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
                    if (e.dtTim1.Hour != DtFmt.INV && vDt1.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.dtTim1.ToString("HH:mm");
                    if (e.dtTim2.Hour != DtFmt.INV && vDt2.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.dtTim2.ToString("HH:mm");
                    if (e.tComp != null && vComp.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.tComp;
                }
        }

        public void ShallowCopy(StackPanel refSp)
        {
            foreach(Grid refg in refSp.Children.OfType<Grid>())
            {
                Grid g = new Grid();
                foreach(ColumnDefinition cd in refg.ColumnDefinitions)
                {
                    ColumnDefinition d = new ColumnDefinition();
                    d.Width = cd.Width;
                    g.ColumnDefinitions.Add(d);
                }
                foreach(TextBlock txt in refg.Children)
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
                vwr.Content = grdNee;
                Children.Add(vwr);
            }

            foreach(StackPanel refsp in refSp.Children.OfType<StackPanel>())
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = refsp.Orientation;

                foreach (Grid refg in refsp.Children.OfType<Grid>())
                {
                    grdQCtrl = new Grid();
                    foreach (ColumnDefinition cd in refg.ColumnDefinitions)
                    {
                        ColumnDefinition d = new ColumnDefinition();
                        d.Width = cd.Width;
                        grdQCtrl.ColumnDefinitions.Add(d);
                    }
                    foreach (RowDefinition rd in refg.RowDefinitions)
                    {
                        RowDefinition d = new RowDefinition();
                        d.Height = rd.Height;
                        grdQCtrl.RowDefinitions.Add(d);
                    }
                    foreach (TextBlock txt in refg.Children.OfType<TextBlock>())
                    {
                        TextBlock t = new TextBlock();
                        if (txt.Name == "txtNq")
                            txtNq = t;
                        else if (txt.Name == "tbxNq")
                            tbxNq = t;
                        t.Text = txt.Text;
                        Grid.SetColumn(t, Grid.GetColumn(txt));
                        Grid.SetRow(t, Grid.GetRow(txt));
                        t.Margin = txt.Margin;
                        grdQCtrl.Children.Add(t);
                    }
                    foreach (TextBox tbx in refg.Children.OfType<TextBox>())
                    {
                        TextBox t = new TextBox();
                        if (tbx.Name == "n")
                            tbxNqs = t;
                        else
                            vTbx[int.Parse(tbx.Name.Substring(1))] = t;
                        t.Width = tbx.Width;
                        Grid.SetColumn(t, Grid.GetColumn(tbx));
                        Grid.SetRow(t, Grid.GetRow(tbx));
                        t.Margin = tbx.Margin;
                        t.IsEnabled = tbx.IsEnabled;
                        grdQCtrl.Children.Add(t);
                    }
                    foreach (RadioButton rdo in refg.Children.OfType<RadioButton>())
                    {
                        RadioButton b = new RadioButton();
                        b.Content = rdo.Content;
                        b.GroupName = rdo.GroupName;
                        Grid.SetColumn(b, Grid.GetColumn(rdo));
                        Grid.SetRow(b, Grid.GetRow(rdo));
                        b.HorizontalAlignment = rdo.HorizontalAlignment;
                        b.VerticalAlignment = rdo.VerticalAlignment;
                        b.IsChecked = rdo.IsChecked;
                        if (rdo.Name == "A")//hardcode, 2 cases
                            rdoA = b;
                        else
                            rdoB = b;
                        b.Checked += rdo_Checked;
                        grdQCtrl.Children.Add(b);
                    }
                    foreach (Button b in refg.Children.OfType<Button>())
                    {
                        btnQSGen = new Button();
                        btnQSGen.Content = b.Content;
                        Grid.SetColumn(btnQSGen, Grid.GetColumn(b));
                        Grid.SetRow(btnQSGen, Grid.GetRow(b));
                        btnQSGen.IsEnabled = b.IsEnabled;
                        btnQSGen.Click += btnQSGen_Click;
                        grdQCtrl.Children.Add(btnQSGen);
                    }
                    sp.Children.Add(grdQCtrl);
                }

                foreach(TabControl tbc in refsp.Children.OfType<TabControl>())
                {
                    tbcQuest = new TabControl();
                    tbcQuest.Width = tbc.Width;
                    tbcQuest.Height = tbc.Height;
                    sp.Children.Add(tbcQuest);
                }

                Children.Add(sp);
            }

            InitQPanel();
        }

        private void btnQSGen_Click(object sender, RoutedEventArgs e)
        {
            TextBox t = tbxNqs;
            int n = int.Parse(t.Text);
            ExamLv lv;
            if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
                lv = ExamLv.A;
            else
                lv = ExamLv.B;
            List<int> vn = new List<int>();
            foreach (IUx i in QuestSheet.GetIUs(lv))
            {
                t = vTbx[(int)i];
                if (t != null)
                    vn.Add(int.Parse(t.Text));
            }
            mSl.GenQPack(n, lv, vn.ToArray());

            ShowQuest();
        }

        private void ShowQuest()
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Dispatcher.Invoke(() => {
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
            TextBox t;
            if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
            {
                foreach (IUx j in QuestSheet.GetIUs(ExamLv.A))
                    if ((t = vTbx[(int)j]) != null)
                        t.IsEnabled = true;
                foreach (IUx j in QuestSheet.GetIUs(ExamLv.B))
                    if ((t = vTbx[(int)j]) != null)
                        t.IsEnabled = false;
            }
            else
            {
                foreach (IUx j in QuestSheet.GetIUs(ExamLv.B))
                    if ((t = vTbx[(int)j]) != null)
                        t.IsEnabled = true;
                foreach (IUx j in QuestSheet.GetIUs(ExamLv.A))
                    if ((t = vTbx[(int)j]) != null)
                        t.IsEnabled = false;
            }
            tbxIU_TextChanged(null, null);
        }

        private void tbxIU_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = tbxNqs;
            if (t == null || t.Text == null || t.Text.Length == 0 || int.Parse(t.Text) <= 0)
            {
                btnQSGen.IsEnabled = false;
                return;
            }
            int n = 0, i;
            bool bG = true;
            if (rdoA.IsChecked.HasValue ? rdoA.IsChecked.Value : false)
            {
                foreach (IUx j in QuestSheet.GetIUs(ExamLv.A))
                    if ((t = vTbx[(int)j]) != null)
                    {
                        if (t.Text != null && 0 < t.Text.Length && 0 < (i = int.Parse(t.Text)))
                            n += i;
                        else
                            bG = false;
                    }
                    else
                        bG = false;
                tbxNq.Text = n.ToString();
                if (bG && n == 30)
                    btnQSGen.IsEnabled = true;
                else
                    btnQSGen.IsEnabled = false;
            }
            else
            {
                foreach (IUx j in QuestSheet.GetIUs(ExamLv.B))
                    if ((t = vTbx[(int)j]) != null)
                    {
                        t.IsEnabled = true;
                        if (t.Text != null && 0 < t.Text.Length && 0 < (i = int.Parse(t.Text)))
                            n += i;
                        else
                            bG = false;
                    }
                    else
                        bG = false;
                tbxNq.Text = n.ToString();
                if (bG && n == 30)
                    btnQSGen.IsEnabled = true;
                else
                    btnQSGen.IsEnabled = false;
            }
        }
    }
}
