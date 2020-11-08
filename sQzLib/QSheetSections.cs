using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class QSheetSections
    {
        public const string SECTION_HEADER = "<SECTION_HEADER>";

        Queue<int> StartIndices = new Queue<int>();
        Queue<string> Headers = new Queue<string>();

        public void AddSection(int startIdx, string header)
        {
            StartIndices.Enqueue(startIdx);
            Headers.Enqueue(header);
        }

        public void Validate()
        {
            int max = -1;
            foreach(int i in StartIndices)
            {
                if(max >= i)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Question sheet section error!\nThe indicies:");
                    foreach (int j in StartIndices)
                        sb.Append(" " + j);
                    System.Windows.MessageBox.Show(sb.ToString());
                    return;
                }
            }
        }
    }
}
