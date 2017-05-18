using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading;
using System.Windows.Media.Effects;
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
        bool bBtnBusy;
        DateTime mDt;
        ExamineeC mNee;
        TakeExam pgTkExm;
        BlurEffect mBlurEff;

        public Authentication()
        {
            InitializeComponent();

            ShowsNavigationUI = false;

            mState = NetCode.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;
            bBtnBusy = false;

            mDt = ExamSlot.INVALID_DT;
            mNee = new ExamineeC();

            mNee.kDtDuration = new TimeSpan(1, 0, 0);

            System.Timers.Timer aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateSrvrMsg;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            if (mNee.ParseTxId(tbxId.Text))
            {
                spMain.Effect = mBlurEff;
                WPopup.s.wpCb = Deblur;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.NEEID_NOK]);
                return;
            }
            mNee.tBirdate = tbxD.Text + "/" + tbxM.Text + "/" + tbxY.Text;
            DateTime dum;
            if (!DateTime.TryParse(mNee.tBirdate, out dum))
            {
                mNee.tBirdate = null;
                spMain.Effect = mBlurEff;
                WPopup.s.wpCb = Deblur;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.BIRDATE_NOK]);
                return;
            }
            try
            {
                mNee.tComp = Environment.MachineName;
            } catch(InvalidOperationException) { mNee.tComp = "unknown"; }//todo
            Thread th = new Thread(() => { mClnt.ConnectWR(ref mCbMsg); });
            th.Start();
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            bBtnBusy = true;
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

            mBlurEff = new BlurEffect();

            Thread th = new Thread(() => {
                if (mClnt.ConnectWR(ref mCbMsg))
                    EnableControls();
                else if(bRunning)
                {
                    Dispatcher.Invoke(() => {
                        btnReconn.IsEnabled = true;
                        spMain.Effect = mBlurEff;
                        WPopup.s.wpCb = Deblur;
                        WPopup.s.ShowDialog(Txt.s._[(int)TxI.CONN_NOK]);});
                }});
            th.Start();
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

        public bool ClntBufHndl(byte[] buf, int offs)
        {
            int l, errc;
            switch (mState)
            {
                case NetCode.Dating:
                    ExamSlot.ReadByteDt(buf, ref offs, out mDt);
                    if(bRunning)
                        Dispatcher.Invoke(() => {
                            txtDate.Text = Txt.s._[(int)TxI.DATE] + mDt.ToString(ExamSlot.FORM_RH);
                        });
                    mState = NetCode.Authenticating;
                    break;
                case NetCode.Authenticating:
                    l = buf.Length - offs;
                    if (l < 1)
                        break;
                    bool rs = BitConverter.ToBoolean(buf, offs);
                    ++offs;
                    if(rs)
                    {
                        ExamineeC e = new ExamineeC();
                        e.bLog = mNee.bLog;
                        rs = e.ReadByte(buf, ref offs);
                        l = buf.Length - offs;
                        if (!rs)
                        {
                            mNee.Merge(e);
                            mState = NetCode.ExamRetrieving;
                            return true;//continue
                        }
                    }
                    else
                    {
                        if (l < 4)
                            break;
                        errc = BitConverter.ToInt32(buf, offs);
                        offs += 4;
                        l -= 4;
                        string msg = null;
                        if (errc == (int)TxI.SIGNIN_AL_1)
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
                            msg = Txt.s._[(int)TxI.SIGNIN_AL_1] +
                                h + ':' + m + Txt.s._[(int)TxI.SIGNIN_AL_2] + mNee.tComp + ".";
                        }
                        else if (errc == (int)TxI.SIGNIN_NOK)
                            msg = Txt.s._[(int)TxI.SIGNIN_NOK];
                        if(bRunning && msg != null)
                            Dispatcher.Invoke(() => {
                                spMain.Effect = mBlurEff;
                                WPopup.s.wpCb = Deblur;
                                WPopup.s.ShowDialog(msg);
                            });
                    }
                    break;
                case NetCode.ExamRetrieving:
                    errc = BitConverter.ToInt32(buf, offs);
                    offs += 4;
                    if(errc == (int)TxI.QS_NFOUND)
                    {
                        mState = NetCode.Authenticating;
                        if(bRunning)
                            Dispatcher.Invoke(() =>
                            {
                                spMain.Effect = mBlurEff;
                                WPopup.s.wpCb = Deblur;
                                WPopup.s.ShowDialog(Txt.s._[(int)TxI.QS_NFOUND]);
                            });
                        break;
                    }
                    QuestSheet qs = new QuestSheet();
                    if (qs.ReadByte(buf, ref offs))
                    {
                        mState = NetCode.Authenticating;
                        spMain.Effect = mBlurEff;
                        WPopup.s.wpCb = Deblur;
                        if(bRunning)
                            Dispatcher.Invoke(() =>
                            {
                                spMain.Effect = mBlurEff;
                                WPopup.s.wpCb = Deblur;
                                WPopup.s.ShowDialog(Txt.s._[(int)TxI.QS_READ_ER]);
                            });
                        break;
                    }
                    if(bRunning)
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
            bBtnBusy = false;
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
                    mNee.ToByte(out outBuf, (int)mState);
                    break;
                case NetCode.ExamRetrieving:
                    outBuf = new byte[6];//hardcode
                    Buffer.BlockCopy(BitConverter.GetBytes((int)mState), 0, outBuf, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(mNee.mAnsSh.uQSId), 0, outBuf, 4, 2);
                    break;
            }
            return true;
        }

        private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            //if (bRunning && mCbMsg.ToUp())
            //    Dispatcher.Invoke(() => {
            //        //WPopup.s.ShowDialog(mCbMsg.txt);
            //        lblStatus.Text += mCbMsg.txt;
            //    });
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            Window.GetWindow(this).Close();
        }

        private void btnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // set filter for file extension and default file extension 
            //dlg.DefaultExt = ".bin";
            //dlg.Filter = "binary file (*.bin)|*.bin";
            bool? result = dlg.ShowDialog();

            string filePath = null;
            if (result == true)
                filePath = dlg.FileName;
            if (filePath != null && mNee.ReadLogFile(filePath))
            {
                tbxId.Text = mNee.tId;
                spMain.Effect = mBlurEff;
                WPopup.s.wpCb = Deblur;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.OPEN_LOG_OK]);
            }
        }

        private void EnableControls()
        {
            if(bRunning)
                Dispatcher.Invoke(() =>
                {
                    tbxId.IsEnabled =
                    tbxD.IsEnabled =
                    tbxM.IsEnabled =
                    tbxY.IsEnabled =
                    btnOpenLog.IsEnabled =
                    btnSignIn.IsEnabled = true;
                    btnReconn.IsEnabled = false;
                });
        }

        private void DisableControls()
        {
            if(bRunning)
                Dispatcher.Invoke(() =>
                {
                    tbxId.IsEnabled =
                    tbxD.IsEnabled =
                    tbxM.IsEnabled =
                    tbxY.IsEnabled =
                    btnOpenLog.IsEnabled =
                    btnReconn.IsEnabled =
                    btnSignIn.IsEnabled = false;
                });
        }

        private void btnReconn_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            Thread th = new Thread(() => {
                if (mClnt.ConnectWR(ref mCbMsg))
                    EnableControls();
                else
                {
                    if(bRunning)
                        Dispatcher.Invoke(() => {
                            btnReconn.IsEnabled = true;
                            spMain.Effect = mBlurEff;
                            WPopup.s.wpCb = Deblur;
                            WPopup.s.ShowDialog(Txt.s._[(int)TxI.CONN_NOK]);
                        });
                }
            });
            th.Start();
        }

        private void Deblur()
        {
            spMain.Effect = null;
            bBtnBusy = false;
        }
    }
}
