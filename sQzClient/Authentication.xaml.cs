using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Threading;
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
        DateTime mDt;
        ExamineeC mNee;
        TakeExam pgTkExm;

        public Authentication()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            mState = NetCode.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;

            mDt = DtFmt.INV_;
            mNee = new ExamineeC();

            mNee.kDtDuration = new TimeSpan(1, 0, 0);
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            if (mNee.ParseTxId(tbxId.Text))
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.NEEID_NOK]);
                spMain.Opacity = 1;
                return;
            }
            mNee.tBirdate = tbxD.Text + "-" + tbxM.Text + "-" + tbxY.Text;
            DateTime dum;
            if (!DateTime.TryParse(mNee.tBirdate, out dum))
            {
                mNee.tBirdate = null;
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.BIRDATE_NOK]);
                spMain.Opacity = 1;
                return;
            }
            try
            {
                mNee.tComp = Environment.MachineName;
            } catch(InvalidOperationException) { mNee.tComp = "unknown"; }//todo
            DisableControls();
            Thread th = new Thread(() => {
                if (mClnt.ConnectWR(ref mCbMsg) && bRunning)
                    Dispatcher.Invoke(() =>
                    {
                        spMain.Opacity = 0.5;
                        WPopup.s.ShowDialog(Txt.s._[(int)TxI.CONN_NOK]);
                        spMain.Opacity = 1;
                        DisableControls();
                        btnReconn.IsEnabled = true;
                    });
            });
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
                case NetCode.Dating:
                    if(!ExamBoard.ReadByteDt(buf, ref offs, out mDt) && bRunning)
                    {
                        Dispatcher.Invoke(() => {
                            txtDate.Text = Txt.s._[(int)TxI.DATE] + mDt.ToString(DtFmt.RR);
                            EnableControls();
                            btnReconn.IsEnabled = false;
                        });
                        mState = NetCode.Authenticating;
                    }
                    break;
                case NetCode.Authenticating:
                    l = buf.Length - offs;
                    if (l < 4)
                        break;
                    errc = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    if(errc == 0)
                    {
                        ExamineeC e = new ExamineeC();
                        e.bLog = mNee.bLog;
                        bool b = e.ReadByte(buf, ref offs);
                        l = buf.Length - offs;
                        if (!b)
                        {
                            mNee.Merge(e);
                            mState = NetCode.ExamRetrieving;
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
                            sb.Append(mNee.tComp + '.');
                            msg = sb.ToString();
                        }
                        else if (errc == (int)TxI.SIGNIN_NOK)
                            msg = Txt.s._[(int)TxI.SIGNIN_NOK];
                        else if (errc == (int)TxI.NEEID_NF)
                            msg = Txt.s._[(int)TxI.NEEID_NF];
                        if (bRunning && msg != null)
                            Dispatcher.Invoke(() => {
                                spMain.Opacity = 0.5;
                                WPopup.s.ShowDialog(msg);
                                spMain.Opacity = 1;
                                EnableControls();
                            });
                    }
                    break;
                case NetCode.ExamRetrieving:
                    errc = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    if(errc == (int)TxI.QS_NFOUND)
                    {
                        mState = NetCode.Authenticating;
                        int qsid = BitConverter.ToInt32(buf, offs);
                        offs += 4;
                        if (bRunning)
                            Dispatcher.Invoke(() =>
                            {
                                spMain.Opacity = 0.5;
                                WPopup.s.ShowDialog(Txt.s._[(int)TxI.QS_NFOUND] + qsid);
                                spMain.Opacity = 1;
                                EnableControls();
                            });
                        break;
                    }
                    QuestSheet qs = new QuestSheet();
                    if (qs.ReadByte(buf, ref offs))
                    {
                        mState = NetCode.Authenticating;
                        if(bRunning)
                            Dispatcher.Invoke(() =>
                            {
                                spMain.Opacity = 0.5;
                                WPopup.s.ShowDialog(Txt.s._[(int)TxI.QS_READ_ER]);
                                spMain.Opacity = 1;
                                EnableControls();
                            });
                        break;
                    }
                    if(bRunning)
                        Dispatcher.Invoke(() =>
                        {
                            pgTkExm = new TakeExam();
                            pgTkExm.mNee = mNee;
                            pgTkExm.mQSh = qs;
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
                case NetCode.Dating:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
                case NetCode.Authenticating:
                    mNee.ToByte(out outBuf, (int)mState);
                    break;
                case NetCode.ExamRetrieving:
                    outBuf = new byte[12];
                    Buffer.BlockCopy(BitConverter.GetBytes((int)mState), 0, outBuf, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(mNee.uId), 0, outBuf, 4, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(mNee.mAnsSh.uQSId), 0, outBuf, 8, 4);
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
            spMain.Opacity = 0.5;
            bool? result = dlg.ShowDialog();

            string filePath = null;
            if (result == true)
                filePath = dlg.FileName;
            if (filePath != null)
            {
                if(mNee.ReadLogFile(filePath))
                {
                    tbxId.Text = mNee.tId;
                    WPopup.s.ShowDialog(Txt.s._[(int)TxI.OPEN_LOG_OK]);
                }
                else
                    WPopup.s.ShowDialog(Txt.s._[(int)TxI.OPEN_LOG_OK]);
            }
            spMain.Opacity = 1;
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

        private void btnReconn_Click(object sender, RoutedEventArgs e)
        {
            btnReconn.IsEnabled = false;
            Thread th = new Thread(() => {
                if (mClnt.ConnectWR(ref mCbMsg) && bRunning)
                    Dispatcher.Invoke(() =>
                    {
                        spMain.Opacity = 0.5;
                        WPopup.s.ShowDialog(Txt.s._[(int)TxI.CONN_NOK]);
                        spMain.Opacity = 1;
                        btnReconn.IsEnabled = true;
                    });
            });
            th.Start();
        }
    }
}
