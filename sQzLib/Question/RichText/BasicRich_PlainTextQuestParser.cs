using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    class BasicRich_PlainTextQuestParser
    {
        public static Tuple<List<Question>, List<PassageWithQuestions>> ParseTokens(Queue<BasicRich_PlainText> tokens)
        {
            List<Question> independentQuestions = ParseQuestions(tokens);
            List<PassageWithQuestions> passageQuestions = ParsePassages(tokens);
            return new Tuple<List<Question>, List<PassageWithQuestions>>(independentQuestions, passageQuestions);
        }

        private static List<Question> ParseQuestions(Queue<BasicRich_PlainText> tokens)
        {
            List<Question> questions = new List<Question>();
            while (tokens.Count > 0)
            {
                if (//sQzLib.Utils.CleanFront(
                    tokens.Peek().IndexOf(PassageWithQuestions.MAGIC_WORD) == 0)
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

        private static List<PassageWithQuestions> ParsePassages(Queue<BasicRich_PlainText> tokens)
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

        private static Question Parse1Question(Queue<BasicRich_PlainText> tokens)
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

        private static PassageWithQuestions Parse1Passage(Queue<BasicRich_PlainText> tokens)
        {
            if (tokens.Count == 0)
                return null;
            if (//sQzLib.Utils.CleanFront(
                tokens.Dequeue().IndexOf(PassageWithQuestions.MAGIC_WORD) != 0)
                return null;
            PassageWithQuestions passageQuest = new PassageWithQuestions();
            passageQuest.Passage = tokens.Dequeue().ToString();
            passageQuest.Questions = ParseQuestions(tokens);
            return passageQuest;
        }
    }
}
