using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Prep0.xaml
    /// </summary>
    public partial class Prep0 : Page
    {
        public Prep0()
        {
            InitializeComponent();
        }

        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            if (Date.ChkFmt(tbxDate.Text))
            {
                Date.DBInsert(tbxDate.Text);
                LoadDate();
            }
        }

        private void LoadDate()
        {
            Date.DBSelect();
            if(0 < Date.svDate.Count)
            {
                bool dark = true;
                Color c = new Color();
                c.A = 0xff;
                c.B = c.G = c.R = 0xf0;
                Dispatcher.Invoke(() => {
                    lbxDate.Items.Clear();
                    foreach (string s in Date.svDate)
                    {
                        ListBoxItem i = new ListBoxItem();
                        i.Content = s;
                        dark = !dark;
                        if (dark)
                            i.Background = new SolidColorBrush(c);
                        i.FontSize = TakeExam.em;
                        lbxDate.Items.Add(i);
                    }
                });
            }
        }

        private void spMain_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDate();
        }

        //private void btnBrowse_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenFileDialog dlg = new OpenFileDialog();

        //    // Set filter for file extension and default file extension 
        //    dlg.DefaultExt = ".txt";
        //    dlg.Filter = "Text documents (.txt)|*.txt";
        //    bool? result = dlg.ShowDialog();

        //    // Get the selected file name and display in a TextBox
        //    string filePath = null;
        //    if (result == true)
        //        tbxFileName.Text = filePath = dlg.FileName;
        //}
    }
}
