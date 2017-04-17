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
        Client2 mClnt;
        NetCode mState;
        UICbMsg mCbMsg;
        bool bRunning;
        ExamDate mDt;
        Examinee mNee;
        TakeExam pgTkExm;

        public Authentication()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            mState = NetCode.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;

            mDt = new ExamDate();
            mNee = new Examinee();

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void Connect(Object source, System.Timers.ElapsedEventArgs e)
        {
            Thread th = new Thread(() => { mClnt.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            mNee.tBirdate = tbxD.Text + "/" + tbxM.Text + "/" + tbxY.Text;
            DateTime dum;
            if (!DateTime.TryParse(mNee.tBirdate, out dum))
            {
                mNee.tBirdate = null;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.BIRDATE_NOTI]);
                return;
            }
            if(mNee.ParseTxId(tbxNeeId.Text))
            {
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.NEEID_NOTI]);
                return;
            }
            Thread th = new Thread(() => { mClnt.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            WPopup.s.cncl = false;
            mClnt.Close();
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
            WPopup.s.owner = w;

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
            int l;
            switch (mState)
            {
                case NetCode.Dating:
                    mDt.ReadByte(buf, ref offs);
                    Dispatcher.Invoke(() => {
                        if (mDt.mDt.Year == ExamDate.INVALID)
                            txtDate.Text = "No connection";
                        else
                            txtDate.Text = Txt.s._[(int)TxI.DATE] + mDt.mDt.ToString("dd/MM/yyyy HH:mm");
                    });
                    mState = NetCode.Authenticating;
                    break;
                case NetCode.Authenticating:
                    l = buf.Length - offs;
                    if (l < 1)
                        return false;
                    bool rs = BitConverter.ToBoolean(buf, offs);
                    ++offs;
                    if(rs)
                    {
                        rs = mNee.ReadByte(buf, ref offs);
                        l = buf.Length - offs;
                        if (!rs)
                        {
                            mState = NetCode.ExamRetrieving;
                            return true;//continue
                        }
                        else
                            return false;
                    }
                    else
                    {
                        if (l < 4)
                            break;
                        int errc = BitConverter.ToInt32(buf, offs);
                        offs += 4;
                        l -= 4;
                        string msg = null;
                        if (errc == (int)TxI.SIGNIN_AL_1)
                        {
                            if (!mNee.ReadByte(buf, ref offs))
                            {
                                msg = Txt.s._[(int)TxI.SIGNIN_AL_1] +
                                    mNee.dtTim1.ToString("HH:mm dd/MM/yyyy") + Txt.s._[(int)TxI.SIGNIN_AL_2] + mNee.tComp + ".";
                            }
                        }
                        else if (errc == (int)TxI.SIGNIN_NO)
                            msg = Txt.s._[(int)TxI.SIGNIN_NO];
                        if(msg != null)
                            Dispatcher.Invoke(() => {
                                WPopup.s.ShowDialog(msg);
                            });
                    }
                    break;
                case NetCode.ExamRetrieving:
                    offs = 0;
                    QuestSheet qs = new QuestSheet();
                    qs.ReadByte(buf, ref offs);
                    Dispatcher.Invoke(() =>
                    {
                        //NavigationService.Navigate(new Uri("TakeExam.xaml", UriKind.Relative));
                        pgTkExm = new TakeExam();
                        pgTkExm.mNee = mNee;
                        pgTkExm.mQSh = qs;
                        NavigationService.Navigate(pgTkExm);
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
                    mNee.ToByteSgning(out outBuf, (int)mState);
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
                Dispatcher.Invoke(() => { WPopup.s.ShowDialog(mCbMsg.txt); });
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}
