using System;
using System.Collections.Generic;

namespace sQzLib
{
    struct MultiChoiceData
    {
        public NonnullRichTextBuilder Stem;
        public NonnullRichTextBuilder[] Options;

        public MultiChoiceData(Queue<NonnullRichTextBuilder> richTexts, int n_options)
        {
            if (richTexts.Count < 5)
                throw new ArgumentException();
            Stem = richTexts.Dequeue();
            Options = new NonnullRichTextBuilder[n_options];
            for(int i = 0; i < n_options; ++i)
                Options[i] = richTexts.Dequeue();
        }
    }

    public class MultiChoiceItem
    {
        public static readonly int N_OPTIONS = 4;// ReadNOptions();
        public const char C0 = '0';
        public const char C1 = '1';
        public int ID_in_DB { get; private set; }
        public NonnullRichText Stem { get; private set; }
        public IUx mIU { get; private set; }
        public NonnullRichText[] Options { get; private set; }
        public bool[] Keys { get; private set; }
        public int[] PermutedOptions { get; private set; }
        public bool IsDifficult { get; private set; }

        static readonly IUx[] IUs_A = { IUx._1, IUx._2, IUx._3, IUx._4, IUx._5, IUx._6 };
        static readonly IUx[] IUs_B = { IUx._7, IUx._8, IUx._10 };
        static readonly IUx[] IUs_C = { IUx._7, IUx._8, IUx._9 };

        public static IUx[] GetIUs(Level lv)
        {
            if (lv == Level.A)
                return IUs_A;
            else if (lv == Level.B)
                return IUs_B;
            return IUs_C;
        }

        MultiChoiceItem() { }

        public MultiChoiceItem(int DB_ID, NonnullRichText[] cleanData, bool[] keys, bool isDifficult)
        {
            ID_in_DB = DB_ID;
            Stem = cleanData[0];
            Options = new NonnullRichText[N_OPTIONS];
            Keys = new bool[N_OPTIONS];
            PermutedOptions = new int[N_OPTIONS];
            for (int i = 0; i < N_OPTIONS; ++i)
            {
                Options[i] = cleanData[i + 1];
                Keys[i] = keys[i];
                PermutedOptions[i] = i;
            }
            IsDifficult = isDifficult;
        }

        //static int ReadNOptions()
        //{
        //    //if (System.IO.File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\3.txt"))
        //    //    return 3;
        //    return 4;
        //}

        public static MultiChoiceItem NewWith(Queue<NonnullRichTextBuilder> richTexts)
        {
            MultiChoiceItem question = new MultiChoiceItem();
            MultiChoiceData questTexts = new MultiChoiceData(richTexts, N_OPTIONS);
            string stem = questTexts.Stem.FirstStringOrDefault();
            if (stem[0] == '*')
            {
                question.IsDifficult = true;
                questTexts.Stem.Trunc1AtLeft();
            }
            else if (1 < stem.Length && stem[0] == '\\' && 
                (stem[1] == '*' || stem[1] == '\\'))
            {
                question.IsDifficult = false;
                questTexts.Stem.Trunc1AtLeft();
            }
            else
                question.IsDifficult = false;
            question.Stem = new NonnullRichText(questTexts.Stem);

            question.Options = new NonnullRichText[N_OPTIONS];
            question.Keys = new bool[N_OPTIONS];
            question.PermutedOptions = new int[N_OPTIONS];
            for (int i = 0; i < N_OPTIONS; ++i)
            {
                string isKey = questTexts.Options[i].FirstStringOrDefault();
                if(isKey == null)
                    question.Keys[i] = false;
                else if(isKey[0] == '\\')
                {
                    if(isKey.Length == 1)
                    {
                        if(questTexts.Options[i].Runs.Count == 1)
                            question.Keys[i] = false;
                        else
                        {
                            question.Keys[i] = true;
                            questTexts.Options[i].Runs.RemoveAt(0);
                        }
                    }
                    else
                    {
                        if (isKey[1] != '\\')
                            question.Keys[i] = true;
                        else
                            question.Keys[i] = false;
                        questTexts.Options[i].Trunc1AtLeft();
                    }
                }
                else
                    question.Keys[i] = false;
                question.Options[i] = new NonnullRichText(questTexts.Options[i]);

                question.PermutedOptions[i] = i;
            }
            bool noAnswer = true;
            for (int i = 0; i < N_OPTIONS; ++i)
                if (question.Keys[i])
                    noAnswer = false;
            if(noAnswer)
                throw new ArgumentNullException();
            return question;
        }

