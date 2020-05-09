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

    public class OptionView : StackPanel
    {
        static CornerRadius LabelCornerRadius;
        //static double LabelWidth;
        Border LabelBorder;

        //public Label AnsCellLabel;TODO: MVC remove
        public ListBoxItem mLbxItem;

        public static void InitLabelWidth()//double labelWidth)
        {
            //if(LabelCornerRadius.Equals(default(CornerRadius)))
            //{
                LabelCornerRadius = new CornerRadius();
                LabelCornerRadius.BottomLeft = LabelCornerRadius.BottomRight = LabelCornerRadius.TopLeft = LabelCornerRadius.TopRight = 50;
                //LabelWidth = labelWidth;
            //}
        }

        //TODO: MVC remove
        public ListBoxItem Render(NonnullRichText richText, int idx, double questionWidth)
        {
            questionWidth -= 10;//alignment

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            LabelBorder = new Border();
            LabelBorder.Width = LabelBorder.Height = 30;
            LabelBorder.CornerRadius = LabelCornerRadius;
            LabelBorder.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            TextBlock tb = new TextBlock();
            tb.Text = "" + (char)('A' + idx);
            tb.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            LabelBorder.Child = tb;
            sp.Children.Add(LabelBorder);
            //TextBlock ansTxt = new TextBlock();
            //ansTxt.Text = text;
            //ansTxt.TextWrapping = TextWrapping.Wrap;
            //ansTxt.Width = questionWidth - LabelBorder.Width;
            //ansTxt.VerticalAlignment = VerticalAlignment.Center;
            //sp.Children.Add(ansTxt);
            sp.Children.Add(NonnullRichTextView.Render(richText));

            mLbxItem = new ListBoxItem();
            mLbxItem.Content = sp;
            mLbxItem.Padding = new Thickness(0);
            mLbxItem.Name = "_" + idx.ToString();

            //AnsCellLabel = new Label();TODO: MVC remove

            return mLbxItem;
        }

        //TODO: MVC remove
        //public Label ansCellLable //mLbl is public after all
        //{
        //    get { return AnsCellLabel; }
        //}

        //TODO: MVC remove
        //public ListBoxItem lbxi
        //{
        //    get { return mLbxItem; }
        //}

        public void Selected()
        {
            LabelBorder.Background = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            TextBlock t = (TextBlock)LabelBorder.Child;
            t.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_Color];
            //AnsCellLabel.Content = 'X';TODO: MVC remove
        }

        public void Unselected()
        {
            LabelBorder.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            TextBlock t = (TextBlock)LabelBorder.Child;
            t.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            //AnsCellLabel.Content = string.Empty;TODO: MVC remove
        }
    }
}
