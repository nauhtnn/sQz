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
        TabItem tbiDB, tbiTmp;

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
            TabControl tbc = new TabControl();
            StackPanel sp = new StackPanel();
            Grid gTit = null, gNee = null;
            foreach (Grid refg in refSp.Children.OfType<Grid>())
            {
                if (refg.Name == "grdTit")
                    gTit = DeepCopyTit(refg, false);
                else
                    gNee = DeepCopyNee(refg);
            }
            sp.Children.Add(gTit);
            sp.Children.Add(gNee);
            grdDB = DeepCopyNee(gNee);
            grdDB.Children.Clear();
            ScrollViewer vwr = null;
            foreach (ScrollViewer refscrvwr in refSp.Children.OfType<ScrollViewer>())
                vwr = DeepCopy(refscrvwr);
            vwr.Content = grdDB;
            sp.Children.Add(vwr);
            tbiDB = new TabItem();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._[(int)TxI.NEE_LS_DB], 0);
            tbiDB.Header = sb.ToString();
            tbiDB.Content = sp;
            tbc.Items.Add(tbiDB);
            //
            sp = new StackPanel();
            foreach (Grid refg in refSp.Children.OfType<Grid>())
            {
                if (refg.Name == "grdTit")
                    gTit = DeepCopyTit(refg, true);
                else
                    gNee = DeepCopyNee(refg);
            }
            sp.Children.Add(gTit);
            sp.Children.Add(gNee);
            grdTmp = DeepCopyNee(gNee);
            grdTmp.Children.Clear();
            foreach (ScrollViewer refscrvwr in refSp.Children.OfType<ScrollViewer>())
                vwr = DeepCopy(refscrvwr);
            vwr.Content = grdTmp;
            sp.Children.Add(vwr);
            tbiTmp = new TabItem();
            sb.Clear();
            sb.AppendFormat(Txt.s._[(int)TxI.NEE_LS_TMP], 0);
            tbiTmp.Header = sb.ToString(); ;
            tbiTmp.Content = sp;
            tbc.Items.Add(tbiTmp);
            //
            Content = tbc;
        }

        Grid DeepCopyTit(Grid refg, bool bTmp)
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
            foreach (TextBlock txt in refg.Children.OfType<TextBlock>())
            {
                TextBlock t = new TextBlock();
                t.Text = txt.Text;
                t.Background = txt.Background;
                t.Foreground = txt.Foreground;
                Grid.SetColumn(t, Grid.GetColumn(txt));
                Grid.SetRow(t, Grid.GetRow(txt));
                g.Children.Add(t);
            }
            foreach(Button btn in refg.Children.OfType<Button>())
            {
                if (!bTmp && (btn.Name == "btnImp" || btn.Name == "btnFile"))
                    continue;
                Button b = new Button();
                b.Height = btn.Height;
                b.Width = btn.Width;
                b.Margin = btn.Margin;
                b.Content = btn.Content;
                b.Background = btn.Background;
                b.Foreground = btn.Foreground;
                b.FontWeight = btn.FontWeight;
                Grid.SetColumn(b, Grid.GetColumn(btn));
                if (bTmp && btn.Name == "btnImp")
                {
                    b.Content = Txt.s._[(int)TxI.PREP_IMP];
                    b.Click += btnImp_Click;
                }
                else if (bTmp && btn.Name == "btnFile")
                {
                    b.Content = "+";
                    b.Click += btnFile_Click;
                }
                else
                {
                    b.Content = Txt.s._[(int)TxI.PREP_DEL];
                    if(bTmp)
                        b.Click += btnXTmp_Click;
                    else
                        b.Click += btnXdb_Click;
                }
                g.Children.Add(b);
            }
            g.Margin = refg.Margin;
            return g;
        }

        Grid DeepCopyNee(Grid refg)
        {
            Grid g = new Grid();
            foreach (ColumnDefinition cd in refg.ColumnDefinitions)
            {
                ColumnDefinition d = new ColumnDefinition();
                d.Width = cd.Width;
                g.ColumnDefinitions.Add(d);
            }
            foreach (TextBlock txt in refg.Children.OfType<TextBlock>())
            {
                TextBlock t = new TextBlock();
                t.Text = txt.Text;
                t.TextAlignment = txt.TextAlignment;
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
            return vwr;
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
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

        private void btnXTmp_Click(object sender, RoutedEventArgs e)
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
            int n = 0;
            foreach (ExamRoom r in sl.vRoom.Values)
                n += r.vExaminee.Count;
            StringBuilder sb = new StringBuilder();
            if(db)
            {
                sb.AppendFormat(Txt.s._[(int)TxI.NEE_LS_DB], n);
                tbiDB.Header = sb.ToString();
            }
            else
            {
                sb.AppendFormat(Txt.s._[(int)TxI.NEE_LS_TMP], n);
                tbiTmp.Header = sb.ToString();
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
                    t.HorizontalAlignment = HorizontalAlignment.Center;
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
                    t.HorizontalAlignment = HorizontalAlignment.Center;
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
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    g.Children.Add(t);
                    dark = !dark;
                }
        }

        private void btnImp_Click(object sender, RoutedEventArgs e)
        {
            string emsg;
            if (mSlTmp.DBInsNee(out emsg) <= 0)
                WPopup.s.ShowDialog(emsg);
            mSlTmp.DelNee();
            grdTmp.Children.Clear();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._[(int)TxI.NEE_LS_TMP], 0);
            tbiTmp.Header = sb.ToString();
            mSlDB.DBSelNee();
            Show(true);
        }
    }
}
