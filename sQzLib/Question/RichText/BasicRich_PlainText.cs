using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WpfApp1
{
    public class BasicRich_PlainText: ICloneable
    {
        public RichTextBox RichText { get; private set; }
        public string PlainText { get; private set; }
        public int Length { get; }

        public BasicRich_PlainText(RichTextBox richTextBox)
        {
            RichText = richTextBox;
            PlainText = null;
            Length = GetTextOfRichText().Length;
        }

        private string GetTextOfRichText()
        {
            StringBuilder text = new StringBuilder();
            foreach (Paragraph p in RichText.Document.Blocks.OfType<Paragraph>())
            {
                foreach (Run run in p.Inlines.OfType<Run>())
                    text.Append(run.Text);
                text.Append("\n");
            }
            return text.ToString();
        }

        public BasicRich_PlainText(string plainText)
        {
            RichText = null;
            PlainText = plainText;
            Length = PlainText.Length;
        }

        public char ElementAt(int index)
        {
            if(PlainText != null)
                return PlainText.ElementAt(index);
            else
            {
                return GetTextOfRichText().ElementAt(index);
            }
        }

        public BasicRich_PlainText Substring(int startIndex)
        {
            if (PlainText != null)
                return new BasicRich_PlainText(PlainText.Substring(startIndex));
            else
                return SubstringOfRichText(startIndex);
        }

        private BasicRich_PlainText SubstringOfRichText(int startIndex)
        {
            return SubstringOfRichText(startIndex, Length - startIndex);
        }

        private BasicRich_PlainText SubstringOfRichText(int startIndex, int length)
        {
            BasicRich_PlainText substring = this.Clone() as BasicRich_PlainText;
            foreach (Paragraph para in substring.RichText.Document.Blocks.OfType<Paragraph>())
            {
                foreach (Run run in para.Inlines.OfType<Run>())
                {
                    if (run.Text.Length <= startIndex)
                    {
                        startIndex -= run.Text.Length;
                        para.Inlines.Remove(run);
                    }
                    if (para.Inlines.OfType<Run>().Count() == 0)
                        substring.RichText.Document.Blocks.Remove(para);
                }
            }
            return null;
        }

        public BasicRich_PlainText Substring(int startIndex, int length)
        {
            if (PlainText != null)
                return Substring(startIndex, length);
            else
                return SubstringOfRichText(startIndex, length);
        }

        public void AppendNewParagraphs(BasicRich_PlainText text)
        {
            if (PlainText != null && text.PlainText != null)
                    PlainText = PlainText + "\n" + text.PlainText;
            else
            {
                if(PlainText != null)
                    ConvertToRichText();
                if (text.PlainText != null)
                    text.ConvertToRichText();
                AppendNewParagraphsFromRichText(text);
            }
        }

        private void ConvertToRichText()
        {
            Run run = new Run();
            run.Text = PlainText;
            Paragraph para = new Paragraph();
            para.Inlines.Add(run);
            RichText = new RichTextBox();
            RichText.Document.Blocks.Add(para);
            PlainText = null;
        }

        private void AppendNewParagraphsFromRichText(BasicRich_PlainText text)
        {
            foreach (Paragraph p in text.RichText.Document.Blocks.OfType<Paragraph>())
                RichText.Document.Blocks.Add(p);
        }

        public object Clone()
        {
            if (PlainText != null)
                return new BasicRich_PlainText(PlainText);
            else
                return new BasicRich_PlainText(RichText);
        }
    }
}
