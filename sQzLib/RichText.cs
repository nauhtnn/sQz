using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    class RichText
    {
        List<object> Runs;

        public bool IsTextOnly()
        {
            return (Runs.Count == 1) && (Runs[0].GetType() == typeof(string));
        }
    }
}
