using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class RichText
    {
        public IReadOnlyList<object> Runs { get; private set; }

        public RichText(RichTextBuilder richText)
        {
            Runs = richText.Runs.AsReadOnly();
        }

        public bool HasImage()
        {
            foreach (object i in Runs)
                if (i is byte[])
                    return true;
            return false;
        }
    }
}
