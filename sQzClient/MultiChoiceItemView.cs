using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using sQzLib;

namespace sQzClient
{
    class MultiChoiceItemView
    {
        public static void Render(MultiChoiceItem question, StackPanel listView)
        {
            listView.Children.Add(NonnullRichTextView.Render(question.Stem));
            foreach (NonnullRichText richText in question.Options)
                listView.Children.Add(NonnullRichTextView.Render(richText));
        }
    }
}
