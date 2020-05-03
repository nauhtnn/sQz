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
            StackPanel wholeViewer = new StackPanel();
            StackPanel lineViewer = new StackPanel();
            lineViewer.Orientation = Orientation.Horizontal;
            Label l = new Label();
            foreach (object run in RichText.Runs)
            {
                if (run is TextLineBreak)
                {
                    wholeViewer.Children.Add(lineViewer);
                    lineViewer = new StackPanel();
                    lineViewer.Orientation = Orientation.Horizontal;
                }
                else
                    AddRunToLineViewer(run, lineViewer);
            }
            if (lineViewer.Children.Count > 0)
                wholeViewer.Children.Add(lineViewer);
            return CompactStack(wholeViewer);
        }

        static StackPanel CompactStack(StackPanel stack)
        {
            StackPanel compactStack = new StackPanel();
            foreach(UIElement i in stack.Children)
            {
                StackPanel childStack = i as StackPanel;
                if(childStack == null)
                    compactStack.Children.Add(childStack);
                else
                {
                    if (childStack.Children.Count == 1)
                    {
                        UIElement element = childStack.Children[0];
                        childStack.Children.Remove(element);
                        compactStack.Children.Add(element);
                    }
                    else if(childStack.Children.Count > 1)
                    {
                        UIElement[] elements = new UIElement[childStack.Children.Count];
                        childStack.Children.CopyTo(elements, 0);
                        childStack.Children.Clear();
                        StackPanel cloneChildStack = new StackPanel();
                        cloneChildStack.Orientation = childStack.Orientation;
                        foreach (UIElement element in elements)
                            cloneChildStack.Children.Add(element);
                        compactStack.Children.Add(cloneChildStack);
                    }
                }
            }
            return compactStack;
        }

        static void AddRunToLineViewer(object run, StackPanel lineViewer)
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
                lineViewer.Children.Add(img);
            }
            else
            {
                TextBlock t = new TextBlock();
                t.Text = s;
                lineViewer.Children.Add(t);
            }
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
