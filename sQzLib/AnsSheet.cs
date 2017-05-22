using System;
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
        public AnsItem[][] vAnsItem;
        public byte[] aAns;
        public ExamLv eLv;
        public int mLv { get { return (int)eLv; } }
        public int uQSId;
        public bool bChanged;
        DgEvntCB dgSelChgCB;

        public AnsSheet() {
            bChanged = false;
            aAns = null;
            uQSId = ushort.MaxValue;
            dgSelChgCB = null;
        }

        public void Init(int uqsid)
        {
            uQSId = uqsid;
            if (aAns == null)
            {
                aAns = new byte[LEN];
                for(int i = 0; i < LEN; ++i)
                    aAns[i] = 0;
            }
        }

        public void InitView(QuestSheet qs, double w, DgEvntCB cb)
        {
            if (cb != null)
                dgSelChgCB = cb;

            vlbxAns = new ListBox[qs.vQuest.Count];
            vAnsItem = new AnsItem[qs.vQuest.Count][];
            
            int idx = -1;
            int j = -1;
            foreach (Question q in qs.vQuest)
            {
                ++idx;
                ListBox lbxAns = new ListBox();
                lbxAns.Width = w;
                lbxAns.Name = "_" + idx;
                lbxAns.SelectionChanged += Ans_SelectionChanged;
                lbxAns.BorderBrush = Theme.s._[(int)BrushId.Ans_TopLine];
                lbxAns.BorderThickness = new Thickness(0, 4, 0, 0);
                vlbxAns[idx] = lbxAns;
                vAnsItem[idx] = new AnsItem[q.vAns.Length];
                for (int i = 0; i < q.vAns.Length; ++i)
                {
                    AnsItem ai = new AnsItem(q.vAns[i], i, w);
                    ++j;//update view from log
                    if (aAns[j] == Convert.ToByte(true))
                    {
                        ai.mLbl.Content = 'X';
                        ai.mLbxItem.IsSelected = true;
                    }
                    vAnsItem[idx][i] = ai;
                    lbxAns.Items.Add(ai.lbxi);
                }
            }
        }

        public int GetByteCount()
        {
            return 12 + aAns.Length;
        }

        public void ToByte(ref byte[] buf, ref int offs)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(uQSId),
                        0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(mLv),
                        0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(aAns.Length), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(aAns, 0, buf, offs, aAns.Length);
            offs += aAns.Length;
        }

        public bool ReadByte(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            if (l < 4)
                return true;
            uQSId = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (l < 4)
                return true;
            int x;
            if (Enum.IsDefined(typeof(ExamLv), x = BitConverter.ToInt32(buf, offs)))
                eLv = (ExamLv)x;
            offs += 4;
            l -= 4;
            if (l < 4)
                return true;
            int sz = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (l < sz)
                return true;
            aAns = new byte[sz];
            Buffer.BlockCopy(buf, offs, aAns, 0, sz);
            offs += sz;
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

        //only Operation0 uses this.
        public void ExtractKey(QuestSheet qs)
        {
            uQSId = qs.uId + (int)qs.eLv;
            if (qs.vQuest != null && qs.vQuest.First() != null && qs.vQuest.First().vKeys != null)
                aAns = new byte[qs.vQuest.Count * 4];//hardcode, todo
            else
                return;
            int i = -1;
            foreach (Question q in qs.vQuest)
                foreach (bool x in q.vKeys)
                    aAns[++i] = Convert.ToByte(x);
        }

        public void Disable()
        {
            foreach (ListBox lbx in vlbxAns)
                lbx.IsEnabled = false;
        }

        private void Ans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bChanged = true;
            ListBox l = sender as ListBox;
            if (l.SelectedItem == null)
                return;
            int qid = Convert.ToInt32(l.Name.Substring(1));
            int i = -1;
            foreach(ListBoxItem li in l.Items)
            {
                ++i;
                if (li.IsSelected)
                {
                    aAns[qid * 4 + i] = 1;//todo
                    vAnsItem[qid][i].Selected();
                }
                else
                {
                    aAns[qid * 4 + i] = 0;//todo
                    vAnsItem[qid][i].Unselected();
                }
            }
            dgSelChgCB?.Invoke();
        }
    }
}
