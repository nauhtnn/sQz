using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Collections.Generic;
using sQzLib;

namespace sQzClient
{
    public partial class TakeExam : Page
    {
        public void Ans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //bChanged = true;
            ListBox l = sender as ListBox;
            if (l.SelectedItem == null)
                return;
            int qid = Convert.ToInt32(l.Name.Substring(1));
            int i = -1;
            foreach (ListBoxItem li in l.Items)
            {
                ++i;
                if (li.IsSelected)
                {
                    //aAns[qid * 4 + i] = 1;//todo
                    //vAnsItem[qid][i].Selected();
                }
                else
                {
                    //aAns[qid * 4 + i] = 0;//todo
                    //vAnsItem[qid][i].Unselected();
                }
            }
            //dgSelChgCB?.Invoke();
        }
    }
}
