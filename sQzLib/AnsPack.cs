﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sQzLib
{
    public class AnsPack
    {
        public Dictionary<uint, AnsSheet> vSheet;
        public AnsPack()
        {
            vSheet = new Dictionary<uint, AnsSheet>();
        }

        public int GetByteCount()
        {
            int sz = 4;
            foreach (AnsSheet s in vSheet.Values)
                sz += s.GetByteCount();
            return sz;
        }

        //only Operation0 uses this.
        public bool ToByte(ref byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            Buffer.BlockCopy(BitConverter.GetBytes(vSheet.Values.Count), 0, buf, offs, 4);//opt?
            offs += 4;
            //l -= 4;
            foreach (AnsSheet i in vSheet.Values)
                i.ToByte(ref buf, ref offs);
            l = buf.Length - offs;
            return false;
        }

        //only Operation0 uses this.
        public void ExtractKey(QuestPack p)
        {
            foreach(QuestSheet qs in p.vSheet.Values)
            {
                AnsSheet i = new AnsSheet();
                i.ExtractKey(qs);
                AnsSheet x;
                if(!vSheet.TryGetValue(i.mId, out x))
                    vSheet.Add(i.mId, i);
            }
        }

        //only Operation1 uses this.
        public void ReadByte(byte[] buf, ref int offs)
        {
            vSheet.Clear();
            if (buf == null)
                return;
            int l = buf.Length - offs;
            if (l < 4)
                return;
            int nSh = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (nSh < 1)
                return;
            while (0 < nSh)
            {
                AnsSheet i = new AnsSheet();
                i.ReadByte(buf, ref offs);
                //if (err)
                //    break;
                //if (!vSheet.TryGetValue(qs.mId, out qs))//todo safer
                    vSheet.Add(i.mId, i);
                --nSh;
            }
        }
    }
}