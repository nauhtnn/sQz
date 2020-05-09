using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Collections.Generic;

namespace sQzLib
{
    public interface MultiChoiceItemController
    {
        void Options_SelectionChanged(object sender, SelectionChangedEventArgs e);
    }
}
