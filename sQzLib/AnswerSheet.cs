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
        public AnswerType[] QuestionTypes;
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
            QuestionTypes = null;
        }

        public void Init(QuestSheet qsheet)
        {
            QuestSheetID = qsheet.ID;
            QuestionTypes = null;
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
            List<byte[]> bytes = new List<byte[]>();
            bytes.Add(BitConverter.GetBytes(QuestSheetID));
            bytes.Add(BitConverter.GetBytes(QuestionTypes.Length));
            foreach (AnswerType type in QuestionTypes)
                bytes.Add(BitConverter.GetBytes((int)type));

            bytes.Add(BitConverter.GetBytes(BytesOfAnswer_Length));

            bytes.Add(BytesOfAnswer);

            return Utils.ToArray_FromListOfBytes(bytes);
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
            int questCount = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;

            if (l < sizeof(int) * questCount)
                return true;
            QuestionTypes = new AnswerType[questCount];
            for(int i = 0; i < questCount; ++i)
            {
                int type;
                if (!Enum.IsDefined(typeof(AnswerType), type = BitConverter.ToInt32(buf, offs)))
                    return true;
                QuestionTypes[i] = (AnswerType)type;

                offs += 4;
                l -= 4;
            }

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
            var typeItor = QuestionTypes.GetEnumerator();

            if (typeItor == null)
                return 999;

            while(offs < BytesOfAnswer.Length)
            {
                typeItor.MoveNext();

                int offs4 = offs + 4;

                if ((AnswerType)typeItor.Current == AnswerType.MultipleTrueFalse)
                    grade += MTFAnswer.S().Grade(ans, BytesOfAnswer, offs);
                else
                    grade += SingleAnswer.S().Grade(ans, BytesOfAnswer, offs);

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
