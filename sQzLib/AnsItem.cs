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

    public class AnsItem //todo: merge with OptionView
    {
        public Label mLbl;
        public OptionView view;

        public AnsItem(string txt, int idx, double w)
        {
            w -= 10;//alignment
            view = new OptionView(txt, idx, w);
            view.Name = "_" + idx.ToString();

            mLbl = new Label();
        }

        public Label lbl //mLbl is public after all
        {
            get { return mLbl; }
        }

        public ListBoxItem lbxi
        {
            get { return view; }
        }

        public void Selected()
        {
            mLbl.Content = 'X';
        }

        public void Unselected()
        {
            mLbl.Content = string.Empty;
        }
    }
}
