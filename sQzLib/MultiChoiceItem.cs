﻿using System;
using System.Collections.Generic;

namespace sQzLib
{
    public class MultiChoiceItem
    {
        public static readonly int N_OPTIONS = ReadNOptions();
        public const char C0 = '0';
        public const char C1 = '1';
        public int ID_in_DB { get; private set; }
        public string Stem { get; private set; }
        public IUx mIU { get; private set; }
        public string[] Options { get; private set; }
        public bool[] Keys { get; private set; }
        public int[] POptions { get; private set; }
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

        public MultiChoiceItem() { }

        public MultiChoiceItem(int DB_ID, string[] cleanData, bool[] keys, bool isDifficult)
        {
            ID_in_DB = DB_ID;
            Stem = cleanData[0];
            Options = new string[N_OPTIONS];
            Keys = new bool[N_OPTIONS];
            POptions = new int[N_OPTIONS];
            for (int i = 0; i < N_OPTIONS; ++i)
            {
                Options[i] = cleanData[i + 1];
                Keys[i] = keys[i];
                POptions[i] = i;
            }
            IsDifficult = isDifficult;
        }

        static int ReadNOptions()
        {
            //if (System.IO.File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\3.txt"))
            //    return 3;
            return 4;
        }

        public void Parse(string[] rawData, int dataIdx)
        {
            Stem = rawData[dataIdx];
            if (1 < Stem.Length && Stem[0] == '*')
            {
                IsDifficult = true;
                Stem = Stem.Substring(1);
            }
            else if (1 < Stem.Length && Stem[0] == '\\' && 
                (Stem[1] == '*' || Stem[1] == '\\'))
            {
                Stem = Stem.Substring(1);
                IsDifficult = false;
            }
            else
                IsDifficult = false;

            Options = new string[N_OPTIONS];
            Keys = new bool[N_OPTIONS];
            POptions = new int[N_OPTIONS];
            ++dataIdx;
            for (int i = 0; i < N_OPTIONS; ++i)
            {
                Options[i] = rawData[dataIdx + i];
                if (1 < Options[i].Length && Options[i][0] == '\\')
                {
                    if (Options[i][1] != '\\')
                    {
                        Keys[i] = true;
                        Options[i] = Utils.CleanFront(Options[i].Substring(1));
                    }
                    else
                        Options[i] = Options[i].Substring(1);
                }
                else
                    Keys[i] = false;
                POptions[i] = i;
            }
            for (int i = 0; i < N_OPTIONS; ++i)
                if (Keys[i])
                    return;
            throw new ArgumentNullException();
        }

        public IEnumerable<string> ToListOfStrings()
        {
            LinkedList<string> s = new LinkedList<string>();
            s.AddLast(Stem);
            foreach (string i in Options)
                s.AddLast(i);
            return s;
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
            q.Options = new string[N_OPTIONS];
            for (int i = 0; i < N_OPTIONS; ++i)
                q.Options[i] = Options[i];
            q.Keys = new bool[N_OPTIONS];
            for (int i = 0; i < N_OPTIONS; ++i)
                q.Keys[i] = Keys[i];
            q.POptions = new int[N_OPTIONS];
            for (int i = 0; i < N_OPTIONS; ++i)
                q.POptions[i] = POptions[i];
            return q;
        }

        public void Randomize(Random rand)
        {
            string[] anss = new string[N_OPTIONS];
            bool[] keys = new bool[N_OPTIONS];
            int[] asort = new int[N_OPTIONS];
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
                anss[n] = Options[idx];
                keys[n] = Keys[idx];
                asort[n] = idx;
            }
            Options = anss;
            Keys = keys;
            POptions = asort;
        }

        public MultiChoiceItem RandomizeDeepCopy(Random rand)
        {
            MultiChoiceItem q = new MultiChoiceItem();
            q.ID_in_DB = ID_in_DB;
            q.Stem = Stem;
            q.mIU = mIU;
            q.IsDifficult = IsDifficult;
            //randomize
            q.Options = new string[N_OPTIONS];
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
                q.POptions[n] = idx;
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
        C
    }
}