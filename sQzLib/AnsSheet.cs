using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace sQzLib
{
    public delegate void DgEvntCB();

    public class AnsItem: StackPanel
    {
        public static CornerRadius sCr;
        static double sW;
        Border mB;

        public Label mLbl;
        public ListBoxItem mLbxItem;

        public static void SInit(double w)
        {
            sCr = new CornerRadius();
            sCr.BottomLeft = sCr.BottomRight = sCr.TopLeft = sCr.TopRight = 50;
            sW = w;
        }

        public AnsItem(string txt, int idx, double w)
        {
            w -= 10;//alignment
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            mB = new Border();
            mB.Width = mB.Height = 30;
            mB.CornerRadius = sCr;
            mB.Background = Theme.s._[(int)BrushId.Q_BG];
            TextBlock tb = new TextBlock();
            tb.Text = "" + (char)('A' + idx);
            tb.Foreground = Theme.s._[(int)BrushId.QID_BG];
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            mB.Child = tb;
            sp.Children.Add(mB);
            TextBlock ansTxt = new TextBlock();
            ansTxt.Text = txt;
            ansTxt.TextWrapping = TextWrapping.Wrap;
            ansTxt.Width = w - mB.Width;
            ansTxt.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(ansTxt);

            mLbxItem = new ListBoxItem();
            mLbxItem.Content = sp;
            mLbxItem.Padding = new Thickness(0);
            mLbxItem.Name = "_" + idx.ToString();

            mLbl = new Label();
        }

        public Label lbl //mLbl is public after all
        {
            get { return mLbl; }
        }

        public ListBoxItem lbxi
        {
            get { return mLbxItem; }
        }

        public void Selected()
        {
            mB.Background = Theme.s._[(int)BrushId.QID_BG];
            TextBlock t = (TextBlock)mB.Child;
            t.Foreground = Theme.s._[(int)BrushId.QID_Color];
            mLbl.Content = 'X';
        }

        public void Unselected()
        {
            mB.Background = Theme.s._[(int)BrushId.Q_BG];
            TextBlock t = (TextBlock)mB.Child;
            t.Foreground = Theme.s._[(int)BrushId.QID_BG];
            mLbl.Content = string.Empty;
        }
    }

    public class AnsSheet
    {
        public ListBox[] vlbxAns;
        public AnsItem[][] vAnsItem;
        public byte[] aAns;
        public ushort uQSId;
        public ExamLvl eLvl;
        public ushort uNeeId;
        public bool bChanged;
        DgEvntCB dgSelChgCB;

        public AnsSheet() {
            bChanged = false;
            aAns = null;
            uQSId = ushort.MaxValue;
            dgSelChgCB = null;
        }

        public void Init(QuestSheet qs, ushort neeId)//todo: only use qs.uId
        {
            uQSId = qs.uId;
            eLvl = qs.eLvl;
            uNeeId = neeId;
            if (aAns == null)
            {
                aAns = new byte[qs.vQuest.Count * 4];//hardcode
                int i = -1;
                foreach (Question q in qs.vQuest)
                {
                    ++i;
                    for (int j = 0; j < q.nAns; ++j)
                        aAns[i * 4 + j] = 0;
                }
            }
        }

        public short Lv
        {
            get { return (short)eLvl; }
        }

        public short Id
        {
            get { return (short)((short)eLvl * uQSId); }
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
            //todo: check length for safety
            Buffer.BlockCopy(BitConverter.GetBytes(uQSId),
                        0, buf, offs, 2);
            offs += 2;
            Buffer.BlockCopy(BitConverter.GetBytes(Lv),
                        0, buf, offs, 2);
            offs += 2;
            Buffer.BlockCopy(BitConverter.GetBytes(uNeeId),
                0, buf, offs, 2);
            offs += 2;
            Buffer.BlockCopy(BitConverter.GetBytes(aAns.Length), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(aAns, 0, buf, offs, aAns.Length);
            offs += aAns.Length;
        }

        public bool ReadByte(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            if (l < 2)
                return true;
            uQSId = BitConverter.ToUInt16(buf, offs);
            offs += 2;
            l -= 2;
            if (l < 2)
                return true;
            eLvl = (ExamLvl)BitConverter.ToInt16(buf, offs);
            offs += 2;
            l -= 2;
            if (l < 2)
                return true;
            uNeeId = BitConverter.ToUInt16(buf, offs);
            offs += 2;
            l -= 2;
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

        public ushort Grade(byte[] ans)
        {
            if (ans == null)
                return 101;
            if (aAns == null)
                return 102;
            if (ans.Length != aAns.Length)
                return 103;
            ushort grade = 0;
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
            uQSId = qs.uId;
            if (qs.vQuest != null && qs.vQuest.First() != null && qs.vQuest.First().vKeys != null)
                aAns = new byte[qs.vQuest.Count * 4];//hardcode
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
