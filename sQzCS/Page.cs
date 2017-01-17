using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace sQzCS
{
    class Page
    {
        public Settings mSt;
    
        public Page()
        {
            mSt = new Settings();
        }
        public void WriteHeader(System.IO.StreamWriter os)
        {
            if (mSt.bDIV)
                WriteHdrDIV(os);
            else
                WriteHdr(os);
        }
        public void WriteHdr(System.IO.StreamWriter os)
        {
            //os.Write("<!DOCTYPE html><html><head><meta charset='utf-8'/><script src='sQz.js'>"\

            //    "</script></head><body>\n")
        }
        public void WriteHdrDIV(System.IO.StreamWriter os)
        {
            os.WriteLine("<!DOCTYPE html><html><head><meta charset='utf-8'/><script src='sQz.js'>" +
                "</script><link rel='stylesheet' type='text/css' href='sQz.css'></head>" +
                "<body onload='setCell()'>");
        }
        public void WriteFooter(System.IO.StreamWriter os)
        {
            os.Write("</body></html>");
        }
        public void WriteFormHeader(System.IO.StreamWriter os, int nQuest)
        {
            os.Write("<form><div id='lp'><div class='tit2'>Answer Sheet</div><div id='sht'>" +
                "<table id='ans'><tr><th class='o i'></th><th>A</th><th>B</th><th>C</th>" +
                "<th>D</th></tr>");
            //if 0 < nQuest
            string buf = null;
            string s = "<tr><td>";
            string e = "</td><td></td><td></td><td></td><td></td></tr>\n";
            for (int i = 0; i < nQuest;)
                buf = buf + s + (++i) + e;
            buf += "</table></div><input type='button'class='btn btn1'" +
                "onclick='score()'value='Submit'><input type='button'" +
                "class='btn'onclick='showAnswer()'value='Ans Keys'>" +
                "</div><div class='bp'></div>";
            os.Write(buf);
        }
        public void WriteFormFooter(System.IO.StreamWriter os)
        {
            os.Write("</form>");
        }
    }
}
