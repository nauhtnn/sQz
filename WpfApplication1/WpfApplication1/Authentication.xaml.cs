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
        int mSz;
        byte[] mBuffer;
        RequestCode mState;
        bool bSrvrMsg;
        string mSrvrMsg;

        public Authentication()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            FirewallHandler fwHndl = new FirewallHandler(3);
            fwHndl.OpenFirewall();
            mSz = 1024 * 1024;
            mState = RequestCode.None;
            mClient = Client0.Instance();
            //Connect(null, null);
            mSrvrMsg = String.Empty;
            bSrvrMsg = false;
        }

        private void Connect(Object source, System.Timers.ElapsedEventArgs e)
        {
            mClient.BeginConnect(CB);
        }

        private void CB(IAsyncResult ar)
        {
            if(ar == null)
            {
                Connect(null, null);
                return;
            }
            if (mState == RequestCode.None)
            {
                TcpClient c = (TcpClient)ar.AsyncState;
                //exception: c.EndConnect(ar);
                if (!c.Connected)
                {
                    Dispatcher.Invoke(() => { txMessage.Text += "connected"; });
                    //else: wait and connect again
                    System.Timers.Timer aTimer = new System.Timers.Timer(2000);
                    // Hook up the Elapsed event for the timer. 
                    aTimer.Elapsed += Connect;
                    aTimer.AutoReset = false;
                    aTimer.Enabled = true;
                    return;
                }
                NetworkStream s = c.GetStream();
                char[] msg = new char[1];
                msg[0] = (char)RequestCode.Dating;
                mBuffer = Encoding.UTF8.GetBytes(msg);
                mState = RequestCode.Dating;
                s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                return;
            }
            if (mState == RequestCode.Dating)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                mBuffer = new byte[mSz];
                mState = RequestCode.Dated;
                s.BeginRead(mBuffer, 0, mSz, CB, s);
            }
            if (mState == RequestCode.Dated)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                int nullIdx = Array.IndexOf(mBuffer, 0);
                nullIdx = nullIdx >= 0 ? nullIdx : mBuffer.Length;
                string date = ASCIIEncoding.ASCII.GetString(mBuffer, 0, nullIdx);
                date = date.Substring(0, date.IndexOf('\0'));
                Dispatcher.Invoke(() => { txtDate.Text = date; });
                mState = RequestCode.Dated;
                //s.BeginRead(mBuffer, 0, mSz, CB, s);
            }
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            mClient.BeginWrite(txtUsername.Text + "\n" + txtPassword.Text, SignInCallback);
        }

        private void SignInCallback(IAsyncResult ar)
        {
            NetworkStream s = (NetworkStream)ar.AsyncState;
            s.EndWrite(ar);
            mClient.BeginRead(mBuffer, mSz, QuestReadCallback);
        }

        private void QuestReadCallback(IAsyncResult ar)
        {
            NetworkStream s = (NetworkStream)ar.AsyncState;
            s.EndRead(ar);
            //NavigationService.Navigate(new TakeExam());
            Dispatcher.Invoke(() =>
            {
                NavigationService.Navigate(new Uri("TakeExam.xaml", UriKind.Relative), mBuffer);//must have Urikind
            });
            //txMessage.Text += "\n" + txtUsername.Text + "\n" + txtPassword + "\n";
        }

        Thread th;
        private void btnStartSer_Click(object sender, RoutedEventArgs e)
        {
            th = new Thread(new ThreadStart(()=> { Server0 t = new Server0(ResponseMsg); t.Start(ref bSrvrMsg, ref mSrvrMsg); }));
            th.Start();
        }

        public string ResponseMsg(char code)
        {
            return "for debug only";
        }

        private void btnStopSer_Click(object sender, RoutedEventArgs e)
        {
            //ServerInstance.Stop();
            //th.Abort();
            //th = null;
            //mClient.BeginConnect(ConnectCallback);
            Connect(null, null);
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