        public IEnumerable<string> ToListOfStrings()
        {
            throw new NotImplementedException();
        }

        public static void DBDelete(IUx eIU, string ids) {
            DBConnect.Update("sqz_question", "del=1", ids);
        }

        public MultiChoiceItem DeepCopy()
        {
            MultiChoiceItem q = new MultiChoiceItem();
            q.ID_in_DB = ID_in_DB;
            q.Stem = Stem;
            q.mIU = mIU;
            q.Options = new NonnullRichText[N_OPTIONS];
            for (int i = 0; i < N_OPTIONS; ++i)
                q.Options[i] = Options[i];
            q.Keys = new bool[N_OPTIONS];
            for (int i = 0; i < N_OPTIONS; ++i)
                q.Keys[i] = Keys[i];
            q.PermutedOptions = new int[N_OPTIONS];
            for (int i = 0; i < N_OPTIONS; ++i)
                q.PermutedOptions[i] = PermutedOptions[i];
            return q;
        }

        public void Randomize(Random rand)
        {
            NonnullRichText[] options = new NonnullRichText[N_OPTIONS];
            bool[] keys = new bool[N_OPTIONS];
            int[] originIdx = new int[N_OPTIONS];
            List<int> l = new List<int>();
            int n = N_OPTIONS;
            for (int i = 0; i < n; ++i)
                l.Add(i);
            while (0 < n)
            {
                int lidx = rand.Next() % n;
                int idx = l[lidx];
                l.RemoveAt(lidx);
                --n;
                options[n] = Options[idx];
                keys[n] = Keys[idx];
                originIdx[n] = idx;
            }
            Options = options;
            Keys = keys;
            PermutedOptions = originIdx;
        }

        public MultiChoiceItem RandomizeDeepCopy(Random rand)
        {
            MultiChoiceItem q = new MultiChoiceItem();
            q.ID_in_DB = ID_in_DB;
            q.Stem = Stem;
            q.mIU = mIU;
            q.IsDifficult = IsDifficult;
            //randomize
            q.Options = new NonnullRichText[N_OPTIONS];
            q.Keys = new bool[N_OPTIONS];
            List<int> l = new List<int>();
            for (int i = 0; i < N_OPTIONS; ++i)
                l.Add(i);
            int n = N_OPTIONS;
            while (0 < n)
            {
                int lidx = rand.Next() % n;
                int idx = l[lidx];
                l.RemoveAt(lidx);
                --n;
                q.Options[n] = Options[idx];
                q.Keys[n] = Keys[idx];
                q.PermutedOptions[n] = idx;
            }
            return q;
        }
    }

    public enum QuestType
    {
        Single = 1,
        Multiple = 2,
        Insertion = 4,
        Selection = 8,
        Matching = 16
    }

    public enum ContentType
    {
        Raw = 1,
        Image = 2,
        Audio = 4,
        Video = 8
    }

    public enum TokenType
    {
        Requirement = 0,
        Stem = 1,
        Ans = 2,
        Both = 3
    }

    public enum IUx
    {
        _1 = 0, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15, _0
    }

    public enum Level
    {
        A,
        B,
        C,
        MAX_COUNT_EACH_LEVEL = 10000, //unsigned smallint 65535
    }

    public enum Difficulty
    {
        Easy,
        Difficult,
        Both
    }
}
