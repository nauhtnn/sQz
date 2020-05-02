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
        public static CornerRadius sCr;
        static double sW;
        Border mB;

        public Label mLbl;
        public ListBoxItem mLbxItem;

        public static void SInit(double w)
        {
            sCr = new CornerRadius();
            sCr.BottomLeft = sCr.BottomRight = sCr.TopLeft = sCr.TopRight = 50;
            sW = w;
        }

        public AnswerSheetCellView(string txt, int idx, double w)
        {
            w -= 10;//alignment
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            mB = new Border();
            mB.Width = mB.Height = 30;
            mB.CornerRadius = sCr;
            mB.Background = Theme.s._[(int)BrushId.Q_BG];
            TextBlock tb = new TextBlock();
            tb.Text = "" + (char)('A' + idx);
            tb.Foreground = Theme.s._[(int)BrushId.QID_BG];
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            mB.Child = tb;
            sp.Children.Add(mB);
            TextBlock ansTxt = new TextBlock();
            ansTxt.Text = txt;
            ansTxt.TextWrapping = TextWrapping.Wrap;
            ansTxt.Width = w - mB.Width;
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
            mB.Background = Theme.s._[(int)BrushId.QID_BG];
            TextBlock t = (TextBlock)mB.Child;
            t.Foreground = Theme.s._[(int)BrushId.QID_Color];
            mLbl.Content = 'X';
        }

        public void Unselected()
        {
            mB.Background = Theme.s._[(int)BrushId.Q_BG];
            TextBlock t = (TextBlock)mB.Child;
            t.Foreground = Theme.s._[(int)BrushId.QID_BG];
            mLbl.Content = string.Empty;
        }
    }
}
