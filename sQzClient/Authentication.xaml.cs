using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Threading;
using System.Windows.Input;
using sQzLib;
using System.Collections.Generic;

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
<<<<<<< HEAD
        ExamineeC thisExaminee;
=======
        ExamineeC User;
>>>>>>> master
        TakeExam pgTkExm;
        string RoomPassword;

        public Authentication()
        {
            App.EnableHookKeys(false);

            InitializeComponent();

            mState = NetPhase.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;

<<<<<<< HEAD
            mDt = DT.INVALID;
            thisExaminee = new ExamineeC();

            thisExaminee.kDtDuration = new TimeSpan(0, 59, 59);
=======
            mDt = DT.INV_;
            User = new ExamineeC();

            User.kDtDuration = new TimeSpan(1, 0, 0);
>>>>>>> master
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
            string trim_ID = tbxId.Text.Trim();
            if (trim_ID.Length > 8)
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._((int)TxI.NEEID_NOK));
                spMain.Opacity = 1;
=======
            try
            {
                User.ParseLvID(tbxId.Text);
            }
            catch(ArgumentException)
            {
                AppView.Opacity = 0.5;
                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.NEEID_NOK]);
                AppView.Opacity = 1;
>>>>>>> master
                return;
            }
            string trim_birthdate = tbxBirthdate.Text.Trim();
            if (trim_birthdate.Length > 10)
            {
<<<<<<< HEAD
                thisExaminee.Birthdate = null;
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._((int)TxI.BIRDATE_NOK));
                spMain.Opacity = 1;
                return;
            }

            RoomPassword = tbxRoomPassword.Text.Trim();
            if (RoomPassword.Length != 8)
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog("Password is incorrect!");
                spMain.Opacity = 1;
=======
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
>>>>>>> master
                return;
            }
            
            thisExaminee.ID = trim_ID;
            thisExaminee.Birthdate = trim_birthdate;
            try
            {
<<<<<<< HEAD
                thisExaminee.ComputerName = Environment.MachineName;
            }
            catch (InvalidOperationException)
            {
                thisExaminee.ComputerName = "unknown";
            }
=======
                User.tComp = Environment.MachineName;
            } catch(InvalidOperationException) { User.tComp = "unknown"; }//todo
>>>>>>> master
            DisableControls();
            Thread th = new Thread(() => {
                if (mClnt.ConnectWR(ref mCbMsg) && bRunning)
                    Dispatcher.Invoke(() =>
                    {
<<<<<<< HEAD
                        spMain.Opacity = 0.5;
                        WPopup.s.ShowDialog(Txt.s._((int)TxI.CONN_NOK));
                        spMain.Opacity = 1;
=======
                        AppView.Opacity = 0.5;
                        PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.CONN_NOK]);
                        AppView.Opacity = 1;
>>>>>>> master
                        DisableControls();
                        btnReconn.IsEnabled = true;
                    });
            });
            th.Start();
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
<<<<<<< HEAD
            WPopup.s.OwnerClosing = true;
=======
            PopupMgr.Singleton.IsOK = false;
