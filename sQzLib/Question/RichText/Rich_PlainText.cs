using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace RichTextPractice
{
    public class Rich_PlainText
    {
        public RichTextBox mRichTextBox { get; private set; }
        public string PlainText { get; private set; }
        public int Length { get; }

        public Rich_PlainText(RichTextBox richTextBox)
        {
            mRichTextBox = richTextBox;
            PlainText = null;
            Length = GetRichText().Length;
        }

        private string GetRichText()
        {
            return mRichTextBox.Document.Blocks.OfType<Paragraph>().First().Inlines.OfType<Run>().First().Text;
        }

        public Rich_PlainText(string plainText)
        {
            mRichTextBox = null;
            PlainText = plainText;
            Length = PlainText.Length;
        }

        public char ElementAt(int index)
        {
            if(PlainText != null)
                return PlainText.ElementAt(index);
            else
            {
                return GetRichText().ElementAt(index);
            }
        }

        public string Substring(int startIndex)
        {
            if (PlainText != null)
                return PlainText.Substring(startIndex);
            else
                return GetRichText().Substring(startIndex);
        }

        public string Substring(int startIndex, int length)
        {
            if (PlainText != null)
                return PlainText.Substring(startIndex, length);
            else
                return GetRichText().Substring(startIndex, length);
        }
    }
}
