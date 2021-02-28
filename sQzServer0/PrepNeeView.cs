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
        public ExamSlotS0 mSlDB;
        public ExamSlotS0 mSlTmp;
        TabItem tbiDB, tbiTmp;

        public PrepNeeView() { }
        public PrepNeeView(ExamSlotS0 sl)
        {
            mSlDB = sl;
            Header = mSlDB.Dt.ToString(DT._);
            Name = DT.CreateNameFromDateTime(Header as string);
            mSlTmp = new ExamSlotS0();
            mSlTmp.Dt = mSlDB.Dt;
            foreach (ExamRoomS0 rom in mSlDB.Rooms.Values)
                if (!mSlTmp.Rooms.ContainsKey(rom.uId))
                {
                    ExamRoomS0 r = new ExamRoomS0();
                    r.uId = rom.uId;
                    mSlTmp.Rooms.Add(r.uId, r);
                }
        }
        public void DeepCopy(StackPanel refSp)
        {
            TabControl tbc = new TabControl();
            StackPanel sp = new StackPanel();
            Grid gNee = null;
            foreach (Grid refg in refSp.Children.OfType<Grid>())
            {
                if (refg.Name != "grdTit")
                    gNee = DeepCopyNee(refg);
            }
            Button btnDel = new Button();
            btnDel.Click += btnDel_Click;
            btnDel.Width = 120;
            btnDel.Height = 40;
            btnDel.Margin = new Thickness(0, 10, 0, 10);
            btnDel.Content = Txt.s._((int)TxI.PREP_DEL);
            if (mSlDB.eStt == ExamStt.Prep)
            {
                btnDel.IsEnabled = true;
                btnDel.Foreground = Theme.s._[(int)BrushId.FG];
                btnDel.Background = Theme.s._[(int)BrushId.Button_Hover];
            }
            else
            {
                btnDel.IsEnabled = false;
                btnDel.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                btnDel.Background = Theme.s._[(int)BrushId.BG_Gray];
            }
            sp.Children.Add(btnDel);
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
            sb.AppendFormat(Txt.s._((int)TxI.NEE_LS_DB), 0);
            tbiDB.Header = sb.ToString();
            tbiDB.Content = sp;
            tbc.Items.Add(tbiDB);
            //
            sp = new StackPanel();
            Grid gTit = null;
            foreach (Grid refg in refSp.Children.OfType<Grid>())
            {
                if (refg.Name == "grdTit")
                    gTit = DeepCopyTit(refg);
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
            sb.AppendFormat(Txt.s._((int)TxI.NEE_LS_TMP), 0);
            tbiTmp.Header = sb.ToString(); ;
            tbiTmp.Content = sp;
            tbc.Items.Add(tbiTmp);
            //
            Content = tbc;
        }

        Grid DeepCopyTit(Grid refg)
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
                Button b = new Button();
                b.IsEnabled = btn.IsEnabled;
                b.Height = btn.Height;
                b.Width = btn.Width;
                b.Margin = btn.Margin;
                b.Content = btn.Content;
                if(b.IsEnabled)
                {
                    b.Background = btn.Background;
                    b.Foreground = btn.Foreground;
                }
                else
                {
                    b.Background = Theme.s._[(int)BrushId.BG_Gray];
                    b.Foreground = Theme.s._[(int)BrushId.FG_Gray];
                }
                b.FontWeight = btn.FontWeight;
                Grid.SetColumn(b, Grid.GetColumn(btn));
                if (btn.Name == "btnImp")
                {
                    b.Content = Txt.s._((int)TxI.PREP_IMP);
                    b.Click += btnImp_Click;
                }
                else if (btn.Name == "btnFile")
                {
                    b.Content = "+";
                    b.Click += btnFile_Click;
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
            mSlTmp.DelNee();
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

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            WPopup.s.ShowDialog(mSlDB.DBDelNee());
            mSlDB.DBSelNee();
            Show(true);
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
            ExamSlotS0 sl;
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
            foreach (ExamRoomS0 r in sl.Rooms.Values)
                n += r.Examinees.Count;
            StringBuilder sb = new StringBuilder();
            if(db)
            {
                sb.AppendFormat(Txt.s._((int)TxI.NEE_LS_DB), n);
                tbiDB.Header = sb.ToString();
            }
            else
            {
                sb.AppendFormat(Txt.s._((int)TxI.NEE_LS_TMP), n);
                tbiTmp.Header = sb.ToString();
            }
            g.Children.Clear();
            int rid = -1;
            foreach (ExamRoomS0 r in sl.Rooms.Values)
                foreach (ExamineeA e in r.Examinees.Values)
                {
                    rid++;
                    RowDefinition rd = new RowDefinition();
                    rd.Height = rh;
                    g.RowDefinitions.Add(rd);
                    TextBlock t = new TextBlock();
                    t.Text = e.ID;
                    if (dark)
                        t.Background = br;
                    Grid.SetRow(t, rid);
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    g.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.Name;
                    if (dark)
                        t.Background = br;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 1);
                    g.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.Birthdate;
                    if (dark)
                        t.Background = br;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 2);
                    t.HorizontalAlignment = HorizontalAlignment.Center;
                    g.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.Birthplace;
                    if (dark)
                        t.Background = br;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 3);
                    g.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = br;
                    t.Text = r.uId.ToString();
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
            sb.AppendFormat(Txt.s._((int)TxI.NEE_LS_TMP), 0);
            tbiTmp.Header = sb.ToString();
            mSlDB.DBSelNee();
            Show(true);
        }
    }
}
