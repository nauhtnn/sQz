using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public interface IText
    {
        bool StartsWith(string text);
        string GetInnerText();

        char Last();
    }
}
