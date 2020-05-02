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

    public class AnswerSheetCellView : StackPanel
    {
        public static CornerRadius mCornerRadius;
        static double Width;
        Border mBorder;

        public Label mLbl;
        public ListBoxItem mLbxItem;

        public static void SInit(double w)
        {
            mCornerRadius = new CornerRadius();
            mCornerRadius.BottomLeft = mCornerRadius.BottomRight = mCornerRadius.TopLeft = mCornerRadius.TopRight = 50;
            Width = w;
        }

        public AnswerSheetCellView(string txt, int idx, double w)
        {
            w -= 10;//alignment
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            mBorder = new Border();
            mBorder.Width = mBorder.Height = 30;
            mBorder.CornerRadius = mCornerRadius;
            mBorder.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            TextBlock tb = new TextBlock();
            tb.Text = "" + (char)('A' + idx);
            tb.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            mBorder.Child = tb;
            sp.Children.Add(mBorder);
            TextBlock ansTxt = new TextBlock();
            ansTxt.Text = txt;
            ansTxt.TextWrapping = TextWrapping.Wrap;
            ansTxt.Width = w - mBorder.Width;
            ansTxt.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(ansTxt);

            mLbxItem = new ListBoxItem();
            mLbxItem.Content = sp;
            mLbxItem.Padding = new Thickness(0);
            mLbxItem.Name = "_" + idx.ToString();

            mLbl = new Label();
        }

        public Label lbl //mLbl is public after all
        {
            get { return mLbl; }
        }

        public ListBoxItem lbxi
        {
            get { return mLbxItem; }
        }

        public void Selected()
        {
            mBorder.Background = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            TextBlock t = (TextBlock)mBorder.Child;
            t.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_Color];
            mLbl.Content = 'X';
        }

        public void Unselected()
        {
            mBorder.Background = Theme.Singleton.DefinedColors[(int)BrushId.Q_BG];
            TextBlock t = (TextBlock)mBorder.Child;
            t.Foreground = Theme.Singleton.DefinedColors[(int)BrushId.QID_BG];
            mLbl.Content = string.Empty;
        }
    }
}
