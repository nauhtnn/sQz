using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Collections.Generic;
using sQzLib;

namespace sQzClient
{
    public partial class TakeExam : Page
    {
        DateTime StartingTime;
        TimeSpan RemainingTime;
        DateTime LastBackupTime;
        TimeSpan BackupInterval;
        bool bRunning;
        bool bBtnBusy;

        const int SMT_OK_M = 20;
        const int SMT_OK_S = 60;

        //models

        public QuestSheet QuestSheetModel;//may be only for hacking rendering test
        public ExamineeA mExaminee;//reference to Auth.mNee

        Client2 mClnt;
        NetPhase mState;

		private void InitModels()
		{
            mState = NetPhase.Dating;
            mClnt = new Client2(ClntBufHndl, ClntBufPrep, false);
            mCbMsg = new UICbMsg();
            bRunning = true;

            mExaminee = new ExamineeC();

            QuestSheetModel = new QuestSheet();
            
            bBtnBusy = false;
			
			InitRemainingTime();
		}
		
		private void InitRemainingTime()
		{
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
                        RemainingTime = mExaminee.kDtDuration = new TimeSpan(0, m, s);
                }
            }
            if (m < 0 || s < 0)
                RemainingTime = mExaminee.kDtDuration;
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
    }
}
