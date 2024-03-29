﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace sQzLib
{
    public class AnsSheet
    {
        public const int LEN = 120;
        public ListBox[] vlbxAns;
        //public OptionView[][] vAnsItem;
        public int uQSLvId;
        public int uQSId { get { return ((int)Level.MAX_COUNT_EACH_LEVEL < uQSLvId) ? uQSLvId - (int)Level.MAX_COUNT_EACH_LEVEL : uQSLvId; } }
        public bool bChanged;
        DgEvntCB dgSelChgCB;
        public byte[] aAns;
        public string tAns
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in aAns)
                    sb.Append((b == 0) ? MultiChoiceItem.C0 : MultiChoiceItem.C1);
                return sb.ToString();
            }
        }

        public AnsSheet() {
            bChanged = false;
            aAns = null;
            uQSLvId = (int)Level.MAX_COUNT_EACH_LEVEL;
            dgSelChgCB = null;
        }

        public void Init(int uqslvid)
        {
            uQSLvId = uqslvid;
            if (aAns == null)
            {
                aAns = new byte[LEN];
                for(int i = 0; i < LEN; ++i)
                    aAns[i] = 0;
            }
        }

        public void InitView(QuestSheet qs, double w, DgEvntCB cb)
        {
            throw new NotImplementedException();
            //if (cb != null)
            //    dgSelChgCB = cb;

            //vlbxAns = new ListBox[qs.Questions.Count];
            //vAnsItem = new OptionView[qs.Questions.Count][];
            
            //int idx = -1;
            //int j = -1;
            //foreach (MultiChoiceItem q in qs.ShallowCopy())
            //{
            //    ++idx;
            //    ListBox lbxAns = new ListBox();
            //    lbxAns.Width = w;
            //    lbxAns.Name = "_" + idx;
            //    //lbxAns.SelectionChanged += Ans_SelectionChanged;
            //    lbxAns.BorderBrush = Theme.Singleton.DefinedColors[(int)BrushId.Ans_TopLine];
            //    lbxAns.BorderThickness = new Thickness(0, 4, 0, 0);
            //    vlbxAns[idx] = lbxAns;
            //    vAnsItem[idx] = new OptionView[MultiChoiceItem.N_OPTIONS];
            //    for (int i = 0; i < MultiChoiceItem.N_OPTIONS; ++i)
            //    {
            //        OptionView ai = new OptionView("xx", i, w);// q.Options[i], i, w);
            //        ++j;//update view from log
            //        if (aAns[j] == Convert.ToByte(true))
            //        {
            //            ai.AnsCellLabel.Content = 'X';
            //            ai.mLbxItem.IsSelected = true;
            //        }
            //        vAnsItem[idx][i] = ai;
            //        lbxAns.Items.Add(ai.lbxi);
            //    }
            //}
        }

        public int GetByteCount()
        {
            return 4 + LEN;
        }

        public void ToByte(ref byte[] buf, ref int offs)//todo: opt-out?
        {
            Buffer.BlockCopy(BitConverter.GetBytes(uQSLvId),
                        0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(aAns, 0, buf, offs, LEN);
            offs += LEN;
        }

        public byte[] ToByte()
        {
            byte[] buf = new byte[4 + LEN];
            int offs = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(uQSLvId),
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
            uQSLvId = BitConverter.ToInt32(buf, offs);
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
            foreach (ListBox lbx in vlbxAns)
                lbx.IsEnabled = false;
        }
    }
}
