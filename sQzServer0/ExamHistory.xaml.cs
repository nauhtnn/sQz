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

        public ExamHistory()
        {
            InitializeComponent();
            mDt = new ExamDate();
            mQPack = new QuestPack();
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
                        it.Content = v[i].ToString("dd/MM/yyyy HH:mm");
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
            spQSh.Children.Clear();
            ListBox l = (ListBox)sender;
            ListBoxItem i = (ListBoxItem)l.SelectedItem;
            if (i == null)
                return;
            if (uint.TryParse(i.Name.Substring(1), out mDt.uId))
            {
                List<int> v = mQPack.DBSelect(mDt.uId);
                foreach(int j in v)
                {
                    ListBoxItem it = new ListBoxItem();
                    if(j < 0)
                    {
                        it.Content = "CB " + (-j);
                        it.Name = "x_" + (-j);
                    }
                    else
                    {
                        it.Content = "NC " + j;
                        it.Content = "NC " + j;
                    }
                    
                    lbxExam.Items.Add(it);
                }
            }
            else
                lbxExam.Items.Clear();
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

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDate();
        }
    }
}
