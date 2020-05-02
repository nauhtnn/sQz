using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class NonnullRichTextBuilder
    {
        public List<object> Runs;

        public static Queue<NonnullRichTextBuilder> NewWith(string[] rawTexts)
        {
            Queue<NonnullRichTextBuilder> richTexts = new Queue<NonnullRichTextBuilder>();
            foreach(string rawText in rawTexts)
                richTexts.Enqueue(new NonnullRichTextBuilder(Utils.CleanSpace(rawText)));
            return richTexts;
        }

        public NonnullRichTextBuilder(List<object> runs)
        {
            if (runs == null)
                throw new ArgumentException();
            Runs = new List<object>();
            foreach (object run in runs)
                AddRun(run);
            if (Runs.Count == 0)
                throw new ArgumentException();
        }

        public NonnullRichTextBuilder(string rawText)
        {
            if(rawText == null)
                throw new ArgumentException();
            string tidyText = Utils.CleanSpace(rawText);
            if (tidyText.Length > 0)
            {
                Runs = new List<object>();
                Runs.Add(tidyText);
            }
            else
                throw new ArgumentException();
        }

        void AddRun(object run)
        {
            if (run == null)
                throw new ArgumentException();
            if (run is byte[])
                Runs.Add(run);
            else
            {
                string s = run as string;
                if (s == null)
                    throw new ArgumentException();
                else
                {
                    string tidyText = Utils.CleanSpace(s);
                    if (tidyText.Length == 0)
                        throw new ArgumentException();
                    Runs.Add(tidyText);
                }
            }   
        }

        public KeyValuePair<string, List<KeyValuePair<int, byte[]>>> SeparateTextFromImages()
        {
            StringBuilder rawText = new StringBuilder();
            List<KeyValuePair<int, byte[]>> imagesAtPositions = new List<KeyValuePair<int, byte[]>>();

            foreach(object run in Runs)
            {
                string s = run as string;
                if (s != null)
                    rawText.Append(s);
                else if(run is byte[])
                {
                    int position = rawText.ToString().Length;
                    imagesAtPositions.Add(new KeyValuePair<int, byte[]>(position, run as byte[]));
                }
                else
                    throw new ArgumentException();
            }
            return new KeyValuePair<string, List<KeyValuePair<int, byte[]>>>(rawText.ToString(), imagesAtPositions);
        }

        public string FirstStringOrDefault()
        {
            return Runs[0] as string;
        }

        public void Trunc1AtLeft()
        {
            if (Runs.Count > 0)
            {
                string text = Runs[0] as string;
                if(text != null && text.Length > 1)
                {
                    string newText = Utils.CleanSpace(text.Substring(1));
                    Runs.RemoveAt(0);
                    Runs.Insert(0, newText);
                }
            }
        }

        public void Replace(string oldValue, string newValue)
        {
            int n = Runs.Count;
            for(int i = 0; i < n; ++i)
            {
                string s = Runs[i] as string;
                if(s != null && s.IndexOf(oldValue) > -1)
                {
                    Runs.Insert(i++, Utils.CleanSpace(s.Replace(oldValue, newValue)));
                    Runs.RemoveAt(i--);
                }
            }
        }
    }
}
