using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public partial class NonnullRichText
    {
        // no null or empty element in this list
        public IReadOnlyList<object> Runs { get; private set; }

        public NonnullRichText(NonnullRichTextBuilder richText)
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
