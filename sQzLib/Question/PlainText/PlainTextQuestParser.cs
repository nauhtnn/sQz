﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    class PlainTextQuestParser
    {
        public static Tuple<List<Question>, List<BasicPassageSection>> ParseTokens(Queue<string> tokens)
        {
            List<Question> independentQuestions = ParseQuestions(tokens);
            List<BasicPassageSection> passageQuestions = ParsePassages(tokens);
            return new Tuple<List<Question>, List<BasicPassageSection>>(independentQuestions, passageQuestions);
        }

        private static List<Question> ParseQuestions(Queue<string> tokens)
        {
            List<Question> questions = new List<Question>();
            while (tokens.Count > 0)
            {
                if (sQzLib.Utils.CleanFront(tokens.Peek()).IndexOf(BasicPassageSection.MAGIC_WORD) == 0)
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

        private static List<BasicPassageSection> ParsePassages(Queue<string> tokens)
        {
            List<BasicPassageSection> passageQuestions = new List<BasicPassageSection>();
            while (tokens.Count > 0)
            {
                BasicPassageSection passageQuestion = Parse1Passage(tokens);
                if (passageQuestion == null)
                {
                    System.Windows.MessageBox.Show("Stop at passage question " + passageQuestions.Count);
                    break;
                }
                passageQuestions.Add(passageQuestion);
            }
            return passageQuestions;
        }

        private static Question Parse1Question(Queue<string> tokens)
        {
            if (tokens.Count < Question.NUMBER_OF_OPTIONS + 1)
            {
                System.Windows.MessageBox.Show("From the end, line " + tokens.Count + " doesn't have 1 stem 4 options!");
                return null;
            }

            Question question = new Question();
            question.Stem = tokens.Dequeue();
            question.vAns = new string[Question.NUMBER_OF_OPTIONS];
            for (int j = 0; j < Question.NUMBER_OF_OPTIONS;)
                question.vAns[j++] = tokens.Dequeue();
            question.vKeys = new bool[Question.NUMBER_OF_OPTIONS];
            for (int j = 0; j < Question.NUMBER_OF_OPTIONS; ++j)
                question.vKeys[j] = false;
            if (question.Stem[0] == '\\' && 1 < question.Stem.Length
                && (question.Stem[1] == '*' || question.Stem[1] == '\\'))
                question.Stem = question.Stem.Substring(1);
            int nKey = 0;
            for (int j = 0; j < Question.NUMBER_OF_OPTIONS; ++j)
            {
                if (question.vAns[j][0] == '\\' && 1 < question.vAns[j].Length)
                {
                    if (question.vAns[j][1] != '\\')
                    {
                        question.vKeys[j] = true;
                        question.vAns[j] = Utils.CleanFront(question.vAns[j].Substring(1));
                        ++nKey;
                    }
                    else
                        question.vAns[j] = question.vAns[j].Substring(1);
                }
            }
            if (nKey != 1)
            {
                System.Windows.MessageBox.Show("From the end, line " + tokens.Count + " has " + nKey + " key!");
                return null;
            }
            return question;
        }

        private static BasicPassageSection Parse1Passage(Queue<string> tokens)
        {
            if (tokens.Count == 0)
                return null;
            if (sQzLib.Utils.CleanFront(tokens.Dequeue()).IndexOf(BasicPassageSection.MAGIC_WORD) != 0)
                return null;
            BasicPassageSection passageQuest = new BasicPassageSection();
            passageQuest.Passage = tokens.Dequeue();
            passageQuest.Questions = ParseQuestions(tokens);
            return passageQuest;
        }
    }
}
