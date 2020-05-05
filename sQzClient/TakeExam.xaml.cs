using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Collections.Generic;
using sQzLib;

namespace sQzClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TakeExam : Page
    {
        DateTime kDtStart;
        TimeSpan dtRemn;
        DateTime dtLastLog;
        TimeSpan kLogIntvl;
        bool bRunning;
        bool bBtnBusy;
        UICbMsg mCbMsg;
        System.Timers.Timer mTimer;

        const int SMT_OK_M = 20;
        const int SMT_OK_S = 60;

        //models

        public QuestSheet QuestSheetModel;//may be only for hacking rendering test
        public ExamineeA mExaminee;//reference to Auth.mNee

        Client2 mClnt;
        NetPhase mState;

        public static double qaWh;

        public TakeExam()
        {
            InitializeComponent();
            mState = NetPhase.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;

            mExaminee = new ExamineeC();

            QuestSheetModel = new QuestSheet();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowFullScreen();

            HackingRenderingTest();

            SetAnswerSheetView();
            SetQuestSheetView();

            bBtnBusy = false;

            txtWelcome.Text = mExaminee.ToString();

            LoadTxt();

            

            int m = -1, s = -1;
            if (mExaminee.mPhase < ExamineePhase.Submitting)
            {
                string t = null;
                if(System.IO.File.Exists("Duration.txt"))
                {
                    string[] lines = System.IO.File.ReadAllLines("Duration.txt");
                    if (lines.Length > 0)
                        t = lines[0];
                }
                if (t != null)
                {
                    string[] vt = t.Split('\t');
                    if (vt.Length == 2)
                    {
                        int.TryParse(vt[0], out m);
                        int.TryParse(vt[1], out s);
                    }
                    if (-1 < m && -1 < s)
                        dtRemn = mExaminee.kDtDuration = new TimeSpan(0, m, s);
                }
            }
            if (m < 0 || s < 0)
                dtRemn = mExaminee.kDtDuration;
            txtRTime.Text = "" + dtRemn.Minutes + " : " + dtRemn.Seconds;
            kLogIntvl = new TimeSpan(0, 0, 30);

            System.Text.StringBuilder msg = new System.Text.StringBuilder();
            msg.Append(mExaminee.tId + " (" + mExaminee.tName + ")");
            if (mExaminee.kDtDuration.Minutes == 30)
                msg.Append(Txt.s._[(int)TxI.EXAMING_MSG_1]);
            else
                msg.AppendFormat(Txt.s._[(int)TxI.EXAMING_MSG_2],
                    mExaminee.kDtDuration.Minutes, mExaminee.kDtDuration.Seconds);
            PopupMgr.Singleton.CbOK = ShowQuestion;
            AppView.Opacity = 0.5;
            PopupMgr.Singleton.ShowDialog(msg.ToString());
            AppView.Opacity = 1;
            if (mExaminee.mPhase < ExamineePhase.Examing)
                mExaminee.mPhase = ExamineePhase.Examing;
            else if (mExaminee.mPhase == ExamineePhase.Submitting)
                Submit();
        }

        void ShowQuestion()
        {
            PopupMgr.Singleton.CbOK = null;
            AppView.Effect = null;
            bBtnBusy = false;
            QuestSheetBG.Visibility = Visibility.Visible;

            mTimer = new System.Timers.Timer(1000);
            mTimer.Elapsed += UpdateSrvrMsg;
            mTimer.AutoReset = true;
            mTimer.Enabled = true;
            dtLastLog = kDtStart = DateTime.Now;
        }

        public void Submit()
        {
            bBtnBusy = true;//
            AppView.Effect = null;
            PopupMgr.Singleton.CbOK = null;
            bRunning = false;
            DisableAll();
            mState = NetPhase.Submiting;
            mExaminee.mPhase = ExamineePhase.Submitting;
            mExaminee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
            if (mClnt.ConnectWR(ref mCbMsg))
                bBtnBusy = false;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            PopupMgr.Singleton.CbOK = Submit;
            AppView.Opacity = 0.5;
            PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.SUBMIT_CAUT],
                Txt.s._[(int)TxI.SUBMIT], Txt.s._[(int)TxI.BTN_CNCL], null);
            AppView.Opacity = 1;
        }

        public bool ClntBufHndl(byte[] buf)
        {
            int offs = 0;
            switch (mState)
            {
                case NetPhase.Submiting:
                    int rs;
                    string msg = null;
                    int l = buf.Length - offs;
                    if(l < 4)
                    {
                        rs = -1;
                        msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                    }
                    else
                        rs = BitConverter.ToInt32(buf, offs);
                    l -= 4;
                    offs += 4;
                    if(rs == 0)
                    {
                        ExamineeC e = new ExamineeC();
                        if (!e.ReadByte(buf, ref offs))
                        {
                            mExaminee.Merge(e);
                            btnSubmit.Content = mExaminee.Grade;
                            msg = Txt.s._[(int)TxI.RESULT] + mExaminee.Grade;
                        }
                        else
                            msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                    }
                    else if (rs == (int)TxI.NEEID_NF)
                        msg = Txt.s._[(int)TxI.NEEID_NF];
                    else if (rs == (int)TxI.RECV_DAT_ER)
                        msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                    else if(msg == null)
                    {
                        if(l < 4)
                            msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                        else
                        {
                            int sz = BitConverter.ToInt32(buf, offs);
                            l -= 4;
                            offs += 4;
                            if(l < sz)
                                msg = Txt.s._[(int)TxI.RECV_DAT_ER];
                            else
                                msg = System.Text.Encoding.UTF8.GetString(buf, offs, sz);
                        }
                    }
                    Dispatcher.Invoke(() => {
                        AppView.Opacity = 0.5;
                        PopupMgr.Singleton.ShowDialog(msg);
                        AppView.Opacity = 1;
                    });
                    break;
            }
            bBtnBusy = false;
            return false;
        }

        public byte[] ClntBufPrep()
        {
            byte[] outBuf;
            switch (mState)
            {
                case NetPhase.Submiting:
                    mExaminee.ToByte(out outBuf, (int)mState);
                    break;
                default:
                    outBuf = null;
                    break;
            }
            return outBuf;
        }

        private void UpdateSrvrMsg(object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning)
            {
                if (0 < dtRemn.Ticks)
                {
                    dtRemn = mExaminee.kDtDuration - (DateTime.Now - kDtStart);
                    if (mExaminee.mAnsSheet.bChanged && kLogIntvl < DateTime.Now - dtLastLog)
                    {
                        dtLastLog = DateTime.Now;
                        mExaminee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = dtRemn.Minutes.ToString() + " : " + dtRemn.Seconds;
                        if (!btnSubmit.IsEnabled && dtRemn.Minutes < SMT_OK_M
                                && dtRemn.Seconds < SMT_OK_S)
                            btnSubmit.IsEnabled = true;
                    });
                }
                else
                {
                    dtRemn = new TimeSpan(0, 0, 0);
                    bRunning = false;
                    Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = "0 : 0";
                        AppView.Opacity = 0.5;
                        PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.TIMEOUT]);
                        AppView.Opacity = 1;
                        Submit();
                    });
                }
            }
        }

        private void DisableAll()
        {
            btnSubmit.IsEnabled = false;
            mTimer.Stop();
            foreach (ListBox l in mExaminee.mAnsSheet.vlbxAns)
                l.IsEnabled = false;
            btnExit.IsEnabled = true;
        }

        void Exit()
        {
            //WPopup.s.wpCb = null;
            //bBtnBusy = false;
            //if (mExaminee.mAnsSheet.bChanged)
            //    mExaminee.ToLogFile(dtRemn.Minutes, dtRemn.Seconds);
            Window.GetWindow(this).Close();
        }

        void WPCancel()
        {
            bBtnBusy = false;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (bBtnBusy)
                return;
            bBtnBusy = true;
            PopupMgr.Singleton.CbOK = Exit;
            AppView.Opacity = 0.5;
            if (mExaminee.mPhase < ExamineePhase.Submitting)
                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.EXIT_CAUT_1],
                    Txt.s._[(int)TxI.EXIT], Txt.s._[(int)TxI.BTN_CNCL], "exit");
            else
                PopupMgr.Singleton.ShowDialog(Txt.s._[(int)TxI.EXIT_CAUT_2],
                    Txt.s._[(int)TxI.EXIT], Txt.s._[(int)TxI.BTN_CNCL], null);
            AppView.Opacity = 1;
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bRunning = false;
            mClnt.Close();
            PopupMgr.Singleton.Exit();
        }
    }
}
