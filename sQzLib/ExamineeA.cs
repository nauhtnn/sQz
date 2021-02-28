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
        //public int LvId { get { return (eLv == ExamLv.A) ? uId : uId + LV_CAP; } }
        //public string tId { get { return eLv.ToString() + uId.ToString("d4"); } }
        //public static string gId(ExamLv lv, int id) { return lv.ToString() + id.ToString("d4"); }
        public string Name;
        public string Birthdate;
        public string Birthplace;
        public int Grade;

        public string ComputerName;
        public DateTime dtTim1;
        public DateTime dtTim2;
        public AnsSheet mAnsSh;

        public abstract void Reset();

        protected void _Reset()
        {
            mDt = DT.INVALID;
            Name = null;
            Birthdate = null;
            Birthplace = null;
            eStt = NeeStt.Signing;
            Grade = LV_CAP;
            dtTim1 = dtTim2 = DT.INVALID;
            ComputerName = string.Empty;
            mAnsSh = new AnsSheet();
        }

        //public void ToByte(out byte[] buf, int prfx)
        //{
        //    List<byte[]> l = new List<byte[]>();// GetBytes_ClientSendingToS1();
        //    int sz = 4;
        //    foreach (byte[] i in l)
        //        sz += i.Length;
        //    buf = new byte[sz];
        //    sz = 0;
        //    Buffer.BlockCopy(BitConverter.GetBytes(prfx), 0, buf, sz, 4);
        //    sz += 4;
        //    foreach (byte[] i in l)
        //    {
        //        Buffer.BlockCopy(i, 0, buf, sz, i.Length);
        //        sz += i.Length;
        //    }
        //}

        //public void ToByte(out byte[] buf)
        //{
        //    List<byte[]> l = new List<byte[]>();// GetBytes_ClientSendingToS1();
        //    int sz = 0;
        //    foreach (byte[] i in l)
        //        sz += i.Length;
        //    buf = new byte[sz];
        //    sz = 0;
        //    foreach (byte[] i in l)
        //    {
        //        Buffer.BlockCopy(i, 0, buf, sz, i.Length);
        //        sz += i.Length;
        //    }
        //}

        //public string Grade { get { return Math.Round((float)uGrade * 0.333, 1).ToString(); } }
    }
}
