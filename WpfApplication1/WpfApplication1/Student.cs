using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    public enum ExamLvl
    {
        Basis = 0,
        Advance = 1
    }
    public class Student
    {
        static List<Student> svStudent = new List<Student>();
        ExamLvl mLvl;
        Int16 mId;
        string mName;
        string mBirthdate;
        string mBirthplace;
        public Student() { }
        public static void ReadTxt(Int16 dateId)
        {
            ReadTxt(sQzCS.Utils.ReadFile("Students" + dateId + ".txt"));
        }
        public static void ReadTxt(string buf)
        {
            if (buf == null)
                return;
            svStudent.Clear();
            string[] students = buf.Split('\n');
            foreach(string stu in students)
            {
                Student stud = new Student();
                string[] s = stu.Split('\t');
                if(s.Length == 4) //todo: hardcode, unsafe
                {
                    if ((s[0])[0] == 'A')
                        stud.mLvl = ExamLvl.Basis;
                    else
                        stud.mLvl = ExamLvl.Advance;
                    stud.mId = Convert.ToInt16(s[0].Substring(1));
                    stud.mName = s[1];
                    stud.mBirthdate = s[2];
                    stud.mBirthplace = s[3];
                }
                svStudent.Add(stud);
            }
        }
        public static byte[] ToByteArr()
        {
            if (svStudent.Count == 0)
                return null;
            List<byte[]> l = new List<byte[]>();
            byte[] b = BitConverter.GetBytes(svStudent.Count);
            l.Add(b);
            for(int i = 0; i < svStudent.Count; ++i)
            {
                Student s = svStudent[i];
                b = BitConverter.GetBytes((Int16)s.mLvl);
                l.Add(b);
                b = BitConverter.GetBytes(s.mId);
                l.Add(b);
                b = Encoding.UTF32.GetBytes(s.mName);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
                b = Encoding.UTF32.GetBytes(s.mBirthdate);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
                b = Encoding.UTF32.GetBytes(s.mBirthplace);
                l.Add(BitConverter.GetBytes(b.Length));
                l.Add(b);
            }
            int sz = 0;
            foreach (byte[] i in l)
                sz += i.Length;
            b = new byte[sz];
            int offs = 0;
            for (int i = 0; i < l.Count; ++i)
            {
                Buffer.BlockCopy(l[i], 0, b, offs, l[i].Length);
                offs += l[i].Length;
            }
            return b;
        }
        public static void ReadByteArr(byte[] buf)
        {
            svStudent.Clear();
            if (buf == null)
                return;
            int offs = 0;
            int nStu = BitConverter.ToInt32(buf, offs);
            offs += 4;
            for(int i = 0; i < nStu; ++i)
            {
                Student s = new Student();
                s.mLvl = (ExamLvl)BitConverter.ToInt16(buf, offs);
                offs += 2;
                s.mId = BitConverter.ToInt16(buf, offs);
                offs += 2;
                int sz = BitConverter.ToInt32(buf, offs);
                offs += 4;
                byte[] b = new byte[sz];
                Buffer.BlockCopy(buf, offs, b, 0, sz);
                s.mName = Encoding.UTF32.GetString(b);
                offs += sz;
                sz = BitConverter.ToInt32(buf, offs);
                offs += 4;
                b = new byte[sz];
                Buffer.BlockCopy(buf, offs, b, 0, sz);
                s.mBirthdate = Encoding.UTF32.GetString(b);
                offs += sz;
                sz = BitConverter.ToInt32(buf, offs);
                offs += 4;
                b = new byte[sz];
                Buffer.BlockCopy(buf, offs, b, 0, sz);
                s.mBirthplace = Encoding.UTF32.GetString(b);
                svStudent.Add(s);
                offs += sz;
            }
        }
    }
}
