using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace sQzLib
{
    class BasicRich_PlainTextQuestParser
    {
        Dictionary<SectionID, List<string>> SectionMagicKeywords;
        string SECTION_MAGIC_PREFIX;
        const string SECTION_MAGIC_CFG_FILEPATH = "sectionMagicKeywords.txt";

        public BasicRich_PlainTextQuestParser()
        {
            LoadSectionMagicKeywords();
        }

        private void LoadSectionMagicKeywords()
        {
            InitDefaultMagicKeywords();
            if (!System.IO.File.Exists(SECTION_MAGIC_CFG_FILEPATH))
                return;

            string[] lines;
            try
            {
                lines = System.IO.File.ReadAllLines(SECTION_MAGIC_CFG_FILEPATH);
            }
            catch(System.IO.IOException e)
            {
                System.Windows.MessageBox.Show("LoadSectionMagicKeywords error\n" + e.ToString());
                return;
            }

            if (lines.Length == 0)
                return;

            SECTION_MAGIC_PREFIX = lines[0].Trim();

            for(int i = 1; i < lines.Length; ++i)
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

        private void InitDefaultMagicKeywords()
        {
            SECTION_MAGIC_PREFIX = string.Empty;
            SectionMagicKeywords = new Dictionary<SectionID, List<string>>();
            List<string> magicKeywords = new List<string>();
            magicKeywords.Add("following [text|passage]");
            magicKeywords.Add("blank");
            SectionMagicKeywords.Add(SectionID.PassageWithBlanks, magicKeywords);
            magicKeywords = new List<string>();
            magicKeywords.Add("following [text|passage]");
            SectionMagicKeywords.Add(SectionID.SimplePassage, magicKeywords);
        }

        public Tuple<List<Question>, List<PassageWithQuestions>> ParseTokens(Queue<BasicRich_PlainText> tokens)
        {
            TrimToFirstSection(tokens);
            List<Question> independentQuestions = ParseQuestions(tokens);
            List<PassageWithQuestions> passageQuestions = ParsePassages(tokens);
            return new Tuple<List<Question>, List<PassageWithQuestions>>(independentQuestions, passageQuestions);
        }

        private SectionID SelectSection(string text)
        {
            if(RegexIsMatch(text, SectionMagicKeywords[SectionID.PassageWithBlanks]))
                return SectionID.PassageWithBlanks;
            if (RegexIsMatch(text, SectionMagicKeywords[SectionID.SimplePassage]))
                return SectionID.SimplePassage;
            return SectionID.DefaultIndependentQuestions;
        }

        private bool RegexIsMatch(string text, List<string> patterns)
        {
            bool matching = true;
            foreach (string pattern in patterns)
            {
                if (!Regex.IsMatch(text, pattern))
                {
                    matching = false;
                    break;
                }
            }
            return matching;
        }

        private void TrimToFirstSection(Queue<BasicRich_PlainText> tokens)
        {
            if (SECTION_MAGIC_PREFIX.Length == 0)
                return;
            while(tokens.Count > 0)
            {
                if (!tokens.Peek().StartsWith(SECTION_MAGIC_PREFIX))
                    tokens.Dequeue();
                else
                    return;
            }
        }

        private List<Question> ParseQuestions(Queue<BasicRich_PlainText> tokens)
        {
            List<Question> questions = new List<Question>();
            while (tokens.Count > 0)
            {
                if (//sQzLib.Utils.CleanFront(
                    tokens.Peek().StartsWith(SECTION_MAGIC_PREFIX))
                    break;

                Question question = Parse1Question(tokens);
                if (question == null)
                {
                    System.Windows.MessageBox.Show("Stop at question " + questions.Count);
                    break;
                }
                questions.Add(question);
            }

            return questions;
        }

        private List<PassageWithQuestions> ParsePassages(Queue<BasicRich_PlainText> tokens)
        {
            List<PassageWithQuestions> passageQuestions = new List<PassageWithQuestions>();
            while (tokens.Count > 0)
            {
                PassageWithQuestions passageQuestion = Parse1Passage(tokens);
                if (passageQuestion == null)
                {
                    System.Windows.MessageBox.Show("Stop at passage question " + passageQuestions.Count);
                    break;
                }
                passageQuestions.Add(passageQuestion);
            }
            return passageQuestions;
        }

        private Question Parse1Question(Queue<BasicRich_PlainText> tokens)
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
            if(key_label < 'A' || 'D' < key_label)
            {
                System.Windows.MessageBox.Show("From the end, line " + tokens.Count + " has key: " + key_label);
                return null;
            }
            question.vKeys = new bool[Question.NUMBER_OF_OPTIONS];
            for (int j = 0; j < Question.NUMBER_OF_OPTIONS; ++j)
                question.vKeys[j] = false;
            question.vKeys[key_label - 'A'] = true;
            return question;
        }

        private PassageWithQuestions Parse1Passage(Queue<BasicRich_PlainText> tokens)
        {
            if (tokens.Count == 0)
                return null;
            if (//sQzLib.Utils.CleanFront(
                !tokens.Dequeue().StartsWith(SECTION_MAGIC_PREFIX))
                return null;
            PassageWithQuestions passageQuest = new PassageWithQuestions();
            passageQuest.Passage = tokens.Dequeue().ToString();
            passageQuest.Questions = ParseQuestions(tokens);
            return passageQuest;
        }
    }

    public enum SectionID
    {
        DefaultIndependentQuestions = 0,
        SimplePassage,
        PassageWithBlanks
    }
}
