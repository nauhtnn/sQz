using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using sQzLib;

namespace sQzLib
{
    public class QuestSheetView
    {
		double BackgroundWidth;
		double Padding;
		StackPanel UI_Container;
		QuestSheet Model;
		public List<ListBox> OptionsGroupedByQuestion;

        public static QuestSheetView NewWith(QuestSheet model, double backgroundWidth, double padding, StackPanel UI_container)
        {
            QuestSheetView questSheet = new QuestSheetView();
			questSheet.Model = model;
			questSheet.BackgroundWidth = backgroundWidth;
			questSheet.Padding = padding;
			questSheet.UI_Container = UI_container;
			OptionsGroupedByQuestion = new List<ListBox>();
            return questSheet;
        }

        public void View()
        {
			OptionView.InitLabelCircle();
			
			double questionIdxHeight = 3 * Padding;
			double questionWidth = BackgroundWidth - Padding - Padding;
            int idxInQuestSheet = 1;
            foreach (MultiChoiceItem model in Model.Questions)
			{
				MultiChoiceItemView question = MultiChoiceItemView.NewWith(model, idxInQuestSheet, questionIdxHeight, questionWidth, UI_container);
				question.Render();
				OptionsGroupedByQuestion.Add(question.Options);
			}
        }
    }
}