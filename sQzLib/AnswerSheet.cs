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
        public int BytesOfAnswer_Length;
        public int Subject;
        public int QuestSheetID;
        public bool bChanged;
        public byte[] BytesOfAnswer;
        public string tAns
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in BytesOfAnswer)
                    sb.Append(Convert.ToChar(b));

                return sb.ToString();
            }
        }

        public AnswerSheet() {
            bChanged = false;
            BytesOfAnswer = null;
            QuestSheetID = ExamineeA.LV_CAP;
        }

        public void Init(QuestSheet qsheet)
        {
            QuestSheetID = qsheet.ID;
            BytesOfAnswer_Length = qsheet.CountAllQuestions() * OptionSelectAnswer.OPTION_COUNT;
            if (BytesOfAnswer == null)
            {
                BytesOfAnswer = new byte[BytesOfAnswer_Length];
                for(int i = 0; i < BytesOfAnswer_Length; ++i)
                    BytesOfAnswer[i] = QuestionAnswer.NO_CHOICE;
            }
        }

        public byte[] GetBytes_S0SendingToS1()
        {
            byte[] buf = new byte[8 + BytesOfAnswer_Length];
            int offs = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(QuestSheetID),
                        0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(BytesOfAnswer_Length),
                        0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BytesOfAnswer, 0, buf, offs, BytesOfAnswer_Length);
            return buf;
        }

        public bool ReadBytes_S1ReceivingFromS0(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            QuestSheetID = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;

            if (l < 4)
                return true;
            BytesOfAnswer_Length = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;

            if (l < BytesOfAnswer_Length)
                return true;
            BytesOfAnswer = new byte[BytesOfAnswer_Length];
            Buffer.BlockCopy(buf, offs, BytesOfAnswer, 0, BytesOfAnswer_Length);
            offs += BytesOfAnswer_Length;
            return false;
        }

        public double Grade(byte[] ans)
        {
            if (ans == null)
                return 101;
            if (BytesOfAnswer == null)
                return 102;
            if (ans.Length != BytesOfAnswer.Length)
                return 103;
            double grade = 0;
            int offs = 0;
            int part1QuestionCount = 22;
            while(offs < BytesOfAnswer.Length)
            {
                int offs4 = offs + 4;
                if (part1QuestionCount > 0)
                {
                    grade += SingleAnswer.S().Grade(ans, BytesOfAnswer, offs);
                    part1QuestionCount--;
                }
                else
                    grade += MTFAnswer.S().Grade(ans, BytesOfAnswer, offs);
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
