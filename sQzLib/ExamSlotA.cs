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

        public Dictionary<int, AnswerPack> AnswerKeyPacks;

        public Dictionary<int, DateTime> vT1;
        public Dictionary<int, DateTime> vT2;
        public ExamStt eStt;

        public ExamSlotA()
        {
            mDt = DT.INVALID;
            eStt = ExamStt.Prep;
            QuestionPacks = new Dictionary<int, QuestPack>();

            AnswerKeyPacks = new Dictionary<int, AnswerPack>();
        }

        public DateTime Dt {
            get { return mDt; }
            set {
                mDt = value;
                foreach(QuestPack p in QuestionPacks.Values)
                    p.mDt = value;
            }
        }

        protected void Safe_AddToQuestionPacks(QuestPack pack)
        {
            if (QuestionPacks.ContainsKey(pack.TestType))
            {
                System.Windows.MessageBox.Show("QuestionPacks already contained key: " +
                    pack.TestType + ". Now merging.");
                StringBuilder merging_status = new StringBuilder();
                foreach (QuestSheet sheet in pack.vSheet.Values)
                {
                    if (QuestionPacks[pack.TestType].vSheet.ContainsKey(sheet.ID))
                        merging_status.Append(sheet.ID + " duplicated.\n");
                    else
                    {
                        merging_status.Append(sheet.ID + " ok.\n");
                        QuestionPacks[pack.TestType].vSheet.Add(sheet.ID, sheet);
                    }
                }
                System.Windows.MessageBox.Show(merging_status.ToString());
            }
            else
                QuestionPacks.Add(pack.TestType, pack);
        }

        protected void Safe_AddToAnswerPacks(AnswerPack answerPack)
        {
            if (AnswerKeyPacks.ContainsKey(answerPack.TestType))
            {
                System.Windows.MessageBox.Show("AnswerKeyPacks already contained key: " +
                    answerPack.TestType + ". Now merging.");
                StringBuilder merging_status = new StringBuilder();
                foreach (AnswerSheet ansSheet in answerPack.vSheet.Values)
                {
                    if (AnswerKeyPacks[answerPack.TestType].vSheet.ContainsKey(ansSheet.QuestSheetID))
                        merging_status.Append(ansSheet.QuestSheetID + " duplicated.\n");
                    else
                    {
                        merging_status.Append(ansSheet.QuestSheetID + " ok.\n");
                        AnswerKeyPacks[answerPack.TestType].vSheet.Add(ansSheet.QuestSheetID, ansSheet);
                    }
                }
                System.Windows.MessageBox.Show(merging_status.ToString());
            }
            else
                AnswerKeyPacks.Add(answerPack.TestType, answerPack);
        }
    }
}
