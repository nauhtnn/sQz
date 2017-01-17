using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace sQzCS
{
    class Program
    {
        public static int MAX_COLUMN = 2;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);

        static void Main(string[] args)
        {
            if (!AttachConsole(-1))
            { // Attach to an parent process console
                AllocConsole(); // Alloc a new console
            }

            System.Console.WriteLine("sQz version 0.0.2");
            int fi = 1;
            string fn = "qz" + fi + ".txt";
            string buf = Utils.ReadFile(fn);
            while (buf != null)
            {
                //buf = Utils.cleanWhSp(buf);
                string[] vToken = Utils.Split(buf, '\n');
                Page pg = new Page();
                Settings st = pg.mSt;

                List<Question> vQuest = new List<Question>();
                Question q = new Question();
                int i = 0, e = vToken.Length;
                while (i < e)
                {
                    q.read(vToken, ref i, st);
                    vQuest.Add(q);
                    q = new Question();
                }
                fn = "qz" + fi + ".html";
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fn);
                if (sw == null)
                {
                    System.Console.WriteLine("Cannot write file " + fn);
                    return;
                }
                else
                    System.Console.WriteLine("Write file " + fn);
                e = vQuest.Count;
                pg.WriteHeader(sw);
                pg.WriteFormHeader(sw, e);
                int j = 0, column = MAX_COLUMN;
                Random r = new Random();
                while (0 < vQuest.Count)
                {
                    if (column == MAX_COLUMN)
                    {
                        sw.Write("<div class='cl1'></div>");
                        column = 0;
                    }
                    i = 0;
                    if (pg.mSt.bQuestSort)
                        i = r.Next(vQuest.Count - 1);
                    vQuest[i].write(sw, ++j, ref column);
                    vQuest.RemoveAt(i);
                }
                pg.WriteFormFooter(sw);
                pg.WriteFooter(sw);
                sw.Close();

                fn = "qz" + (++fi) + ".txt";
                buf = Utils.ReadFile(fn);
            }
        }
    }
}
