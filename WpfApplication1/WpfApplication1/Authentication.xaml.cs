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
using System.Timers;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Authentication.xaml
    /// </summary>
    public partial class Authentication : Page
    {
        public Authentication()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            FirewallHandler fwHndl = new FirewallHandler();
            fwHndl.OpenFirewall();
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new TakeExam());
            //NavigationService.Navigate(new Uri("TakeExam.xaml", UriKind.Relative));//must have Urikind
            //txMessage.Text += "\n" + txtUsername.Text + "\n" + txtPassword + "\n";
            ClientInstance.Connect("127.0.0.1", "hello world");
        }

        private void btnStartSer_Click(object sender, RoutedEventArgs e)
        {
            Timer aTimer = new Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = false;
            aTimer.Enabled = true;

            ServerInstance.Start();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            SignIn(null, null);
        }

        private void btnStopSer_Click(object sender, RoutedEventArgs e)
        {
            ServerInstance.Stop();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = (Window)Parent;
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
        }

        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            txtUsername.Text = String.Empty;
            if (txtUsername.Text == "type username" ||
                !System.Text.RegularExpressions.Regex.Match(txtUsername.Text, "[a-zA-Z0-9]").Success)
                txtUsername.Text = String.Empty;
        }

        //protected virtual void OnNavigatedFrom(NavigationEventArgs e)
        //{
        //    int a = 0;
        //    ++a;
        //}

        //protected void OnNavigatingFrom(NavigatingCancelEventArgs e)
        //{
        //    int a = 0;
        //    ++a;
        //}

        //protected void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    int a = 0;
        //    ++a;
        //}
    }
}
