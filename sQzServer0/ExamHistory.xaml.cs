using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using sQzLib;

namespace sQzServer0
{
    /// <summary>
    /// Interaction logic for ExamHistory.xaml
    /// </summary>
    public partial class ExamHistory : Page
    {
        ExamDate mDt;
        QuestPack mQPack;
        QuestSheet mQSh;
        ExamRoom mRoom;
        Dictionary<int, string> vAns;

        public ExamHistory()
        {
            InitializeComponent();
            mDt = new ExamDate();
            mQPack = new QuestPack();
            mRoom = new ExamRoom();
        }

        private void LoadDate()
        {
            Dictionary<uint, DateTime> v = mDt.DBSelect();
            if (0 < v.Keys.Count)
            {
                bool dark = true;
                Color c = new Color();
                c.A = 0xff;
                c.B = c.G = c.R = 0xf0;
                Dispatcher.Invoke(() => {
                    lbxDate.Items.Clear();
                    foreach (uint i in v.Keys)
                    {
                        ListBoxItem it = new ListBoxItem();
                        it.Name = "_" + i;
                        it.Content = v[i].ToString(ExamDate.DtFmt.H);
                        dark = !dark;
                        if (dark)
                            it.Background = new SolidColorBrush(c);
                        lbxDate.Items.Add(it);
                    }
                });
            }
        }

        private void lbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbxExam.Items.Clear();
            lbxNee.Items.Clear();
            spQSh.Children.Clear();
            ListBox l = (ListBox)sender;
            ListBoxItem i = (ListBoxItem)l.SelectedItem;
            if (i == null)
                return;
            if (uint.TryParse(i.Name.Substring(1), out mDt.uId))
            {
                List<int> v = mQPack.DBSelectId(mDt.uId);
                foreach(int j in v)
                {
                    ListBoxItem it = new ListBoxItem();
                    if(j < 0)
                    {
                        it.Content = "CB " + (-j);
                        it.Name = "p_" + (-j);
                    }
                    else
                    {
                        it.Content = "NC " + j;
                        it.Name = "p" + j;
                    }
                    
                    lbxExam.Items.Add(it);
                }
                Dictionary<int, ushort> vQIdx = mRoom.DBSelectId(mDt.uId, out vAns);
                foreach (int j in vQIdx.Keys)
                {
                    ListBoxItem it = new ListBoxItem();
                    if (j < 0)
                    {
                        it.Content = "CB " + (-j);
                        it.Name = "r_" + (-j);
                    }
                    else
                    {
                        it.Content = "NC " + j;
                        it.Name = "r" + j;
                    }

                    lbxNee.Items.Add(it);
                }
            }
        }

        private void lbxExam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            spQSh.Children.Clear();
            ListBox l = (ListBox)sender;
            ListBoxItem i = (ListBoxItem)l.SelectedItem;
            if (i == null)
                return;
            ushort id;
            short lv;
            if (i.Name[1] == '_')
            {
                lv = -1;
                if (!ushort.TryParse(i.Name.Substring(2), out id))
                    return;
            }
            else
            {
                lv = 1;
                if (!ushort.TryParse(i.Name.Substring(1), out id))
                    return;
            }
            mQSh = new QuestSheet();
            mQSh.DBSelect(mDt.uId, lv, id);

            int x = 0;
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            foreach (Question q in mQSh.vQuest)
            {
                TextBlock tbx = new TextBlock();
                tbx.Text = ++x + ") " + q.ToString();
                dark = !dark;
                if (dark)
                    tbx.Background = new SolidColorBrush(c);
                else
                    tbx.Background = Theme.s._[(int)BrushId.LeftPanel_BG];
                spQSh.Children.Add(tbx);
            }
        }

        private void lbxNee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            spQSh.Children.Clear();
            ListBox l = (ListBox)sender;
            ListBoxItem i = (ListBoxItem)l.SelectedItem;
            if (i == null)
                return;
            ushort id;
            short lv;
            if (i.Name[1] == '_')
            {
                lv = -1;
                if (!ushort.TryParse(i.Name.Substring(2), out id))
                    return;
            }
            else
            {
                lv = 1;
                if (!ushort.TryParse(i.Name.Substring(1), out id))
                    return;
            }
            mQSh = new QuestSheet();
            mQSh.DBSelect(mDt.uId, lv, id);

            string ans = null;
            vAns.TryGetValue(lv*id, out ans);

            int x = -1;
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            int k = 0;
            foreach (Question q in mQSh.vQuest)
            {
                TextBlock tbx = new TextBlock();
                tbx.Text = ++x + ") " + q.ToString() + "\nChoose ";
                for (int o = k, k4 = k + 4; k < k4; ++k)
                    if (ans[k] == '1')
                        tbx.Text += (char)('A' + k - o);
                dark = !dark;
                if (dark)
                    tbx.Background = new SolidColorBrush(c);
                else
                    tbx.Background = Theme.s._[(int)BrushId.LeftPanel_BG];
                spQSh.Children.Add(tbx);
            }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDate();
        }
    }
}
