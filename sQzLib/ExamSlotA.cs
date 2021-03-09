using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;

namespace sQzLib
{
    public enum ExamStt
    {
        Prep = 0,
        Oper,
        Arch
    }

    public abstract class ExamSlotA
    {
        public DateTime mDt;
        public Dictionary<int, QuestPack> QuestionPacks;

        public AnswerPack mKeyPack;

        public Dictionary<int, DateTime> vT1;
        public Dictionary<int, DateTime> vT2;
        public ExamStt eStt;

        public ExamSlotA()
        {
            mDt = DT.INVALID;
            eStt = ExamStt.Prep;
            QuestionPacks = new Dictionary<int, QuestPack>();

            mKeyPack = new AnswerPack();
        }

        public DateTime Dt {
            get { return mDt; }
            set {
                mDt = value;
                foreach(QuestPack p in QuestionPacks.Values)
                    p.mDt = value;
            }
        }
    }
}
