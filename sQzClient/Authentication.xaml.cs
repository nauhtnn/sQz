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
using sQzLib;

namespace sQzClient
{
    /// <summary>
    /// Interaction logic for Authentication.xaml
    /// </summary>
    public partial class Authentication : Page
    {
        Client2 mClient;
        NetCode mState;
        UICbMsg mCbMsg;
        bool bReconn;
        string mNeeId;
        string mBirdate;
        public Authentication()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            bReconn = false;
            mState = NetCode.Dating;
            mClient = new Client2(CliBufHndl, CliBufPrep);
            mCbMsg = new UICbMsg();

            mNeeId = mBirdate = string.Empty;

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void Connect(Object source, System.Timers.ElapsedEventArgs e)
        {
            Thread th = new Thread(() => { mClient.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            mNeeId = tbxNeeId.Text;
            mBirdate = tbxD.Text + "/" + tbxM.Text + "/" + tbxY.Text;
            Thread th = new Thread(() => { mClient.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = (Window)Parent;
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;

            double rt = w.RenderSize.Width / 1280; //design size
            ScaleTransform st = new ScaleTransform(rt, rt);
            spMain.RenderTransform = st;

            FirewallHandler fwHndl = new FirewallHandler(3);
            lblStatus.Text += fwHndl.OpenFirewall();

            Connect(null, null);
        }

        private void btnReconn_Click(object sender, RoutedEventArgs e)
        {
        }

        public bool CliBufHndl(byte[] buf, int offs)
        {
            switch (mState)
            {
                case NetCode.Dating:
                    Date.ReadByteArr(buf, ref offs, buf.Length);
                    Dispatcher.Invoke(() => {
                        if (Date.sbArr != null)
                            txtWelcome.Text = Encoding.UTF32.GetString(Date.sbArr);
                    });
                    mState = NetCode.Authenticating;
                    break;
                case NetCode.Authenticating:
                    bool auth = false;
                    if (buf.Length == 4)
                        auth = BitConverter.ToInt32(buf, 0) == 1;
                    if (!auth)
                        Dispatcher.Invoke(() => { lblStatus.Text += "fail to auth, retry"; });
                    else
                    {
                        mState = NetCode.ExamRetrieving;
                        return true;//continue
                    }
                    break;
                case NetCode.ExamRetrieving:
                    offs = 0;
                    Question.ReadByteArr(buf, ref offs, buf.Length, false);
                    Dispatcher.Invoke(() =>
                    {
                        NavigationService.Navigate(new Uri("TakeExam.xaml", UriKind.Relative));
                    });
                    break;
            }
            return false;
        }

        public bool CliBufPrep(ref byte[] outBuf)
        {
            switch (mState)
            {
                case NetCode.Dating:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.Authenticating:
                    byte[] a = Encoding.UTF32.GetBytes(mBirdate);
                    outBuf = new byte[10 + a.Length];
                    Buffer.BlockCopy(BitConverter.GetBytes((int)mState), 0, outBuf, 0, 4);
                    if(mNeeId[0] == 'A')
                        Buffer.BlockCopy(BitConverter.GetBytes((int)ExamLvl.Basis), 0, outBuf, 4, 4);
                    else
                        Buffer.BlockCopy(BitConverter.GetBytes((int)ExamLvl.Advance), 0, outBuf, 4, 4);
                    int id;
                    if (int.TryParse(mNeeId.Substring(1), out id))
                        Buffer.BlockCopy(BitConverter.GetBytes(id), 0, outBuf, 8, 4);
                    Buffer.BlockCopy(a, 0, outBuf, 10, a.Length);
                    break;
                case NetCode.ExamRetrieving:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
            }
            return true;
        }

        private void UpdateSrvrMsg(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (mCbMsg.ToUp())
                Dispatcher.Invoke(() => {
                    lblStatus.Text += mCbMsg.txt;
                });
        }

        private void spMain_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void tbx_DMYKeyUp(object sender, KeyEventArgs e)
        {
            TextBox t = (TextBox)sender;
            if (t.Text.Length == 0)
            {
                if (!char.IsDigit(e.Key.ToString()[0]))
                {
                    txtDMYMsg.Text = "Digit only!";
                    e.Handled = true;
                }
            }
            else
            {
                if (tbxNeeId.Text.Length == 0 || tbxD.Text.Length == 0 ||
                    tbxM.Text.Length == 0 || tbxY.Text.Length == 0)
                    btnSignIn.IsEnabled = false;
                else
                    btnSignIn.IsEnabled = true;
            }
            mNeeId = t.Text;
        }

        private void tbx_NeeIdKeyUp(object sender, KeyEventArgs e)
        {
            TextBox t = (TextBox)sender;
            if (t.Text.Length == 0)
            {
                if (e.Key != Key.A && e.Key != Key.B)
                {
                    txtNeeIdMsg.Text = "Examinee Id must start with 'A' or 'B'!";
                    e.Handled = true;
                }
            }
            else
            {
                if (tbxD.Text.Length == 0 || tbxM.Text.Length == 0 ||
                    tbxY.Text.Length == 0)
                    btnSignIn.IsEnabled = false;
                else
                    btnSignIn.IsEnabled = true;
            }
            mBirdate = tbxD.Text + "/" + tbxM.Text + "/" + tbxY.Text;
        }
    }
}
