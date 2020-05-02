using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Threading;
using System.Windows.Input;
using sQzLib;

namespace sQzClient
{
    /// <summary>
    /// Interaction logic for Authentication.xaml
    /// </summary>
    public partial class Authentication : Page
    {
        Client2 mClnt;
        NetPhase mState;
        UICbMsg mCbMsg;
        bool bRunning;
        DateTime mDt;
        ExamineeC User;
        TakeExam pgTkExm;

        public Authentication()
        {
            InitializeComponent();

            mState = NetPhase.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;

            mDt = DT.INV_;
            User = new ExamineeC();

            User.kDtDuration = new TimeSpan(1, 0, 0);
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                User.ParseLvID(tbxId.Text);
            }
            catch(ArgumentException)
            {
                AppView.Opacity = 0.5;
                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.NEEID_NOK]);
                AppView.Opacity = 1;
                return;
            }
            int x, y, z;
            if(!int.TryParse(tbxD.Text, out x) || !int.TryParse(tbxM.Text, out y)
                || !int.TryParse(tbxY.Text, out z))
            {
                User.tBirdate = null;
                AppView.Opacity = 0.5;
                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.BIRDATE_NOK]);
                AppView.Opacity = 1;
                return;
            }
            User.tBirdate = x.ToString("d2") + "-" + y.ToString("d2") + "-" + z.ToString("d2");
            DateTime dum;
            if (DT.To_(User.tBirdate, DT.RR, out dum))
            {
                User.tBirdate = null;
                AppView.Opacity = 0.5;
                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.BIRDATE_NOK]);
                AppView.Opacity = 1;
                return;
            }
            try
            {
                User.tComp = Environment.MachineName;
            } catch(InvalidOperationException) { User.tComp = "unknown"; }//todo
            DisableControls();
            Thread th = new Thread(() => {
                if (mClnt.ConnectWR(ref mCbMsg) && bRunning)
                    Dispatcher.Invoke(() =>
                    {
                        AppView.Opacity = 0.5;
                        PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.CONN_NOK]);
                        AppView.Opacity = 1;
                        DisableControls();
                        btnReconn.IsEnabled = true;
                    });
            });
            th.Start();
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            PopupMgr.Singleton.IsOK = false;
            mClnt.Close();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 28;

            LoadTxt();

            PopupMgr.Singleton.ParentWindow = w;

            //FirewallHandler fwHndl = new FirewallHandler(3);
            //lblStatus.Text += fwHndl.OpenFirewall();

            btnReconn_Click(null, null);
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            txtLalgitc.Text = t._[(int)TxI.LALGITC];
            txtWelcome.Text = t._[(int)TxI.WELCOME];
            txtId.Text = t._[(int)TxI.NEEID];
            txtBirdate.Text = t._[(int)TxI.BIRDATE] + t._[(int)TxI.BIRDATE_MSG];
            btnSignIn.Content = t._[(int)TxI.SIGNIN];
            btnOpenLog.Content = t._[(int)TxI.OPEN_LOG];
            btnReconn.Content = t._[(int)TxI.CONN];
            btnExit.Content = t._[(int)TxI.EXIT];
        }

        public bool ClntBufHndl(byte[] buf)
        {
            int offs = 0;
            int l, errc;
            switch (mState)
            {
                case NetPhase.Dating:
                    if(!DT.ReadByte(buf, ref offs, out mDt) && bRunning)
                    {
                        Dispatcher.Invoke(() => {
                            txtDate.Text = Txt.s._[(int)TxI.DATE] + mDt.ToString(DT.RR);
                            EnableControls();
                            btnReconn.IsEnabled = false;
                        });
                        mState = NetPhase.Authenticating;
                    }
                    break;
                case NetPhase.Authenticating:
                    l = buf.Length - offs;
                    if (l < 4)
                        break;
                    errc = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    if(errc == 0)
                    {
                        ExamineeC e = new ExamineeC();
                        e.bLog = User.bLog;
                        bool b = e.ReadByte(buf, ref offs);
                        l = buf.Length - offs;
                        if (!b)
                        {
                            User.Merge(e);
                            mState = NetPhase.ExamRetrieving;
                            return true;//continue
                        }
                    }
                    else
                    {
                        string msg = null;
                        if (errc == (int)TxI.SIGNIN_AL)
                        {
                            if (l < 4)
                                break;
                            int sz = BitConverter.ToInt32(buf, offs);
                            l -= 4;
                            offs += 4;
                            if (l < sz)
                                break;
                            string comp = "";
                            if (0 < sz)
                            {
                                comp = Encoding.UTF8.GetString(buf, offs, sz);
                                l -= sz;
                                offs += sz;
                            }
                            if (l < 8)
                                break;
                            int h = BitConverter.ToInt32(buf, offs);
                            offs += 4;
                            int m = BitConverter.ToInt32(buf, offs);
                            offs += 4;
                            l -= 8;
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat(Txt.s._[(int)TxI.SIGNIN_AL], h.ToString() + ':' + m);
                            sb.Append(comp + '.');
                            msg = sb.ToString();
                        }
                        else if (errc == (int)TxI.SIGNIN_NOK)
                            msg = Txt.s._[(int)TxI.SIGNIN_NOK];
                        else if (errc == (int)TxI.NEEID_NF)
                            msg = Txt.s._[(int)TxI.NEEID_NF];
                        else if (errc == (int)TxI.RECV_DAT_ER)
                            msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                        if (bRunning && msg != null)
                            Dispatcher.Invoke(() => {
                                AppView.Opacity = 0.5;
                                PopupMgr.Singleton.ShowDialog(msg);
                                AppView.Opacity = 1;
                                EnableControls();
                            });
                    }
                    break;
                case NetPhase.ExamRetrieving:
                    errc = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    if(errc == (int)TxI.QS_NFOUND)
                    {
                        mState = NetPhase.Authenticating;
                        int qsid = BitConverter.ToInt32(buf, offs);
                        offs += 4;
                        if (bRunning)
                            Dispatcher.Invoke(() =>
                            {
                                AppView.Opacity = 0.5;
                                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.QS_NFOUND] + qsid);
                                AppView.Opacity = 1;
                                EnableControls();
                            });
                        break;
                    }
                    QuestSheet qs = new QuestSheet();
                    if (qs.ReadByte(buf, ref offs))
                    {
                        mState = NetPhase.Authenticating;
                        if(bRunning)
                            Dispatcher.Invoke(() =>
                            {
                                AppView.Opacity = 0.5;
                                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.QS_READ_ER]);
                                AppView.Opacity = 1;
                                EnableControls();
                            });
                        break;
                    }
                    if(bRunning)
                        Dispatcher.Invoke(() =>
                        {
                            pgTkExm = new TakeExam();
                            pgTkExm.mExaminee = User;
                            pgTkExm.mQuestSheet = qs;
                            NavigationService.Navigate(pgTkExm);
                        });
                    break;
            }
            return false;
        }

        public byte[] ClntBufPrep()
        {
            byte[] outBuf;
            switch (mState)
            {
                case NetPhase.Dating:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetPhase.Authenticating:
                    User.ToByte(out outBuf, (int)mState);
                    break;
                case NetPhase.ExamRetrieving:
                    outBuf = new byte[12];
                    Buffer.BlockCopy(BitConverter.GetBytes((int)mState), 0, outBuf, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(User.LvId), 0, outBuf, 4, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(User.mAnsSheet.uQSId), 0, outBuf, 8, 4);
                    break;
                default:
                    outBuf = null;
                    break;
            }
            return outBuf;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            DisableControls();
            Window.GetWindow(this).Close();
        }

        private void btnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            //DisableControls();
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // set filter for file extension and default file extension 
            //dlg.DefaultExt = ".bin";
            //dlg.Filter = "binary file (*.bin)|*.bin";
            AppView.Opacity = 0.5;
            bool? result = dlg.ShowDialog();

            string filePath = null;
            if (result == true)
                filePath = dlg.FileName;
            if (filePath != null)
            {
                if(User.ReadLogFile(filePath))
                {
                    tbxId.Text = User.tId;
                    PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.OPEN_LOG_OK]);
                }
                else
                    PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.OPEN_LOG_OK]);
            }
            AppView.Opacity = 1;
            //EnableControls();
        }

        private void EnableControls()
        {
            tbxId.IsEnabled =
            tbxD.IsEnabled =
            tbxM.IsEnabled =
            tbxY.IsEnabled =
            btnOpenLog.IsEnabled =
            btnSignIn.IsEnabled = true;
        }

        private void DisableControls()
        {
            tbxId.IsEnabled =
            tbxD.IsEnabled =
            tbxM.IsEnabled =
            tbxY.IsEnabled =
            btnOpenLog.IsEnabled =
            btnSignIn.IsEnabled = false;
        }

        private void tbx_PrevwNumberOnly(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete && e.Key != Key.Back && e.Key != Key.Tab &&
                ((int)e.Key < (int)Key.Left || (int)Key.Down < (int)e.Key) &&
                ((int)e.Key < (int)Key.D0 || (int)Key.D9 < (int)e.Key))
                e.Handled = true;
        }

        private void btnReconn_Click(object sender, RoutedEventArgs e)
        {
            btnReconn.IsEnabled = false;
            Thread th = new Thread(() => {
                if (mClnt.ConnectWR(ref mCbMsg) && bRunning)
                    Dispatcher.Invoke(() =>
                    {
                        AppView.Opacity = 0.5;
                        PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.CONN_NOK]);
                        AppView.Opacity = 1;
                        btnReconn.IsEnabled = true;
                    });
            });
            th.Start();
        }
    }
}
