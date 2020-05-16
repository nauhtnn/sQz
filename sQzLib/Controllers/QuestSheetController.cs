using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Collections.Generic;

namespace sQzLib
{
    public class QuestSheetController
    {
        public QuestSheetController NewWidth(QuestSheetView view)
        {
            QuestSheetController controller = new QuestSheetController();
            foreach (ListBox options in view.OptionsGroupedByQuestion)
                options.SelectionChanged += Options_SelectionChanged;
            return controller;
        }

        void Options_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