>>>>>>> master
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
            txtLalgitc.Text = t._((int)TxI.LALGITC);
            txtWelcome.Text = t._((int)TxI.WELCOME);
            txtId.Text = t._((int)TxI.NEEID);
            txtBirdate.Text = t._((int)TxI.BIRDATE);
            txtRoomPassword.Text = t._((int)TxI.OP_PW);
            btnSignIn.Content = t._((int)TxI.SIGNIN);
            btnOpenLog.Content = t._((int)TxI.OPEN_LOG);
            btnReconn.Content = t._((int)TxI.CONN);
            btnExit.Content = t._((int)TxI.EXIT);
        }

        public bool ClntBufHndl(byte[] buf)
        {
            int offs = 0;
            int l, errc;
            switch (mState)
            {
<<<<<<< HEAD
                case NetCode.Dating:
                    if((mDt = DT.ReadByte(buf, ref offs)) != DT.INVALID && bRunning)
=======
                case NetPhase.Dating:
                    if(!DT.ReadByte(buf, ref offs, out mDt) && bRunning)
>>>>>>> master
                    {
                        if (buf.Length - offs < sizeof(long))
                        {
                            MessageBox.Show("Error data!");
                            return false;
                        }
                        thisExaminee.FullTestDuration = new TimeSpan(BitConverter.ToInt64(buf, offs));
                        offs += sizeof(long);

                        string subject = Utils.ReadBytesOfString(buf, ref offs);

                        Dispatcher.Invoke(() => {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(Txt.s._((int)TxI.DATE) + mDt.ToString(DT.RR));
                            if (subject != null)
                                sb.Append(". " + Txt.s._((int)TxI.SUBJECT) + ": " + subject + ". ");
                            sb.Append(Txt.s._((int)TxI.DURATION) + ": " +
                                Utils.GetMinutes(thisExaminee.FullTestDuration) +
                                " " + Txt.s._((int)TxI.MINUTE));
                            txtDate.Text = sb.ToString();
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
<<<<<<< HEAD
                        e.bLog = thisExaminee.bLog;
                        bool b = e.ReadBytes_FromS1(buf, ref offs);
                        l = buf.Length - offs;
                        if (!b)
                        {
                            thisExaminee.MergeWithS1(e);
                            if (!thisExaminee.bLog)
                                thisExaminee.kDtDuration = thisExaminee.FullTestDuration;
                            mState = NetCode.ExamRetrieving;
=======
                        e.bLog = User.bLog;
                        bool b = e.ReadByte(buf, ref offs);
                        l = buf.Length - offs;
                        if (!b)
                        {
                            User.Merge(e);
                            mState = NetPhase.ExamRetrieving;
>>>>>>> master
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
                            sb.AppendFormat(Txt.s._((int)TxI.SIGNIN_AL), h.ToString() + ':' + m);
                            sb.Append(comp + '.');
                            msg = sb.ToString();
                        }
                        else if (errc == (int)TxI.SIGNIN_NOK)
                            msg = Txt.s._((int)TxI.SIGNIN_NOK);
                        else if (errc == (int)TxI.NEEID_NF)
                            msg = Txt.s._((int)TxI.NEEID_NF);
                        else if (errc == (int)TxI.RECV_DAT_ER)
                            msg = Txt.s._((int)TxI.RECV_DAT_ER);
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
<<<<<<< HEAD
                                spMain.Opacity = 0.5;
                                WPopup.s.ShowDialog(Txt.s._((int)TxI.QS_NFOUND) + qsid);
                                spMain.Opacity = 1;
=======
                                AppView.Opacity = 0.5;
                                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.QS_NFOUND] + qsid);
                                AppView.Opacity = 1;
>>>>>>> master
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
<<<<<<< HEAD
                                spMain.Opacity = 0.5;
                                WPopup.s.ShowDialog(Txt.s._((int)TxI.QS_READ_ER));
                                spMain.Opacity = 1;
=======
                                AppView.Opacity = 0.5;
                                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.QS_READ_ER]);
                                AppView.Opacity = 1;
>>>>>>> master
                                EnableControls();
                            });
                        break;
                    }
                    if(bRunning)
                        Dispatcher.Invoke(() =>
                        {
                            pgTkExm = new TakeExam();
<<<<<<< HEAD
                            pgTkExm.thisExaminee = thisExaminee;
                            pgTkExm.QuestionSheet = qs;
=======
                            pgTkExm.mExaminee = User;
                            pgTkExm.QuestSheetModel = qs;
>>>>>>> master
                            NavigationService.Navigate(pgTkExm);
                        });
                    break;
            }
            return false;
        }

        public byte[] ClntBufPrep()
        {
            byte[] outBuf;
            List<byte[]> bytes;
            switch (mState)
            {
                case NetPhase.Dating:
                    outBuf = BitConverter.GetBytes((int)mState);
                    break;
<<<<<<< HEAD
                case NetCode.Authenticating:
                    bytes = new List<byte[]>();
                    bytes.Add(BitConverter.GetBytes((int)mState));
                    Utils.AppendBytesOfString(RoomPassword, bytes);
                    bytes.AddRange(thisExaminee.GetBytes_SendingToS1());
                    outBuf = Utils.ToArray_FromListOfBytes(bytes);
                    break;
                case NetCode.ExamRetrieving:
                    bytes = new List<byte[]>();
                    bytes.Add(BitConverter.GetBytes((int)mState));
                    Utils.AppendBytesOfString(thisExaminee.ID, bytes);
                    bytes.Add(BitConverter.GetBytes(thisExaminee.AnswerSheet.QuestSheetID));
                    outBuf = Utils.ToArray_FromListOfBytes(bytes);
=======
                case NetPhase.Authenticating:
                    User.ToByte(out outBuf, (int)mState);
                    break;
                case NetPhase.ExamRetrieving:
                    outBuf = new byte[12];
                    Buffer.BlockCopy(BitConverter.GetBytes((int)mState), 0, outBuf, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(User.LvId), 0, outBuf, 4, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(User.mAnsSheet.uQSId), 0, outBuf, 8, 4);
>>>>>>> master
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
<<<<<<< HEAD
                if(thisExaminee.ReadLogFile(filePath))
                {
                    tbxId.Text = thisExaminee.ID;
                    WPopup.s.ShowDialog(Txt.s._((int)TxI.OPEN_LOG_OK));
                }
                else
                    WPopup.s.ShowDialog(Txt.s._((int)TxI.OPEN_LOG_OK));
=======
                if(User.ReadLogFile(filePath))
                {
                    tbxId.Text = User.tId;
                    PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.OPEN_LOG_OK]);
                }
                else
                    PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.OPEN_LOG_OK]);
>>>>>>> master
            }
            AppView.Opacity = 1;
            //EnableControls();
        }

        private void EnableControls()
        {
            tbxId.IsEnabled =
            tbxBirthdate.IsEnabled =
            tbxRoomPassword.IsEnabled =
            btnOpenLog.IsEnabled =
            btnSignIn.IsEnabled = true;
        }

        private void DisableControls()
        {
            tbxId.IsEnabled =
            tbxBirthdate.IsEnabled =
            tbxRoomPassword.IsEnabled =
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
<<<<<<< HEAD
                        spMain.Opacity = 0.5;
                        WPopup.s.ShowDialog(Txt.s._((int)TxI.CONN_NOK));
                        spMain.Opacity = 1;
=======
                        AppView.Opacity = 0.5;
                        PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.CONN_NOK]);
                        AppView.Opacity = 1;
>>>>>>> master
                        btnReconn.IsEnabled = true;
                    });
            });
            th.Start();
        }
    }
}
