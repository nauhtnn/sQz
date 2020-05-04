using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using sQzLib;

namespace sQzClient
{
    class QuestSheetView
    {
        public void SetSize(double backgroundSize, double margin)
        {
            MultiChoiceItemView.IdxHeight = 3 * margin;
            MultiChoiceItemView.QuestionWidth = backgroundSize - margin - margin;
        }

        public void RenderModelToView(QuestSheet model, StackPanel view)
        {
            int questIdx = 1;
            foreach (MultiChoiceItem question in model.Questions)
                MultiChoiceItemView.RenderModelToViewer(question, questIdx++, view);
        }
    }
}
