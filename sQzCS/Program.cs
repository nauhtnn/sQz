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

        static void PrintQuestionList(string filePath, List<Question> questions)
        {
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            List<string> lines = new List<string>();
            foreach (Question question in questions)
                lines.AddRange(question.ToListOfString());
            System.IO.File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }

        static void LogError(string errorFile, List<Question> questions)
        {
            Console.WriteLine("Error in " + errorFile);
            string errorLog = "qz_error.txt";
            Console.WriteLine("Check current questions in " + errorLog);
            PrintQuestionList(errorLog, questions);
            Console.Read();
        }

        public static void Main(string[] args)
        {
            if (!AttachConsole(-1))
            { // Attach to an parent process console
                AllocConsole(); // Alloc a new console
            }

            System.Console.WriteLine("sQz_CS 1.1.0.0");
            int fi = 1;
            string fn = "qz" + fi + ".txt";
            string buf = Utils.ReadFile(fn);
            while (buf != null)
            {
                //buf = Utils.cleanWhSp(buf);
                Page pg = new Page();

                Question.StartRead(Utils.Split(buf, '\n'), pg.mSt);
                List<Question> vQuest = new List<Question>();
                Question q = new Question();
                bool hasQuestion;
                try
                {
                    hasQuestion = q.Read();
                }
                catch(ArgumentException)
                {
                    LogError(fn, vQuest);
                    return;
                }
                while (hasQuestion)
                {
                    vQuest.Add(q);
                    q = new Question();
                    try
                    {
                        hasQuestion = q.Read();
                    }
                    catch (ArgumentException)
                    {
                        LogError(fn, vQuest);
                        return;
                    }
                }
                q = null;
                fn = "qz" + fi + ".html";
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fn);
                if (sw == null)
                {
                    System.Console.WriteLine("Cannot write file " + fn);
                    return;
                }
                else
                    System.Console.WriteLine("Write file " + fn);
                int e = vQuest.Count;
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
                    int i = 0;
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
