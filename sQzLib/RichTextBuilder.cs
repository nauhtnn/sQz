using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class RichTextBuilder
    {
        public List<object> Runs;

        public RichTextBuilder()
        {
            Runs = new List<object>();
        }

        public void AddRun(object run)
        {
            if (run is string || run is byte[])
            {
                Runs.Add(run);
            }
            else
                throw new ArgumentException();
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

        public string FirstOrDefault(int i)
        {
            if(Runs.Count > 0 && Runs[0] is string)
                return Runs[0] as string;
            return null;
        }

        public void Replace(string oldValue, string newValue)
        {
            foreach(object i in Runs)
            {
                string s = i as string;
                if(s != null)
                    s.Replace(oldValue, newValue);
            }
        }
    }
}
