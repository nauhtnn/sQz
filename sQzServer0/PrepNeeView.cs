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
    class PrepNeeView: StackPanel
    {
        Button btnXdb;
        Button btnXfl;
        Button btnBrowse;
        Button btnIns;
        ListBox lbxDB;
        ListBox lbxFile;
        public ExamSlot mSlDB;
        public ExamSlot mSlFile;

        public PrepNeeView() { }
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
                foreach (Button btn in refg.Children.OfType<Button>())
                {
                    Button b = new Button();
                    b.Content = btn.Content;
                    b.Height = btn.Height;
                    b.Width = btn.Width;
                    Grid.SetColumn(b, Grid.GetColumn(btn));
                    btnXdb = b;
                    b.Click += btnXdb_Click;
                    g.Children.Add(b);
                }
                foreach(StackPanel sp in refg.Children.OfType<StackPanel>())
                {
                    StackPanel p = new StackPanel();
                    p.Orientation = sp.Orientation;
                    Grid.SetColumn(p, Grid.GetColumn(sp));
                    p.HorizontalAlignment = sp.HorizontalAlignment;
                    foreach(Button btn in sp.Children)
                    {
                        Button b = new Button();
                        b.Content = btn.Content;
                        b.Height = btn.Height;
                        b.Width = btn.Width;
                        b.Margin = btn.Margin;
                        Grid.SetColumn(b, Grid.GetColumn(btn));
                        if (btn.Name == "a")
                        {
                            btnBrowse = b;
                            b.Click += btnBrowse_Click;
                        }
                        else
                        {
                            btnXfl = b;
                            b.Click += btnXfl_Click;
                        }
                        p.Children.Add(b);
                    }
                    g.Children.Add(p);
                }
                g.Name = refg.Name;
                Children.Add(g);
            }
            foreach(StackPanel refsp in refSp.Children.OfType<StackPanel>())
            {
                StackPanel sp = new StackPanel();
                sp.Margin = refsp.Margin;
                sp.Orientation = refsp.Orientation;
                foreach (ListBox reflbx in refsp.Children.OfType<ListBox>())
                {
                    if(reflbx.Name == "_1")
                    {
                        lbxDB = new ListBox();
                        lbxDB.Width = reflbx.Width;
                        lbxDB.Height = reflbx.Height;
                    }
                    else
                    {
                        lbxFile = new ListBox();
                        lbxFile.Width = reflbx.Width;
                        lbxFile.Height = reflbx.Height;
                    }
                }
                foreach (Button refb in refsp.Children.OfType<Button>())
                {
                    btnIns = new Button();
                    btnIns.Height = refb.Height;
                    btnIns.Width = refb.Width;
                    btnIns.Margin = refb.Margin;
                    btnIns.Content = "<<";
                    btnIns.Click += btnIns_Click;
                }
                sp.Children.Add(lbxDB);
                sp.Children.Add(btnIns);
                sp.Children.Add(lbxFile);
                Children.Add(sp);
            }
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

            string emsg = mSlDB.ReadF(fp, ref mSlFile);
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
            mSlFile.DelNee();
            Show(false);
        }

        public void Show(bool db)
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            ListBox l = null;
            ExamSlot sl;
            if (db)
            {
                sl = mSlDB;
                l = lbxDB;
            }
            else
            {
                sl = mSlFile;
                l = lbxFile;
            }
            l.Items.Clear();
            foreach (ExamRoom r in sl.vRoom.Values)
                foreach (ExamineeA e in r.vExaminee.Values)
                {
                    ListBoxItem i = new ListBoxItem();
                    i.Content = e.ToString();
                    dark = !dark;
                    if (dark)
                        i.Background = new SolidColorBrush(c);
                    l.Items.Add(i);
                }
        }

        private void btnIns_Click(object sender, RoutedEventArgs e)
        {
            lbxFile.Items.Clear();
            string emsg;
            if (mSlFile.DBInsNee(out emsg) <= 0)
                WPopup.s.ShowDialog(emsg);
            mSlDB.DBSelNee();
            Show(true);
        }
    }
}
