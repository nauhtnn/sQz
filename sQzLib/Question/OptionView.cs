using System.Windows;
using System.Windows.Controls;

namespace sQzLib
{
    public class OptionView: ListBoxItem //todo: merge with AnsItem
    {
        public static CornerRadius sCr;
        static double sW;
        Border mB;
        public string Idx_Label;

        public static void Init(double w)
        {
            sCr = new CornerRadius();
            sCr.BottomLeft = sCr.BottomRight = sCr.TopLeft = sCr.TopRight = 50;
            sW = w;
        }

        public OptionView(string txt, int idx, double w)
        {
            StackPanel view = new StackPanel();
            view.Orientation = Orientation.Horizontal;
            mB = new Border();
            mB.Width = mB.Height = 30;
            mB.CornerRadius = sCr;
            mB.Background = Theme.s._[(int)BrushId.Q_BG];
            TextBlock tb = new TextBlock();
            Idx_Label = "" + (char)('A' + idx);
            tb.Text = Idx_Label;
            tb.Foreground = Theme.s._[(int)BrushId.QID_BG];
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            mB.Child = tb;
            view.Children.Add(mB);
            TextBlock ansTxt = new TextBlock();
            ansTxt.Text = txt;
            ansTxt.TextWrapping = TextWrapping.Wrap;
            ansTxt.Width = w - mB.Width - 8;//todo: remove hard-code
            ansTxt.VerticalAlignment = VerticalAlignment.Center;
            view.Children.Add(ansTxt);

            Content = view;
            Padding = new Thickness(0);

            Selected += OptionView_Selected;
            Unselected += OptionView_Unselected;
        }

        private void OptionView_Unselected(object sender, RoutedEventArgs e)
        {
            mB.Background = Theme.s._[(int)BrushId.Q_BG];
            TextBlock t = (TextBlock)mB.Child;
            t.Foreground = Theme.s._[(int)BrushId.QID_BG];
        }

        private void OptionView_Selected(object sender, RoutedEventArgs e)
        {
            mB.Background = Theme.s._[(int)BrushId.QID_BG];
            TextBlock t = (TextBlock)mB.Child;
            t.Foreground = Theme.s._[(int)BrushId.QID_Color];
        }
    }
}
