using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using sQzLib;
using System.Net.NetworkInformation;

namespace sQzClient
{
    public class NonnullRichTextView
    {
        public static UIElement Render(NonnullRichText RichText)
        {
            if (RichText.HasImage())
                return RenderWithImage(RichText);
            else
                return RenderNoImage(RichText);
        }

        static UIElement RenderWithImage(NonnullRichText RichText)
        {
            StackPanel lines = new StackPanel();
            StackPanel line = new StackPanel();
            line.Orientation = Orientation.Horizontal;
            foreach (object run in RichText.Runs)
            {
                if (run is TextLineBreak)
                {
                    lines.Children.Add(line);
                    line = new StackPanel();
                    line.Orientation = Orientation.Horizontal;
                }
                else
                    AddTextOrImageToLine(run, line);
            }
            if (line.Children.Count > 0)
                lines.Children.Add(line);
            return CompactStack(lines);
        }

        static UIElement CompactStack(UIElement source)
        {
            StackPanel IsStack = source as StackPanel;
            if (IsStack == null)
                return source;
            else if (IsStack.Children.Count == 1)
                return CompactStackHaving1Child(IsStack);
            else
                return CompactStackHavingChildren(IsStack);
        }

        static UIElement CompactStackHaving1Child(StackPanel source)
        {
            UIElement onlyChild = source.Children[0];
            source.Children.Clear();
            return CompactStack(onlyChild);
        }

        static StackPanel CompactStackHavingChildren(StackPanel source)
        {
            StackPanel target = new StackPanel();
            target.Orientation = source.Orientation;
            UIElement[] children = GetChildrenFromStackPanel(source);
            foreach (UIElement child in children)
                target.Children.Add(CompactStack(child));
            return target;
        }

        static UIElement[] GetChildrenFromStackPanel(StackPanel source)
        {
            UIElement[] children = new UIElement[source.Children.Count];
            source.Children.CopyTo(children, 0);
            source.Children.Clear();
            return children;
        }

        static void AddTextOrImageToLine(object run, StackPanel line)
        {
            string s = run as string;
            if (s == null)
                AddImageToLine(run as byte[], line);
            else
            {
                TextBlock t = new TextBlock();
                t.Text = s;
                line.Children.Add(t);
            }
        }

        static void AddImageToLine(byte[] imageInBytes, StackPanel line)
        {
            BitmapImage bi = new BitmapImage();
            var stream = new MemoryStream(imageInBytes);
            bi.BeginInit();
            bi.StreamSource = stream;
            bi.EndInit();
            Image img = new Image();
            img.Source = bi;
            img.Width = bi.Width;
            img.Height = bi.Height;
            line.Children.Add(img);
        }

        static UIElement RenderNoImage(NonnullRichText RichText)
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
