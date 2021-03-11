using System;
using System.Collections.Generic;
using System.Text;

namespace sQzLib
{
    public enum NeeStt
    {
         Signing = 0,
         Info,
         Authenticated,
         Examing,
         Submitting,
         Finished
    }

    public abstract class ExamineeA
    {
        public const int LV_CAP = 10000;//db sqz_examinee `id` SMALLINT UNSIGNED
        public DateTime mDt;
        public NeeStt eStt;
        //public ExamLv eLv;
        //public int uId;
        public string ID;
        public int TestType;
        //public int LvId { get { return (eLv == ExamLv.A) ? uId : uId + LV_CAP; } }
        //public string tId { get { return eLv.ToString() + uId.ToString("d4"); } }
        //public static string gId(ExamLv lv, int id) { return lv.ToString() + id.ToString("d4"); }
        public string Name;
        public string Birthdate;
        public int CorrectCount;

        public string ComputerName;
        public DateTime dtTim1;
        public DateTime dtTim2;
        public AnswerSheet AnswerSheet;

        public abstract void Reset();

        protected void _Reset()
        {
            TestType = 0;
            mDt = DT.INVALID;
            Name = null;
            Birthdate = null;
            eStt = NeeStt.Signing;
            CorrectCount = LV_CAP;
            dtTim1 = dtTim2 = DT.INVALID;
            ComputerName = string.Empty;
            AnswerSheet = new AnswerSheet();
        }

        public string Grade {
            get
            {
                return CorrectCount.ToString();
            }
        }
    }
}
