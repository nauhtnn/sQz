using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    class RichTextBuilder
    {
        public StringBuilder RawText { get; private set; }
        public List<KeyValuePair<int, byte[]>> ImagesAtPositions { get; private set; }

        public RichTextBuilder()
        {
            RawText = new StringBuilder();
            ImagesAtPositions = new List<KeyValuePair<int, byte[]>>();
        }

        public void AddRawText(string rawText)
        {
            RawText.Append(rawText);
        }

        public void AddImage(byte[] imageInBytes)
        {
            int position = RawText.ToString().Length;
            ImagesAtPositions.Add(new KeyValuePair<int, byte[]>(position, imageInBytes));
        }
    }
}
