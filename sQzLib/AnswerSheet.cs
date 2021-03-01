using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace sQzLib
{
    public class AnswerSheet
    {
        public const int LEN = 120;
        public int questSheetID;
        public int uQSId { get { return (ExamineeA.LV_CAP < questSheetID) ? questSheetID - ExamineeA.LV_CAP : questSheetID; } }
        public bool bChanged;
        public byte[] aAns;
        public string tAns
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in aAns)
                    sb.Append((b == 0) ? Question.C0 : Question.C1);
                return sb.ToString();
            }
        }

        public AnswerSheet() {
            bChanged = false;
            aAns = null;
            questSheetID = ExamineeA.LV_CAP;
        }

        public void Init(int uqslvid)
        {
            questSheetID = uqslvid;
            if (aAns == null)
            {
                aAns = new byte[LEN];
                for(int i = 0; i < LEN; ++i)
                    aAns[i] = 0;
            }
        }

        public int GetByteCount()
        {
            return 4 + LEN;
        }

        public void ToByte(ref byte[] buf, ref int offs)//todo: opt-out?
        {
            Buffer.BlockCopy(BitConverter.GetBytes(questSheetID),
                        0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(aAns, 0, buf, offs, LEN);
            offs += LEN;
        }

        public byte[] ToByte()
        {
            byte[] buf = new byte[4 + LEN];
            int offs = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(questSheetID),
                        0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(aAns, 0, buf, offs, LEN);
            return buf;
        }

        public bool ReadByte(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            questSheetID = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (l < LEN)
                return true;
            aAns = new byte[LEN];
            Buffer.BlockCopy(buf, offs, aAns, 0, LEN);
            offs += LEN;
            return false;
        }

        public int Grade(byte[] ans)
        {
            if (ans == null)
                return 101;
            if (aAns == null)
                return 102;
            if (ans.Length != aAns.Length)
                return 103;
            int grade = 0;
            int offs = 0;
            while(offs < aAns.Length)
            {
                int offs4 = offs + 4;
                for(; offs < offs4; ++offs)
                    if (ans[offs] != aAns[offs])
                        break;
                if (offs == offs4)
                    ++grade;
                offs = offs4;
            }
            return grade;
        }

        public void Disable()
        {
            //foreach (ListBox lbx in OptionContainers)
            //    lbx.IsEnabled = false;
        }
    }
}
