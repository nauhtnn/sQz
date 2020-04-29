using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class RichText
    {
        string RawText;
        List<KeyValuePair<int, byte[]>> ImagesAtPositions;

        public RichText(string rawText, List<KeyValuePair<int, byte[]>> imagesAtPositions)
        {
            RawText = rawText;
            ImagesAtPositions = imagesAtPositions;
        }

        public bool HasImage()
        {
            return ImagesAtPositions.Count > 0;
        }

        public List<object> GetRuns()
        {
            List<object> runs = new List<object>();
            int textPos1 = 0;
            int textPos2 = 0;
            foreach(KeyValuePair<int, byte[]> image in ImagesAtPositions)
            {
                textPos2 = image.Key;
                if(textPos1 < textPos2)
                    runs.Add(RawText.Substring(textPos1, textPos2));
                runs.Add(image.Value);
                textPos1 = textPos2;
            }
            if (textPos2 < RawText.Length)
                runs.Add(RawText.Substring(textPos1));
            return runs;
        }
    }
}
