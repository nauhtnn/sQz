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
        Button btnBrowse;
        Button btnIns;
        ListBox lbxDB;
        ListBox lbxFile;
        public ExamSlot mSl;

        public PrepNeeView() { }
        public void ShallowCopy(StackPanel refSp)
        {
            foreach (Button refb in refSp.Children.OfType<Button>())
            {
                btnBrowse = new Button();
                btnBrowse.Height = refb.Height;
                btnBrowse.Width = refb.Width;
                btnBrowse.Content = "+";//Txt.s._[(int)TxI.NEE_ADD];
                btnBrowse.IsEnabled = refb.IsEnabled;
                btnBrowse.Click += btnBrowse_Click;
                Children.Add(btnBrowse);
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

            mSl.ReadF(fp);

            Show(false);
        }

        public void Show(bool db)
        {
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            ListBox l = null;
            if (db)
                l = lbxDB;
            else
                l = lbxFile;
            l.Items.Clear();
            foreach (ExamRoom r in mSl.vRoom.Values)
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
            mSl.DBInsNee();
            Show(true);
        }
    }
}
