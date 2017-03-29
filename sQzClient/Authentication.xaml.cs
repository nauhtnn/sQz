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
        bool bRunning;
        string mNeeId;
        string mBirdate;

        public Authentication()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            mState = NetCode.Dating;
            mClient = new Client2(ClntBufHndl, ClntBufPrep);
            mCbMsg = new UICbMsg();
            bRunning = true;

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
            DateTime dummy;
            if (!DateTime.TryParse(mBirdate, out dummy))
            {
                WPopup.ShowDialog(Txt.s._[(int)TxI.BIRDATE_NOTI]);
                return;
            }
            ExamLvl lv = ExamLvl.Basis;
            ushort id = ushort.MaxValue;
            if(!Examinee.ToID(mNeeId, ref lv, ref id))
            {
                WPopup.ShowDialog(Txt.s._[(int)TxI.NEEID_NOTI]);
                return;
            }
            Thread th = new Thread(() => { mClient.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            mClient.Close();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 20;

            LoadTxt();

            //double rt = w.RenderSize.Width / 1280; //design size
            //ScaleTransform st = new ScaleTransform(rt, rt);
            //spMain.RenderTransform = st;
            WPopup.Config(w, w.FontSize);

            FirewallHandler fwHndl = new FirewallHandler(3);
            lblStatus.Text += fwHndl.OpenFirewall();

            Connect(null, null);
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            txtLalgitc.Text = t._[(int)TxI.LALGITC];
            txtWelcome.Text = t._[(int)TxI.WELCOME];
            txtNeeId.Text = t._[(int)TxI.NEEID];
            txtBirdate.Text = t._[(int)TxI.BIRDATE] + t._[(int)TxI.BIRDATE_MSG];
            btnSignIn.Content = t._[(int)TxI.SIGNIN];
            btnExit.Content = t._[(int)TxI.EXIT];
        }

        public bool ClntBufHndl(byte[] buf, int offs)
        {
            switch (mState)
            {
                case NetCode.Dating:
                    Date.ReadByteArr(buf, ref offs, buf.Length);
                    Dispatcher.Invoke(() => {
                        if (Date.sbArr != null)
                            txtDate.Text = Txt.s._[(int)TxI.DATE] + Encoding.UTF32.GetString(Date.sbArr);
                        else
                            txtDate.Text = "No connection";
                    });
                    mState = NetCode.Authenticating;
                    break;
                case NetCode.Authenticating:
                    bool rs = Examinee.CliReadAuthArr(buf, offs, out Examinee.sAuthNee);
                    if(rs)
                    {
                        mState = NetCode.ExamRetrieving;
                        return true;//continue
                    }
                    else
                    {
                        ++offs;//todo
                        int sz = BitConverter.ToInt32(buf, offs);
                        offs += 4;
                        string txt = Encoding.UTF32.GetString(buf, offs, sz);
                        Dispatcher.Invoke(() => {
                            WPopup.ShowDialog(txt);
                        });
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

        public bool ClntBufPrep(ref byte[] outBuf)
        {
            switch (mState)
            {
                case NetCode.Dating:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.Authenticating:
                    Examinee.CliToAuthArr(out outBuf, (int)mState, mNeeId, mBirdate);
                    break;
                case NetCode.ExamRetrieving:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
            }
            return true;
        }

        private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning && mCbMsg.ToUp())
                Dispatcher.Invoke(() => { WPopup.ShowDialog(mCbMsg.txt); });
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}
