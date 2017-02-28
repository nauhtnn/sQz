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
using System.Threading;
using System.Net.Sockets;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Authentication.xaml
    /// </summary>
    public partial class Authentication : Page
    {
        Client0 mClient;
        byte[] mBuffer;
        int mSz;
        public Authentication()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            FirewallHandler fwHndl = new FirewallHandler();
            fwHndl.OpenFirewall();
            mClient = Client0.GetInstance();
            mSz = 1024 * 1024;
            mBuffer = new byte[mSz];
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            mClient.BeginWrite(txtUsername.Text + "\n" + txtPassword.Text, SignInCallback);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            TcpClient c = (TcpClient)ar.AsyncState;
            //exception: c.EndConnect(ar);
            Dispatcher.Invoke(() => { txMessage.Text += "connected"; });
        }

        private void SignInCallback(IAsyncResult ar)
        {
            NetworkStream s = (NetworkStream)ar.AsyncState;
            s.EndWrite(ar);
            mClient.BeginRead(QuestReadCallback, mBuffer, mSz);
        }

        private void QuestReadCallback(IAsyncResult ar)
        {
            NetworkStream s = (NetworkStream)ar.AsyncState;
            s.EndRead(ar);
            //NavigationService.Navigate(new TakeExam());
            Dispatcher.Invoke(() =>
            {
                NavigationService.LoadCompleted += TakeExam.NavigationService_LoadCompleted;
                NavigationService.Navigate(new Uri("TakeExam.xaml", UriKind.Relative), mBuffer);//must have Urikind
            });
            //txMessage.Text += "\n" + txtUsername.Text + "\n" + txtPassword + "\n";
        }

        Thread th;
        private void btnStartSer_Click(object sender, RoutedEventArgs e)
        {
            th = new Thread(new ThreadStart(()=> { ServerInstance.Start(); }));
            th.Start();
        }

        private void btnStopSer_Click(object sender, RoutedEventArgs e)
        {
            //ServerInstance.Stop();
            //th.Abort();
            //th = null;
            mClient.BeginConnect(ConnectCallback);
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
