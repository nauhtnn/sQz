using System.Windows;
using System.Windows.Controls;

namespace sQzLib
{
    partial class OptionView : ListBoxItem
    {
        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            LabelBorder.Background = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            TextBlock t = (TextBlock)LabelBorder.Child;
            t.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_Color];
            //AnsCellLabel.Content = 'X';TODO: MVC remove
            //ListBox options = Parent as ListBox;
            //if(options != null)
            //{
            //    foreach
            //}
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            LabelBorder.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            TextBlock t = (TextBlock)LabelBorder.Child;
            t.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            //AnsCellLabel.Content = string.Empty;TODO: MVC remove
        }
    }
}
