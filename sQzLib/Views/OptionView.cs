using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace sQzLib
{
    public delegate void DgEvntCB();

    partial class OptionView : ListBoxItem
    {
        static CornerRadius LabelCornerRadius;
        Border LabelBorder;

        //public Label AnsCellLabel;TODO: MVC remove

        public static void InitLabelCircle()
        {
            //if(LabelCornerRadius.Equals(default(CornerRadius)))
            //{
                LabelCornerRadius = new CornerRadius();
                LabelCornerRadius.BottomLeft = LabelCornerRadius.BottomRight = LabelCornerRadius.TopLeft = LabelCornerRadius.TopRight = 50;
            //}
        }

        //TODO: MVC remove
        public static OptionView NewWith(NonnullRichText richText, int idx, double questionWidth)
        {
            OptionView option = new OptionView();

            questionWidth -= 10;//alignment

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            option.LabelBorder = new Border();
            option.LabelBorder.Width = 30;
            option.LabelBorder.Height = 30;
            option.LabelBorder.CornerRadius = LabelCornerRadius;
            option.LabelBorder.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            TextBlock tb = new TextBlock();
            tb.Text = "" + (char)('A' + idx);
            tb.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            option.LabelBorder.Child = tb;
            sp.Children.Add(option.LabelBorder);
            //TextBlock ansTxt = new TextBlock();
            //ansTxt.Text = text;
            //ansTxt.TextWrapping = TextWrapping.Wrap;
            //ansTxt.Width = questionWidth - LabelBorder.Width;
            //ansTxt.VerticalAlignment = VerticalAlignment.Center;
            //sp.Children.Add(ansTxt);
            sp.Children.Add(NonnullRichTextView.Render(richText));

            option.Content = sp;
            option.Padding = new Thickness(0);
            option.Name = "_" + idx.ToString();

            //AnsCellLabel = new Label();TODO: MVC remove

            return option;
        }

        //TODO: MVC remove
        //public Label ansCellLable //mLbl is public after all
        //{
        //    get { return AnsCellLabel; }
        //}

        //TODO: MVC remove
        //public ListBoxItem lbxi
        //{
        //    get { return SelectableTextAndIdx; }
        //}

        
    }
}
