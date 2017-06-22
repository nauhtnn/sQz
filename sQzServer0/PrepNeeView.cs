using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using sQzLib;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media;

namespace sQzServer0
{
    class PrepNeeView: TabItem
    {
        Grid grdDB;
        Grid grdTmp;
        public ExamSlot mSlDB;
        public ExamSlot mSlTmp;

        public PrepNeeView() { }
        public PrepNeeView(ExamSlot sl)
        {
            mSlDB = sl;
            Header = mSlDB.Dt.ToString(DT.hh);
            Name = "_" + (Header as string).Replace(':', '_');
            mSlTmp = new ExamSlot();
            mSlTmp.Dt = mSlDB.Dt;
            foreach (ExamRoom rom in mSlDB.vRoom.Values)
                if (!mSlTmp.vRoom.ContainsKey(rom.uId))
                {
                    ExamRoom r = new ExamRoom();
                    r.uId = rom.uId;
                    mSlTmp.vRoom.Add(r.uId, r);
                }
        }
        public void DeepCopy(StackPanel refSp)
        {
            StackPanel p = new StackPanel();
            StackPanel sp1 = null, sp2 = null;
            foreach(StackPanel sp in refSp.Children.OfType<StackPanel>())
            {
                if (sp.Name == "sp1")
                    sp1 = DeepCopySp1(sp);
                else
                    sp2 = DeepCopySp2(sp);
            }

            Grid gTitle1 = null, gTitle2;
            foreach (Grid refg in refSp.Children.OfType<Grid>())
                gTitle1 = DeepCopy(refg);
            gTitle2 = DeepCopy(gTitle1);

            ScrollViewer vwr1 = null, vwr2;
            foreach (ScrollViewer refscrvwr in refSp.Children.OfType<ScrollViewer>())
                vwr1 = DeepCopy(refscrvwr);
            grdDB = vwr1.Content as Grid;
            vwr2 = DeepCopy(vwr1);
            grdTmp = vwr2.Content as Grid;

            p.Children.Add(sp1);
            p.Children.Add(gTitle1);
            p.Children.Add(vwr1);
            p.Children.Add(sp2);
            p.Children.Add(gTitle2);
            p.Children.Add(vwr2);
            Content = p;
        }

        StackPanel DeepCopySp1(StackPanel sp)
        {
            StackPanel p = new StackPanel();
            foreach(TextBlock tbx in sp.Children.OfType<TextBlock>())
            {
                TextBlock t = new TextBlock();
                t.Text = tbx.Text;
                t.Width = tbx.Width;
                t.Padding = tbx.Padding;
                p.Children.Add(t);
            }
            foreach(Button btn in sp.Children.OfType<Button>())
            {
                Button b = new Button();
                b.Height = btn.Height;
                b.Width = btn.Width;
                b.Margin = btn.Margin;
                b.Content = Txt.s._[(int)TxI.PREP_DEL];
                b.Click += btnXdb_Click;
                p.Children.Add(b);
            }
            p.Orientation = sp.Orientation;
            return p;
        }

        StackPanel DeepCopySp2(StackPanel sp)
        {
            StackPanel p = new StackPanel();
            foreach (TextBlock tbx in sp.Children.OfType<TextBlock>())
            {
                TextBlock t = new TextBlock();
                t.Text = tbx.Text;
                t.Width = tbx.Width;
                t.Padding = tbx.Padding;
                p.Children.Add(t);
            }
            foreach (Button btn in sp.Children.OfType<Button>())
            {
                Button b = new Button();
                b.Height = btn.Height;
                b.Width = btn.Width;
                b.Margin = btn.Margin;
                b.Content = btn.Content;
                b.Background = btn.Background;
                b.Foreground = btn.Foreground;
                b.FontWeight = btn.FontWeight;
                if (btn.Name == "btnImpDB")
                    b.Click += btnIns_Click;
                else if (btn.Name == "btnFileNee")
                    b.Click += btnBrowse_Click;
                else
                    b.Click += btnXdb_Click;
                p.Children.Add(b);
            }
            p.Orientation = sp.Orientation;
            return p;
        }

        Grid DeepCopy(Grid refg)
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
            return g;
        }

        ScrollViewer DeepCopy(ScrollViewer swvr)
        {
            ScrollViewer vwr = new ScrollViewer();
            Grid refg = swvr.Content as Grid;
            vwr.Width = swvr.Width;
            vwr.Height = swvr.Height;
            vwr.HorizontalAlignment = HorizontalAlignment.Left;
            Grid grdNee = new Grid();
            foreach (ColumnDefinition cd in refg.ColumnDefinitions)
            {
                ColumnDefinition d = new ColumnDefinition();
                d.Width = cd.Width;
                grdNee.ColumnDefinitions.Add(d);
            }
            vwr.Content = grdNee;
            return vwr;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            // set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "text documents (*.txt)|*.txt";
            bool? result = dlg.ShowDialog();
            if (result == false)
                return;

            string fp = dlg.FileName;

            string emsg = mSlDB.ReadF(fp, ref mSlTmp);
            if(emsg != null)
            {
                Window w = Window.GetWindow(this);
                if(w != null)
                    w.Opacity = 0.5;
                WPopup.s.ShowDialog(emsg);
                if (w != null)
                    w.Opacity = 1;
            }

            Show(false);
        }

        private void btnXdb_Click(object sender, RoutedEventArgs e)
        {
            WPopup.s.ShowDialog(mSlDB.DBDelNee());
            mSlDB.DBSelNee();
            Show(true);
        }

        private void btnXfl_Click(object sender, RoutedEventArgs e)
        {
            mSlTmp.DelNee();
            Show(false);
        }

        public void Show(bool db)
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            SolidColorBrush br = new SolidColorBrush(c);
            GridLength rh = new GridLength(26);
            Grid g;
            ExamSlot sl;
            if (db)
            {
                sl = mSlDB;
                g = grdDB;
            }
            else
            {
                sl = mSlTmp;
                g = grdTmp;
            }
            g.Children.Clear();
            int rid = -1;
            foreach (ExamRoom r in sl.vRoom.Values)
                foreach (ExamineeA e in r.vExaminee.Values)
                {
                    rid++;
                    RowDefinition rd = new RowDefinition();
                    rd.Height = rh;
                    g.RowDefinitions.Add(rd);
                    TextBlock t = new TextBlock();
                    t.Text = e.tId;
                    if (dark)
                        t.Background = br;
                    Grid.SetRow(t, rid);
                    g.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tName;
                    if (dark)
                        t.Background = br;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 1);
                    g.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirdate;
                    if (dark)
                        t.Background = br;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 2);
                    g.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirthplace;
                    if (dark)
                        t.Background = br;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 3);
                    g.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = br;
                    t.Text = (r.uId + 1).ToString();
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 4);
                    g.Children.Add(t);
                    dark = !dark;
                }
        }

        private void btnIns_Click(object sender, RoutedEventArgs e)
        {
            grdTmp.Children.Clear();
            string emsg;
            if (mSlTmp.DBInsNee(out emsg) <= 0)
                WPopup.s.ShowDialog(emsg);
            mSlDB.DBSelNee();
            Show(true);
        }
    }
}
