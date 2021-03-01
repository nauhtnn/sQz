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
        public QuestPack QuestionPack;

        public AnswerPack mKeyPack;

        public Dictionary<int, DateTime> vT1;
        public Dictionary<int, DateTime> vT2;
        public ExamStt eStt;

        public ExamSlotA()
        {
            mDt = DT.INVALID;
            eStt = ExamStt.Prep;
            QuestionPack = new QuestPack();

            mKeyPack = new AnswerPack();
        }

        public DateTime Dt {
            get { return mDt; }
            set {
                mDt = value;
                QuestionPack.mDt = value;
            }
        }
    }
}
