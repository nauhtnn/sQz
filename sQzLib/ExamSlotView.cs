using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace sQzLib
{
    public class ExamSlotView
    {
        public Dictionary<int, TextBlock> vGrade;
        public Dictionary<int, TextBlock> vDt1;
        public Dictionary<int, TextBlock> vDt2;
        public Dictionary<int, TextBlock> vComp;
        Grid grdNee;
        public TabItem tbiNee;

        public ExamSlotView()
        {
            vGrade = new Dictionary<int, TextBlock>();
            vDt1 = new Dictionary<int, TextBlock>();
            vDt2 = new Dictionary<int, TextBlock>();
            vComp = new Dictionary<int, TextBlock>();
        }

        public TabItem TabItemExaminee(List<ExamRoom> vRoom, Grid refGrid, string hdr)
        {
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            bool dark = false;
            int rid = -1;
            grdNee = new Grid();
            foreach(ColumnDefinition cd in refGrid.ColumnDefinitions)
            {
                ColumnDefinition d = new ColumnDefinition();
                d.Width = cd.Width;
                grdNee.ColumnDefinitions.Add(d);
            }
            foreach (ExamRoom r in vRoom)
                foreach (ExamineeA e in r.vExaminee.Values)
                {
                    rid++;
                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(20);
                    grdNee.RowDefinitions.Add(rd);
                    TextBlock t = new TextBlock();
                    t.Text = e.tId;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tName;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 1);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirdate;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 2);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    t.Text = e.tBirthplace;
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 3);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vGrade.Add(e.mLv + e.uId, t);
                    if (e.uGrade != ushort.MaxValue)
                        t.Text = e.uGrade.ToString();
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 4);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vDt1.Add(e.mLv + e.uId, t);
                    if (e.dtTim1.Year != ExamSlot.INVALID)
                        t.Text = e.dtTim1.ToString("HH:mm");
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 5);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vDt2.Add(e.mLv + e.uId, t);
                    if (e.dtTim2.Year != ExamSlot.INVALID)
                        t.Text = e.dtTim2.ToString("HH:mm");
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 6);
                    grdNee.Children.Add(t);
                    t = new TextBlock();
                    if (dark)
                        t.Background = new SolidColorBrush(c);
                    vComp.Add(e.mLv + e.uId, t);
                    if (e.tComp != null)
                        t.Text = e.tComp;
                    Grid.SetRow(t, rid);
                    Grid.SetColumn(t, 7);
                    grdNee.Children.Add(t);
                    dark = !dark;
                }
            tbiNee = new TabItem();
            tbiNee.Header = hdr;
            tbiNee.Content = grdNee;
            return tbiNee;
        }

        public void UpdateRsView(List<ExamRoom> vRoom)
        {
            TextBlock t;
            foreach (ExamRoom r in vRoom)
                foreach (ExamineeA e in r.vExaminee.Values)
                {
                    if (e.uGrade != ushort.MaxValue && vGrade.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.uGrade.ToString();
                    if (e.dtTim1.Hour != ExamSlot.INVALID && vDt1.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.dtTim1.ToString("HH:mm");
                    if (e.dtTim2.Hour != ExamSlot.INVALID && vDt2.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.dtTim2.ToString("HH:mm");
                    if (e.tComp != null && vComp.TryGetValue(e.mLv + e.uId, out t))
                        t.Text = e.tComp;
                }
        }
    }
}
