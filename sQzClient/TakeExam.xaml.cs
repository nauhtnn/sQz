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
		System.Timers.Timer TimeController;
        UICbMsg mCbMsg;
        
        public TakeExam()
        {
            InitializeComponent();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
			InitModels();
			InitViews();

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
			
			InitTimeController();
        }
		
		void InitTimeController()
		{
			TimeController = new System.Timers.Timer(1000);
            TimeController.Elapsed += TimeControllerUpdate;
            TimeController.AutoReset = true;
            TimeController.Enabled = true;
            StartingTime = DateTime.Now;
			LastBackupTime = StartingTime;
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
            mExaminee.ToLogFile(RemainingTime.Minutes, RemainingTime.Seconds);
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

        private void TimeControllerUpdate(object source, System.Timers.ElapsedEventArgs e)
        {
            if (bRunning)
            {
                if (0 < RemainingTime.Ticks)
                {
                    RemainingTime = mExaminee.kDtDuration - (DateTime.Now - StartingTime);
                    if (mExaminee.mAnsSheet.bChanged && BackupInterval < DateTime.Now - LastBackupTime)
                    {
                        LastBackupTime = DateTime.Now;
                        mExaminee.ToLogFile(RemainingTime.Minutes, RemainingTime.Seconds);
                    }
                    Dispatcher.Invoke(() =>
                    {
                        txtRTime.Text = RemainingTime.Minutes.ToString() + " : " + RemainingTime.Seconds;
                        if (!btnSubmit.IsEnabled && RemainingTime.Minutes < SMT_OK_M
                                && RemainingTime.Seconds < SMT_OK_S)
                            btnSubmit.IsEnabled = true;
                    });
                }
                else
                {
                    RemainingTime = new TimeSpan(0, 0, 0);
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
            TimeController.Stop();
            foreach (ListBox l in mExaminee.mAnsSheet.vlbxAns)
                l.IsEnabled = false;
            btnExit.IsEnabled = true;
        }

        void Exit()
        {
            //WPopup.s.wpCb = null;
            //bBtnBusy = false;
            //if (mExaminee.mAnsSheet.bChanged)
            //    mExaminee.ToLogFile(RemainingTime.Minutes, RemainingTime.Seconds);
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
		
		public void Options_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //bChanged = true;
            ListBox l = sender as ListBox;
            if (l.SelectedItem == null)
                return;
            int qid = Convert.ToInt32(l.Name.Substring(1));
            int i = -1;
            foreach (ListBoxItem li in l.Items)
            {
                ++i;
                if (li.IsSelected)
                {
                    //aAns[qid * 4 + i] = 1;//todo
                    //vAnsItem[qid][i].Selected();
                }
                else
                {
                    //aAns[qid * 4 + i] = 0;//todo
                    //vAnsItem[qid][i].Unselected();
                }
            }
            //dgSelChgCB?.Invoke();
        }
    }
}
