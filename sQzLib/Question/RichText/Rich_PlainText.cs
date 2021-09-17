using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.IO;
using System.Windows;

namespace sQzLib
{
    public enum TEXT_FORMAT
    {
        None,
        Bold,
        Italic,
        Underline,
        Image
    }
    public class RunData
    {
        public string Text;
        public TEXT_FORMAT Format;
        public byte[] ImageData;

        public RunData(string text)
        {
            Text = text;
            Format = TEXT_FORMAT.None;
            ImageData = null;
        }
    }

    public class ParagraphData
    {
        public Queue<RunData> Runs;

        public ParagraphData()
        {
            Runs = new Queue<RunData>();
        }

        public ParagraphData(string text)
        {
            Runs = new Queue<RunData>();
            Runs.Enqueue(new RunData(text));
        }
    }

    public class Rich_PlainText: IText, ICloneable
    {
        public string PlainText;
        public Queue<ParagraphData> Paragraphs;
        
        public int Length { get; }

        public Rich_PlainText(Queue<ParagraphData> paragraphs)
        {
            Paragraphs = paragraphs;
            PlainText = null;
            Length = GetInnerTextOfRichText().Length;
        }

        public int IndexOf(string value)
        {
            if (PlainText != null)
                return PlainText.IndexOf(value);
            else
                return GetInnerTextOfRichText().IndexOf(value);
        }

        public string GetInnerText()
        {
            if (PlainText != null)
                return PlainText;
            else
                return GetInnerTextOfRichText();
        }

        public string GetInnerTextOfRichText()
        {
            StringBuilder text = new StringBuilder();
            foreach (ParagraphData p in Paragraphs)
            {
                foreach (RunData run in p.Runs)
                    text.Append(run.Text);
                text.Append("\n");
            }
            return text.ToString();
        }

        public bool StartsWith(string text)
        {
            if (PlainText != null)
                return PlainText.StartsWith(text);
            else
                return GetInnerTextOfRichText().StartsWith(text);
        }

        public Rich_PlainText(string plainText)
        {
            Paragraphs = null;
            PlainText = plainText;
            Length = PlainText.Length;
        }

        public char ElementAt(int index)
        {
            if(PlainText != null)
                return PlainText.ElementAt(index);
            else
            {
                return GetInnerTextOfRichText().ElementAt(index);
            }
        }

        public char Last()
        {
            if (PlainText != null)
                return PlainText.Last();
            else
            {
                char lastChar = (char)0;
                foreach (ParagraphData p in Paragraphs)
                {
                    foreach (RunData run in p.Runs)
                        if(run.Text.Length > 0)
                            lastChar = run.Text.Last();
                }
                return lastChar;
            }
        }

        public Rich_PlainText Substring(int startIndex)
        {
            if (PlainText != null)
                return new Rich_PlainText(PlainText.Substring(startIndex));
            else
                return SubstringOfRichText(startIndex);
        }

        private Rich_PlainText SubstringOfRichText(int startIndex)
        {
            return SubstringOfRichText(startIndex, Length - startIndex);
        }

        private Rich_PlainText SubstringOfRichText(int startIndex, int length)
        {
            Rich_PlainText substring = this.Clone() as Rich_PlainText;
            foreach (ParagraphData para in substring.Paragraphs)
            {
                foreach (RunData run in para.Runs)
                {
                    if (run.Text.Length <= startIndex)
                    {
                        startIndex -= run.Text.Length;
                        para.Runs.Dequeue();
                    }
                    if (para.Runs.Count() == 0)
                        substring.Paragraphs.Dequeue();
                }
            }
            return null;
        }

        public Rich_PlainText Substring(int startIndex, int length)
        {
            if (PlainText != null)
                return new Rich_PlainText(PlainText.Substring(startIndex, length));
            else
                return SubstringOfRichText(startIndex, length);
        }

        public void AppendNewParagraphs(Rich_PlainText text)
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
            Paragraphs = new Queue<ParagraphData>();
            Paragraphs.Enqueue(new ParagraphData(PlainText));
            PlainText = null;
        }

        private void AppendNewParagraphsFromRichText(Rich_PlainText text)
        {
            Paragraphs.Concat(text.Paragraphs);
        }

        public object Clone()
        {
            if (PlainText != null)
                return new Rich_PlainText(PlainText);
            else
                return new Rich_PlainText(Paragraphs);
        }
    }
}
