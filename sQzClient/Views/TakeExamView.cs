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
        //views

        QuestSheetView QuestSheetViewer;
        AnswerGridView AnswerSheetViewer;
		
		private void InitViews()
		{
			SetWindowFullScreen();

            HackingRenderingTest();

            SetAnswerSheetView();
            SetQuestSheetView();
			
			LoadTxt();
			
			txtWelcome.Text = mExaminee.ToString();
            
            txtRTime.Text = "" + RemainingTime.Minutes + " : " + RemainingTime.Seconds;
            BackupInterval = new TimeSpan(0, 0, 30);

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
		}

        private void LoadTxt()
        {
            AnswerTitle.Text = Txt.s._[(int)TxI.ANS_SHEET];
            btnSubmit.Content = Txt.s._[(int)TxI.SUBMIT];
            btnExit.Content = Txt.s._[(int)TxI.EXIT];
        }

        private void SetWindowFullScreen()
        {
            Window w = Window.GetWindow(this);
            w.WindowStyle = WindowStyle.None;
            w.WindowState = WindowState.Maximized;
            w.ResizeMode = ResizeMode.NoResize;
            w.Closing += W_Closing;
            w.FontSize = 16;

            PopupMgr.SetParentWindow(w);
            PopupMgr.Singleton.CbNOK = WPCancel;
        }

        void HackingRenderingTest()
        {
            QuestSheetModel.ParseDocx(".\\quiz.docx");

            QuestSheetBG.Visibility = Visibility.Visible;
        }

        void SetAnswerSheetView()
        {
            AnswerSheetBG.Background = Theme.Singleton.DefinedColors[(int)BrushId.AnswerSheet_BG];
            
            AnswerCells.Background = Theme.Singleton.DefinedColors[(int)BrushId.Sheet_BG];

            //mExaminee.mAnsSheet.Init(mQuestSheet.LvId);
            //mExaminee.mAnsSheet.InitView(mQuestSheet, qaWh, null);
            //mExaminee.mAnsSheet.bChanged = false;

            AnswerSheetViewer = AnswerGridView.NewWith(AnswerCells);
            AnswerSheetViewer.FirstRenderTableToView(QuestSheetModel.Questions.Count, AnswerCells);
        }

        void SetQuestSheetView()
        {
            QuestionsView.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            QuestSheetViewer = QuestSheetView.NewWith(QuestSheetModel, QuestSheetBG.Width - SystemParameters.ScrollWidth, FontSize / 2, QuestionsView);

            QuestSheetViewer.View();
        }
    }
}
