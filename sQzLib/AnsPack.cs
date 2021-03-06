﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class AnsPack
    {
        public SortedList<int, AnsSheet> Sheets;
        public AnsPack()
        {
            Sheets = new SortedList<int, AnsSheet>();
        }

        public int GetByteCount()
        {
            int sz = 4;
            foreach (AnsSheet s in Sheets.Values)
                sz += s.GetByteCount();
            return sz;
        }

        //only Operation0 uses this.
        public bool ToByte(ref byte[] buf, ref int offs)//todo: opt-out?
        {
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            Buffer.BlockCopy(BitConverter.GetBytes(Sheets.Values.Count), 0, buf, offs, 4);
            offs += 4;
            //l -= 4;
            foreach (AnsSheet i in Sheets.Values)
                i.ToByte(ref buf, ref offs);
            l = buf.Length - offs;
            return false;
        }

        public List<byte[]> ToByte()
        {
            List<byte[]> l = new List<byte[]>();
            l.Add(BitConverter.GetBytes(Sheets.Values.Count));
            foreach (AnsSheet i in Sheets.Values)
                l.Add(i.ToByte());
            return l;
        }

        //only Operation0 uses this.
        public void ExtractKey(List<QuestSheet> l)
        {
            foreach(QuestSheet qs in l)
            {
                AnsSheet i = new AnsSheet();
                qs.ExtractKey(i);
                if (!Sheets.ContainsKey(i.uQSLvId))
                    Sheets.Add(i.uQSLvId, i);
                else
                    Sheets[i.uQSLvId] = i;
            }
        }

        public AnsSheet ExtractKey(QuestSheet qs)
        {
            AnsSheet i = new AnsSheet();
            qs.ExtractKey(i);
            if (!Sheets.ContainsKey(i.uQSLvId))
            {
                Sheets.Add(i.uQSLvId, i);
                return i;
            }
            return null;
        }

        //only Operation1 uses this.
        public bool ReadByte(byte[] buf, ref int offs)
        {
            Sheets.Clear();
            if (buf == null)
                return true;
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            int nSh = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (nSh < 0)
                return true;
            while (0 < nSh)
            {
                AnsSheet i = new AnsSheet();
                if (i.ReadByte(buf, ref offs) || Sheets.ContainsKey(i.uQSLvId))
                    return true;
                Sheets.Add(i.uQSLvId, i);
                --nSh;
            }
            return false;
        }
    }
}
