using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using sQzLib;

namespace sQzClient
{
    public class NonnullRichTextView
    {
        public static UIElement Render(NonnullRichText RichText)
        {
            if (RichText.HasImage())
                return ToViewWithImage(RichText);
            else
                return ToViewNoImage(RichText);
        }

        static UIElement ToViewWithImage(NonnullRichText RichText)
        {
            StackPanel content = new StackPanel();
            foreach (object run in RichText.Runs)
            {
                string s = run as string;
                if (s == null)
                {
                    BitmapImage bi = new BitmapImage();
                    var stream = new MemoryStream(run as byte[]);
                    bi.BeginInit();
                    bi.StreamSource = stream;
                    bi.EndInit();
                    Image img = new Image();
                    img.Source = bi;
                    img.Width = bi.Width;
                    img.Height = bi.Height;
                    content.Children.Add(img);
                }
                else
                {
                    TextBlock t = new TextBlock();
                    t.Text = s;
                    content.Children.Add(t);
                }
            }
            return content;
        }

        static UIElement ToViewNoImage(NonnullRichText RichText)
        {
            StringBuilder text = new StringBuilder();
            foreach (object run in RichText.Runs)
            {
                string s = run as string;
                if (s != null)
                    text.Append(s);
            }
            TextBlock t = new TextBlock();
            t.Text = text.ToString();
            return t;
        }
    }
}
