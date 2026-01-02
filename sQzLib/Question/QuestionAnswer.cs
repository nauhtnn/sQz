using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    

    public abstract class QuestionAnswer
    {
        public const byte FALSE = (byte) '0';
        public const byte TRUE = (byte)'1';
        public const byte NO_CHOICE = (byte) '9';

        public abstract double Grade(byte[] answer, byte[] answerKey, int offset);

        public abstract bool IsIgnoredLetter(char letter);

        public abstract byte[] ParseAnswerKey(char[] answerKey);
    }

    public abstract class OptionSelectAnswer: QuestionAnswer
    {
        public const int OPTION_COUNT = 4;

        public override bool IsIgnoredLetter(char letter)
        {
            return (letter == ' ' || letter == '\t' ||
                letter == '.' || letter == ',' || letter == ':');
        }
    }

    public sealed class SingleAnswer: OptionSelectAnswer
    {
        private static SingleAnswer Singleton = null;

        public static SingleAnswer S()
        {
            if (Singleton == null)
                Singleton = new SingleAnswer();
            return Singleton;
        }

        public override byte[] ParseAnswerKey(char[] letters)
        {
            if (letters.Length == 0)
                return null;

            byte[] answerKey = new byte[OPTION_COUNT];

            bool isSet = false;

            foreach (char letter in letters)
            {
                if (IsIgnoredLetter(letter))
                    continue;

                if (!isSet && 'A' <= letter && letter <= 'D')
                {
                    isSet = true;
                    for (int i = 0; i < OPTION_COUNT; i++)
                    {
                        answerKey[i] = ('A' + i == letter) ? TRUE : FALSE;
                    }
                }
                else
                    return null;
            }

            if (isSet)
                return answerKey;
            else
                return null;
        }

        public override double Grade(byte[] answer, byte[] answerKey, int offset)
        {
            for(int i = 0; i < OPTION_COUNT; ++i)
            {
                if((answer[offset + i] == TRUE && answerKey[offset + i] != TRUE) ||
                    (answer[offset + i] != TRUE && answerKey[offset + i] == TRUE))
                {
                    return 0;
                }
            }
            return 0.25;
        }
    }

    public sealed class MTFAnswer : OptionSelectAnswer
    {
        private static MTFAnswer Singleton = null;

        public static MTFAnswer S()
        {
            if (Singleton == null)
                Singleton = new MTFAnswer();
            return Singleton;
        }

        public override byte[] ParseAnswerKey(char[] letters)
        {
            if (letters.Length == 0)
                return null;

            int optionIdx = 0;
            byte[] answerKey = new byte[OPTION_COUNT];

            foreach (char letter in letters)
            {
                if (IsIgnoredLetter(letter))
                    continue;

                if (optionIdx < OPTION_COUNT)
                {
                    if(letter == 'Đ')
                        answerKey[optionIdx] = TRUE;
                    else
                    {
                        if (letter == 'S')
                            answerKey[optionIdx] = FALSE;
                        else
                            return null;
                    }
                    optionIdx++;
                }
                else
                    return null;
            }

            return answerKey;
        }

        public override double Grade(byte[] answer, byte[] answerKey, int offset)
        {
            int count = 0;
            for (int i = 0; i < OPTION_COUNT; ++i)
            {
                if (answerKey[offset + i] == answer[offset + i])
                {
                    ++count;
                }
            }
            switch(count)
            {
                case 1:
                    return 0.1;
                case 2:
                    return 0.25;
                case 3:
                    return 0.5;
                case OPTION_COUNT:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}
