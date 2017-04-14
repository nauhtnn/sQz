using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace sQzLib
{
    public class AnsItem: StackPanel
    {
        public static CornerRadius sCr;
        static double sW;
        Border mB;

        Label mLbl;
        ListBoxItem mLbxItem;

        public static void SInit(double w)
        {
            sCr = new CornerRadius();
            sCr.BottomLeft = sCr.BottomRight = sCr.TopLeft = sCr.TopRight = 50;
            sW = w;
        }

        public AnsItem(string txt, int idx)
        {
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
            ansTxt.Width = 470;// TakeExam.qaWh - mB.Width;//hardcode
            ansTxt.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(ansTxt);

            mLbxItem = new ListBoxItem();
            mLbxItem.Content = sp;
            mLbxItem.Name = "_" + idx.ToString();

            mLbl = new Label();
        }

        public Label lbl
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
        public uint mId;
        public ExamLvl eLvl;
        public ushort mNeeId;

        public AnsSheet() {}

        public void Init(QuestSheet qs, ushort neeId)
        {
            mId = qs.mId;
            eLvl = qs.eLvl;
            mNeeId = neeId;
            aAns = new byte[qs.vQuest.Count * 4];//hardcode
            int i = -1;
            foreach (Question q in qs.vQuest)
            {
                ++i;
                if (q.vKeys != null){
                    int j = -1;
                    foreach (bool x in q.vKeys)
                        aAns[i * 4 + ++j] = Convert.ToByte(x);
                }
                else
                    for(int j = 0; j < q.nAns; ++j)
                        aAns[i * 4 + j] = 0;
            }
        }

        public void InitView(QuestSheet qs, double w)
        {
            vlbxAns = new ListBox[qs.vQuest.Count];
            vAnsItem = new AnsItem[qs.vQuest.Count][];
            
            int idx = -1;
            int aidx = -1;
            foreach (Question q in qs.vQuest)
            {
                ++idx;
                ListBox lbxAns = new ListBox();
                lbxAns.Width = w;// qaWh;
                lbxAns.Name = "_" + idx;
                lbxAns.SelectionChanged += Ans_SelectionChanged;
                lbxAns.BorderBrush = Theme.s._[(int)BrushId.Ans_TopLine];
                lbxAns.BorderThickness = new Thickness(0, 4, 0, 0);
                vlbxAns[idx] = lbxAns;
                ++aidx;
                vAnsItem[aidx] = new AnsItem[q.vAns.Length];
                for (int i = 0; i < q.vAns.Length; ++i)
                {
                    AnsItem ai = new AnsItem(q.vAns[i], i);
                    vAnsItem[aidx][i] = ai;
                    lbxAns.Items.Add(ai.lbxi);
                }
            }
        }

        public int GetByteCount()
        {
            return 14 + aAns.Length;
        }

        public void ToByte(ref byte[] buf, ref int offs)
        {
            //todo: check length for safety
            Buffer.BlockCopy(BitConverter.GetBytes(mId),
                        0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BitConverter.GetBytes((int)eLvl),
                        0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(BitConverter.GetBytes(mNeeId),
                0, buf, offs, 2);
            offs += 2;
            Buffer.BlockCopy(BitConverter.GetBytes(aAns.Length), 0, buf, offs, 4);
            offs += 4;
            Buffer.BlockCopy(aAns, 0, buf, offs, aAns.Length);
            offs += aAns.Length;
        }

        public void ReadByte(byte[] buf, ref int offs)
        {
            int l = buf.Length - offs;
            if (l < 4)
                return;
            mId = BitConverter.ToUInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (l < 4)
                return;
            eLvl = (ExamLvl)BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (l < 2)
                return;
            mNeeId = BitConverter.ToUInt16(buf, offs);
            offs += 2;
            l -= 2;
            if (l < 4)
                return;
            int sz = BitConverter.ToInt32(buf, offs);
            offs += 4;
            l -= 4;
            if (l < sz)
                return;
            aAns = new byte[sz];
            Buffer.BlockCopy(buf, offs, aAns, 0, sz);
            offs += sz;
        }

        public ushort Mark(byte[] ans)
        {
            if (ans == null)
                return 101;
            if (aAns == null)
                return 102;
            if (ans.Length != aAns.Length)
                return 103;
            ushort mark = 0;
            int offs = 0;
            while(offs < aAns.Length)
            {
                int offs2 = offs + 4;
                for(; offs < offs2; ++offs)
                    if (ans[offs] != aAns[offs])
                        break;
                if (offs == offs2)
                    ++mark;
                offs = offs2;
            }
            return mark;
        }

        //only Operation0 uses this.
        public void ExtractKey(QuestSheet qs)
        {
            mId = qs.mId;
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
            ListBox l = sender as ListBox;
            if (l.SelectedItem == null)
                return;
            int qid = Convert.ToInt32(l.Name.Substring(1));
            //for (int i = 0, n = l.Items.Count; i < n; ++i)
            int i = -1;
            foreach(ListBoxItem li in l.Items)
            {
                //ListBoxItem li = (ListBoxItem)l.Items[i];
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
        }
    }
}
