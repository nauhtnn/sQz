﻿using Microsoft.Win32;
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
using MySql.Data.MySqlClient;
using sQzLib;

namespace sQzServer0
{
    /// <summary>
    /// Interaction logic for Prep0.xaml
    /// </summary>
    public partial class Prep0 : Page
    {
        List<CheckBox> vChk;
        IUx mSelQCat;
        QuestSheet mDBQS;
        QuestSheet mTmpQS;
        ExamBoard mBrd;

        public Prep0()
        {
            InitializeComponent();
            mSelQCat = IUx._0;
            mDBQS = new QuestSheet();
            mTmpQS = new QuestSheet();
            mBrd = new ExamBoard();
            vChk = new List<CheckBox>();
        }

        private void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("MainMenu.xaml", UriKind.Relative));
        }

        private void InsertBoard(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            if (DT.To_(tbxBrd.Text, DT._, out dt))
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.BOARD_NOK]);
                spMain.Opacity = 1;
            }
            else
            {
                ExamBoard eb = new ExamBoard();
                eb.mDt = dt;
                string msg;
                if(0 < eb.DBIns(out msg))
                {
                    spMain.Opacity = 0.5;
                    WPopup.s.ShowDialog(Txt.s._[(int)TxI.BOARD_OK]);
                    spMain.Opacity = 1;
                    LoadBrd();
                    tbxBrd.Text = string.Empty;
                }
                else
                {
                    spMain.Opacity = 0.5;
                    WPopup.s.ShowDialog(msg);
                    spMain.Opacity = 1;
                }
            }
        }

        private void InsertSlot(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            string t = tbxSl.Text;
            if (DT.To_(t, DT.h, out dt))
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.SLOT_NOK]);
                spMain.Opacity = 1;
            }
            else
            {
                string msg;
                if(0 < mBrd.DBInsSl(dt, out msg))
                {
                    spMain.Opacity = 0.5;
                    WPopup.s.ShowDialog(Txt.s._[(int)TxI.SLOT_OK]);
                    spMain.Opacity = 1;
                    LoadSl();
                    tbxSl.Text = string.Empty;
                }
                else
                {
                    spMain.Opacity = 0.5;
                    WPopup.s.ShowDialog(msg);
                    spMain.Opacity = 1;
                }
            }
        }

        private void LoadBrd()
        {
            string emsg;
            List<DateTime> v = ExamBoard.DBSel(out emsg);
            if(v == null)
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(emsg);
                spMain.Opacity = 1;
                return;
            }
            lbxBrd.Items.Clear();
            foreach(DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DT.__);
                lbxBrd.Items.Add(it);
            }
        }

        private void LoadSl()
        {
            string emsg;
            List<DateTime> v = mBrd.DBSelSl(false, out emsg);
            if(v == null)
            {
                spMain.Opacity = 0.5;
                WPopup.s.ShowDialog(emsg);
                spMain.Opacity = 1;
                return;
            }
            //bool dark = true;
            //Color c = new Color();
            //c.A = 0xff;
            //c.B = c.G = c.R = 0xf0;
            lbxSl.Items.Clear();
            foreach (DateTime dt in v)
            {
                ListBoxItem it = new ListBoxItem();
                it.Content = dt.ToString(DT.hh);
                it.Selected += lbxSl_Selected;
                it.Unselected += lbxSl_Unselected;
                //dark = !dark;
                //if (dark)
                //    it.Background = new SolidColorBrush(c);
                lbxSl.Items.Add(it);
            }
        }

        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        { }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.FontSize = 16;

            LoadTxt();

            InitLbxQCatgry();
            LoadBrd();
            Window w = Window.GetWindow(this);
            if(w != null)
                w.Closing += W_Closing;
        }

        void InitLbxQCatgry()
        {
            List<string> qCatName = new List<string>();
            for (int i = (int)TxI.IU01; i <= (int)TxI.IU15; ++i)
                qCatName.Add(Txt.s._[i]);
            bool dark = true;
            Color c = new Color();
            c.A = 0xff;
            c.B = c.G = c.R = 0xf0;
            Brush b = new SolidColorBrush(c);
            Dispatcher.Invoke(() => {
                lbxQCatgry.Items.Clear();
                foreach (string i in qCatName)
                {
                    ListBoxItem it = new ListBoxItem();
                    it.Content = i;
                    dark = !dark;
                    if (dark)
                        it.Background = b;
                    lbxQCatgry.Items.Add(i);
                }
            });
        }

        private void SelectBoard(object sender, SelectionChangedEventArgs e)
        {
            tbcNee.Items.Clear();
            ListBox l = sender as ListBox;
            ListBoxItem i = l.SelectedItem as ListBoxItem;
            if (i == null)
            {
                lbxSl.IsEnabled = false;
                return;
            }
            DateTime dt;
            if(!DT.To_(i.Content as string, DT._, out dt))
            {
                mBrd.mDt = dt;
                lbxSl.IsEnabled = true;
                LoadSl();
            }
        }

        private void OpenRawFile_of_MCItems(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            // set filter for file extension and default file extension 
            dlg.DefaultExt = ".docx";
            dlg.Filter = "text documents (.docx;.txt)|*.docx;*.txt";
            bool? result = dlg.ShowDialog();

            string fpath = null;
            if (result == true)
                fpath = dlg.FileName;
            else
                return;
            if (fpath.EndsWith(".docx"))
                mTmpQS.ReadDocx(fpath);
            else
                mTmpQS.ReadTxt(fpath);
            ShowTmpQ();
        }

        private void ShowDBQ()
        {
            SolidColorBrush evenbg = Theme.s._[(int)BrushId.BG];
            SolidColorBrush oddbg = Theme.s._[(int)BrushId.Q_BG];
            SolidColorBrush difbg = Theme.s._[(int)BrushId.Ans_TopLine];
            SolidColorBrush bg;
            bool even = false;
            int x = -1;
            gDBQuest.Children.Clear();
            gDBQuest.RowDefinitions.Clear();
            vChk.Clear();
            double w = gDBQuest.ColumnDefinitions.First().Width.Value;
            foreach (Question q in mDBQS.ShallowCopy())
            {
                TextBlock i = new TextBlock();
                i.Text = (++x + 1) + ". " + q.Stmt;
                i.Width = w;
                i.TextWrapping = TextWrapping.Wrap;
                StackPanel sp = new StackPanel();
                sp.Children.Add(i);
                for (int idx = 0; idx < Question.N_ANS; ++idx)
                {
                    TextBlock j = new TextBlock();
                    j.Text = ((char)('A' + idx)).ToString() + ") " + q.vAns[idx];
                    j.Width = w;
                    j.TextWrapping = TextWrapping.Wrap;
                    if (q.vKeys[idx])
                        j.FontWeight = FontWeights.Bold;
                    sp.Children.Add(j);
                }
                /*if (q.bDiff)
                    bg = difbg;
                else*/ if (even)
                    bg = evenbg;
                else
                    bg = oddbg;
                even = !even;
                sp.Background = bg;
                RowDefinition rd = new RowDefinition();
                gDBQuest.RowDefinitions.Add(rd);
                Grid.SetRow(sp, x);
                gDBQuest.Children.Add(sp);
                CheckBox chk = new CheckBox();
                chk.Name = "c" + q.uId;
                chk.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(chk, 1);
                Grid.SetRow(chk, x);
                gDBQuest.Children.Add(chk);
                vChk.Add(chk);
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._[(int)TxI.Q_DB], mDBQS.Count, QuestSheet.DBGetND(mSelQCat));
            tbiDBQ.Header = sb.ToString();
        }

        private void ShowTmpQ()
        {
            SolidColorBrush evenbg = Theme.s._[(int)BrushId.BG];
            SolidColorBrush oddbg = Theme.s._[(int)BrushId.Q_BG];
            SolidColorBrush difbg = Theme.s._[(int)BrushId.Ans_TopLine];
            SolidColorBrush bg;
            bool even = false;
            int x = -1;
            double w = svwrTmpQ.Width;
            svwrTmpQ.Content = null;
            StackPanel sp = new StackPanel();
            foreach (Question q in mTmpQS.ShallowCopy())
            {
                TextBlock i = new TextBlock();
                i.Text = (++x + 1) + ". " + q.Stmt;
                i.Width = w;
                i.TextWrapping = TextWrapping.Wrap;
                /*if (q.bDiff)
                    bg = difbg;
                else*/ if (even)
                    bg = evenbg;
                else
                    bg = oddbg;
                even = !even;
                i.Background = bg;
                sp.Children.Add(i);
                for (int idx = 0; idx < Question.N_ANS; ++idx)
                {
                    TextBlock j = new TextBlock();
                    j.Text = ((char)('A' + idx)).ToString() + ") " + q.vAns[idx];
                    j.Width = w;
                    j.TextWrapping = TextWrapping.Wrap;
                    if (q.vKeys[idx])
                        j.FontWeight = FontWeights.Bold;
                    j.Background = bg;
                    sp.Children.Add(j);
                }
            }
            svwrTmpQ.Content = sp;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._[(int)TxI.Q_TMP], mTmpQS.Count, mTmpQS.CountD);
            tbiTmpQ.Header = sb.ToString();
        }

        private void InsertMCItems(object sender, RoutedEventArgs e)
        {
            if (mSelQCat == IUx._0)
            {
                WPopup.s.ShowDialog(Txt.s._[(int)TxI.PREP_IU15]);
                return;
            }
            if (mTmpQS.Count == 0)
                return;
            gDBQuest.Children.Clear();
            svwrTmpQ.Content = null;
            mTmpQS.DBIns(mSelQCat);
            mTmpQS.Clear();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._[(int)TxI.Q_TMP], 0, mTmpQS.CountD);
            tbiTmpQ.Header = sb.ToString();
            mDBQS.DBSelect(mSelQCat, QuestDiff.Both);
            ShowDBQ();
        }

        private void SelectIUx(object sender, SelectionChangedEventArgs e)
        {
            ListBox l = (ListBox)sender;
            if (Enum.IsDefined(typeof(IUx), l.SelectedIndex))
            {
                mSelQCat = (IUx)l.SelectedIndex;
                mDBQS.DBSelect(mSelQCat, QuestDiff.Both);
                ShowDBQ();
            }
        }

        private void LoadTxt()
        {
            Txt t = Txt.s;
            btnMMenu.Content = t._[(int)TxI.BACK_MMENU];
            txtDt.Text = t._[(int)TxI.DATE_L];
            txtHm.Text = t._[(int)TxI.TIME_L];
            txtIU.Text = t._[(int)TxI.IUS];
            tbi1.Header = t._[(int)TxI.PREP_NEE];
            tbi2.Header = t._[(int)TxI.PREP_Q];
            txtId.Text = t._[(int)TxI.NEEID_S];
            txtName.Text = t._[(int)TxI.NEE_NAME];
            txtBirdate.Text = t._[(int)TxI.BIRDATE];
            txtBirpl.Text = t._[(int)TxI.BIRPL];
            txtRoom.Text = t._[(int)TxI.ROOM];
            btnImp.Content = t._[(int)TxI.PREP_IMP];
            btnDelQ.Content = t._[(int)TxI.PREP_DEL_SEL];
            btnImpQ.Content = t._[(int)TxI.PREP_IMP];
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Txt.s._[(int)TxI.Q_DB], 0, QuestSheet.DBGetND(mSelQCat));
            tbiDBQ.Header = sb.ToString();
            sb.Clear();
            sb.AppendFormat(Txt.s._[(int)TxI.Q_TMP], 0, mTmpQS.CountD);
            tbiTmpQ.Header = sb.ToString();
        }

        private void DeleteSelectedMCItems(object sender, RoutedEventArgs e)
        {
            StringBuilder qids = new StringBuilder();
            foreach(CheckBox c in vChk)
                if(c.IsChecked == true)
                {
                    int uqid;
                    if (int.TryParse(c.Name.Substring(1), out uqid))
                        qids.Append("id=" + uqid + " OR ");
                }
            bool toUpdate = false;
            if (0 < qids.Length)
            {
                qids.Remove(qids.Length - 4, 4);//remove the last " OR "
                Question.DBDelete(mSelQCat, qids.ToString());
                toUpdate = true;
            }
            if (toUpdate)
            {
                mDBQS.DBSelect(mSelQCat, QuestDiff.Both);
                ShowDBQ();
            }
            chkAll.IsChecked = false;
        }

        private void lbxSl_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            ExamSlot sl = new ExamSlot();
            DateTime dt;
            DT.To_(mBrd.mDt.ToString(DT._) + ' ' + i.Content as string, DT.H, out dt);
            sl.Dt = dt;
            sl.DBSelRoomId();
            sl.DBSelStt();
            sl.DBSelNee();
            PrepNeeView pnv = new PrepNeeView(sl);
            pnv.DeepCopy(refSl);
            pnv.Show(true);
            tbcNee.Items.Add(pnv);
            pnv.Focus();
        }

        private void lbxSl_Unselected(object sender, RoutedEventArgs e)
        {
            ListBoxItem i = sender as ListBoxItem;
            if (i == null)
                return;
            foreach(TabItem ti in tbcNee.Items)
                if(ti.Name == "_" + (i.Content as string).Replace(':', '_'))
                {
                    tbcNee.Items.Remove(ti);
                    break;
                }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox c in vChk)
                c.IsChecked = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox c in vChk)
                c.IsChecked = false;
        }
    }
}
