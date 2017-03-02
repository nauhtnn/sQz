using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
    /// Interaction logic for Operation1.xaml
    /// </summary>
    public partial class Operation1 : Page
    {
        Client0 mClient;
        int mSz;
        byte[] mBuffer;
        RequestCode mState;

        public Operation1()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            FirewallHandler fwHndl = new FirewallHandler(1);
            fwHndl.OpenFirewall();
            mSz = 1024 * 1024;
            mState = RequestCode.None;
            mClient = Client0.Instance();
        }

        private void ScaleScreen(double r)
        {
            svwrStudent.Height = svwrStudent.Height * r;
            svwrStudent.Background = new SolidColorBrush(Colors.AliceBlue);
        }

        private void spMain_Loaded(object sender, RoutedEventArgs e)
        {
            spMain.Background = TakeExam.vBrush[(int)BrushId.BG];
            Window w = (Window)Parent;
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            
            double scaleW = spMain.RenderSize.Width / 640; //d:DesignWidth
            //double scaleH = spMain.RenderSize.Height / 360; //d:DesignHeight
            ScaleScreen(scaleW);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            mClient.BeginConnect(CB);
        }

        private void CB(IAsyncResult ar)
        {
            if (ar == null)
            {
                btnConnect_Click(null, null);
                return;
            }
            if (mState == RequestCode.None)
            {
                TcpClient c = (TcpClient)ar.AsyncState;
                //exception: c.EndConnect(ar);
                if (!c.Connected)
                {
                    //else: wait and connect again
                    //System.Timers.Timer aTimer = new System.Timers.Timer(2000);
                    //// Hook up the Elapsed event for the timer. 
                    //aTimer.Elapsed += Connect;
                    //aTimer.AutoReset = false;
                    //aTimer.Enabled = true;
                    return;
                }
                NetworkStream s = c.GetStream();
                char[] msg = new char[1];
                msg[0] = (char)RequestCode.DateStudentRetriving;
                mBuffer = Encoding.UTF8.GetBytes(msg);
                mState = RequestCode.DateStudentRetriving;
                s.BeginWrite(mBuffer, 0, mBuffer.Length, CB, s);
                return;
            }
            if (mState == RequestCode.DateStudentRetriving)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                mBuffer = new byte[mSz];
                mState = RequestCode.DateStudentRetrieved;
                s.BeginRead(mBuffer, 0, mSz, CB, s);
            }
            if (mState == RequestCode.DateStudentRetrieved)
            {
                NetworkStream s = (NetworkStream)ar.AsyncState;
                int nullIdx = Array.IndexOf(mBuffer, 0);
                nullIdx = nullIdx >= 0 ? nullIdx : mBuffer.Length;
                string dat = ASCIIEncoding.ASCII.GetString(mBuffer, 0, nullIdx);
                dat = dat.Substring(0, dat.IndexOf('\0'));
                Dispatcher.Invoke(() => {
                    int idx1 = dat.IndexOf('\n');
                    string date = dat.Substring(0, idx1++);
                    int idx2 = dat.IndexOf('\n', idx1);
                    while (idx2 != -1) //not check ends with '\n' here
                    {
                        string t = dat.Substring(idx1, idx2 - idx1);
                        TextBlock x = new TextBlock();
                        x.Text = t;
                        spStudent.Children.Add(x);
                        idx1 = ++idx2;
                        idx2 = dat.IndexOf('\n', idx2);
                    }
                });
                mState = RequestCode.Dated;
                //s.BeginRead(mBuffer, 0, mSz, CB, s);
            }
        }
    }
}
