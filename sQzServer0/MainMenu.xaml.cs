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
using sQzLib;

namespace sQzServer0
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        int uVer = 100;
        public MainMenu()
        {
            InitializeComponent();
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            txtLalgitc.Text = t._((int)TxI.LALGITC);
            txtsQz.Text = t._((int)TxI.SQZ);
            btnPrep.Content = t._((int)TxI.PREP);
            btnOp.Content = t._((int)TxI.OPER);
            btnArchv.Content = t._((int)TxI.ARCH);
            btnExit.Content = t._((int)TxI.EXIT);
        }

        private void btnPrep_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Prep0.xaml", UriKind.Relative));
        }

        private void btnArchv_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Archive.xaml", UriKind.Relative));
        }

        private void btnOp_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Operation0.xaml", UriKind.Relative));
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Application.Current.MainWindow;
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 28;

            LoadTxt();

            MySql.Data.MySqlClient.MySqlConnection conn = DBConnect.Init();
            if (conn == null)
            {
                DisableBtns();
                WPopup.s.ShowDialog(Txt.s._((int)TxI.DB_NOK));
                return;
            }
            string qry = DBConnect.mkQrySelect("sqz_version", "ver", null);
            string emsg;
            MySql.Data.MySqlClient.MySqlDataReader reader =
                DBConnect.exeQrySelect(conn, qry, out emsg);
            if(reader == null)
            {
                DisableBtns();
                WPopup.s.ShowDialog(emsg);
            }
            else
            {
                bool bNVer = true;
                if (reader.Read())
                {
                    int ver = 0;
                    if (!reader.IsDBNull(0))
                        ver = reader.GetInt32(0);
                    if (ver == uVer)
                        bNVer = false;
                }
                reader.Close();
                if (bNVer)
                {
                    DisableBtns();
                    WPopup.s.ShowDialog(Txt.s._((int)TxI.DB_VER_NOK) +
                        (uVer / 100) + '.' + (uVer % 100 / 10) + '.' + (uVer % 10));
                }
            }
            
            DBConnect.Close(ref conn);
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WPopup.s.Exit();
        }

        void DisableBtns()
        {
            btnArchv.IsEnabled = btnOp.IsEnabled
                = btnPrep.IsEnabled = false;
        }
    }
}
