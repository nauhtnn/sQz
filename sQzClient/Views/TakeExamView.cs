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
        AnswerSheetView AnswerSheetViewer;

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

            //AnswerSheetCellView.SInit(Window.GetWindow(this).FontSize);
            //mExaminee.mAnsSheet.Init(mQuestSheet.LvId);
            //mExaminee.mAnsSheet.InitView(mQuestSheet, qaWh, null);
            //mExaminee.mAnsSheet.bChanged = false;

            AnswerSheetViewer = new AnswerSheetView();
            AnswerSheetViewer.FirstRenderTableToView(QuestSheetModel.Questions.Count, AnswerCells);
        }

        void SetQuestSheetView()
        {
            QuestionsView.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            MultiChoiceItemView.Controller = this;
            //double mrg = FontSize / 2;
            //qiWh = 3 * mrg;
            //qMrg = new Thickness(mrg, mrg, 0, mrg);
            //qaWh = (QuestSheetBG.Width - SystemParameters.ScrollWidth) / 2 - mrg - mrg - qiWh;
            QuestSheetViewer = new QuestSheetView();

            QuestSheetViewer.SetSize(QuestSheetBG.Width - SystemParameters.ScrollWidth, FontSize / 2);

            QuestSheetViewer.RenderModelToView(QuestSheetModel, QuestionsView);
        }
    }
}
