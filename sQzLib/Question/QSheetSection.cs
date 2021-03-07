using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public abstract class QSheetSection
    {
        public static Dictionary<SectionID, List<string>> SectionMagicKeywords;
        public static string SECTION_MAGIC_PREFIX;
        const string SECTION_MAGIC_CFG_FILEPATH = "sectionMagicKeywords.txt";

        public string Requirements;
        public List<Question> Questions;
        abstract public bool Parse(Queue<BasicRich_PlainText> tokens);
        abstract public void DBAppendQryIns(string prefx, ref int idx, int qSheetID, StringBuilder vals);

        public static void LoadSectionMagicKeywords()
        {
            InitDefaultMagicKeywords();
            if (!System.IO.File.Exists(SECTION_MAGIC_CFG_FILEPATH))
                return;

            string[] lines;
            try
            {
                lines = System.IO.File.ReadAllLines(SECTION_MAGIC_CFG_FILEPATH);
            }
            catch (System.IO.IOException e)
            {
                System.Windows.MessageBox.Show("LoadSectionMagicKeywords error\n" + e.ToString());
                return;
            }

            if (lines.Length == 0)
                return;

            SECTION_MAGIC_PREFIX = lines[0].Trim();

            for (int i = 1; i < lines.Length; ++i)
            {
                string[] words = lines[i].Split('\t');
                if (words.Length < 2)
                    continue;
                foreach (string w in words)
                    if (w.Trim().Length == 0)
                        continue;
                if (!Enum.IsDefined(typeof(SectionID), words[0]))
                    continue;
                List<string> keywords = new List<string>();
                for (int j = 1; j < words.Length; ++j)
                    keywords.Add(words[j]);
                SectionMagicKeywords.Add((SectionID)Enum.Parse(typeof(SectionID), words[0]), keywords);
            }
        }

        protected void InitDefaultEmpty()
        {
            Requirements = string.Empty;
            Questions = new List<Question>();
        }

        public static void InitDefaultMagicKeywords()
        {
            SECTION_MAGIC_PREFIX = string.Empty;
            SectionMagicKeywords = new Dictionary<SectionID, List<string>>();
            List<string> magicKeywords = new List<string>();
            magicKeywords.Add("following [text|passage]");
            magicKeywords.Add("blank");
            SectionMagicKeywords.Add(SectionID.PassageWithBlanks, magicKeywords);
            magicKeywords = new List<string>();
            magicKeywords.Add("following [text|passage]");
            SectionMagicKeywords.Add(SectionID.BasicPassage, magicKeywords);
        }

        public static void TrimToFirstSection(Queue<BasicRich_PlainText> tokens)
        {
            if (SECTION_MAGIC_PREFIX.Length == 0)
                return;
            while (tokens.Count > 0)
            {
                if (!tokens.Peek().StartsWith(SECTION_MAGIC_PREFIX))
                    tokens.Dequeue();
                else
                    return;
            }
        }

        public int CountQuestions()
        {
            return Questions.Count;
        }

        protected Question Parse1Question(Queue<BasicRich_PlainText> tokens)
        {
            if (tokens.Count < Question.NUMBER_OF_OPTIONS + 2)//+ stem, answer
            {
                System.Windows.MessageBox.Show("From the end, line " + tokens.Count + " doesn't have 1 stem 4 options 1 answer!");
                return null;
            }

            Question question = new Question();
            question.Stem = tokens.Dequeue().ToString();
            question.vAns = new string[Question.NUMBER_OF_OPTIONS];
            for (int j = 0; j < Question.NUMBER_OF_OPTIONS;)
                question.vAns[j++] = tokens.Dequeue().ToString();
            char key_label = tokens.Dequeue().Last();
            if (key_label < 'A' || 'D' < key_label)
            {
                System.Windows.MessageBox.Show("From the end, line " + tokens.Count + " has key: " + key_label +
                    "\nNeighbor stem: " + question.Stem);
                return null;
            }
            question.vKeys = new bool[Question.NUMBER_OF_OPTIONS];
            for (int j = 0; j < Question.NUMBER_OF_OPTIONS; ++j)
                question.vKeys[j] = false;
            question.vKeys[key_label - 'A'] = true;
            return question;
        }

        protected bool ParseQuestions(Queue<BasicRich_PlainText> tokens)
        {
            Questions = new List<Question>();
            while (tokens.Count > 0)
            {
                if (SECTION_MAGIC_PREFIX.Length > 0 &&
                    //sQzLib.Utils.CleanFront(
                    tokens.Peek().StartsWith(SECTION_MAGIC_PREFIX))
                    break;

                Question question = Parse1Question(tokens);
                if (question == null)
                {
                    System.Windows.MessageBox.Show("Stop at question " + Questions.Count);
                    return false;
                }
                Questions.Add(question);
            }
            return true;
        }
    }
}
