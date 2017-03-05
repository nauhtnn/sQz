using System;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
//using sQzCS;
using System.Collections.Generic;

namespace WpfApplication1
{
    //from left to right
    enum BrushId {
		BG = 0,
		LeftPanel_BG,
		Sheet_BG,
		Button_Hover,
		Q_BG,
		QID_BG,
		QID_Color,
		Ans_TopLine,
		Ans_Highlight,
		Count
	}
	
	enum ThicknessId {
		LT = 0,
		MT,
		RT,
		LB,
		MB,
		RB,
		Count
	}
	
	enum ThemeId {
		Harvard = 0,
		Berkeley,
		Count
	}
	
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		double em = 16 * 1.2;
        Grid grdAnswerSheet;
        Label[][] AnswerSheet;
        double[] vWidth;
		
		Label dmsg = new Label();
		List<Question> vQuest;
		
		public MainWindow()
		{
			InitializeComponent();
			vQuest = new List<Question>();
            InitBrush();
            InitThickness();
            vFontFml = new FontFamily[2];
            vFontFml[0] = new FontFamily("Arial");
            vFontFml[1] = new FontFamily("Arial");
        }

        void InitQuestion()
        {
            string fn = "qz1.txt";
			if(!System.IO.File.Exists(fn))
			{
				dmsg.Content += "input file doesn't exist";
				return;
			}
            string buf = Utils.ReadFile(fn);
			if(buf != null)
				dmsg.Content += "buf size = " + buf.Length + '\n';
			else
				return;
            //sQzCS.Page pg = new sQzCS.Page();
            //Question.StartRead(Utils.Split(buf, '\n'), pg.mSt);
            Question.StartRead(Utils.Split(buf, '\n'), null);
            Question q = new Question();
            while (q.Read())
            {
                vQuest.Add(q);
                q = new Question();
            }
			q = null;
			// fn = "qz1.html";
			// System.IO.StreamWriter sw = new System.IO.StreamWriter(fn);
			// if (sw == null)
			// {
				// dmsg.Content += "Cannot write file " + fn;
				// return;
			// }
			// else
				// dmsg.Content += "Write file " + fn + '\n';
			// int e = vQuest.Count;
			// pg.WriteHeader(sw);
			// pg.WriteFormHeader(sw, e);
			// int MAX_COLUMN = 2;
			// int j = 0, column = MAX_COLUMN;
			// Random r = new Random();
			// while (0 < vQuest.Count)
			// {
				// if (column == MAX_COLUMN)
				// {
					// sw.Write("<div class='cl1'></div>");
					// column = 0;
				// }
				// int i = 0;
				// if (pg.mSt.bQuestSort)
					// i = r.Next(vQuest.Count - 1);
				// vQuest[i].write(sw, ++j, ref column);
				// vQuest.RemoveAt(i);
			// }
			// pg.WriteFormFooter(sw);
			// pg.WriteFooter(sw);
			// sw.Close();
        }
		
		SolidColorBrush[] vBrush;
		SolidColorBrush[][] vTheme;
		Thickness[] vThickness;
        FontFamily[] vFontFml;
		
		void InitBrush() {
			vTheme = new SolidColorBrush[(int)ThemeId.Count][];
            //Harvard theme
            SolidColorBrush[] br = new SolidColorBrush[(int)BrushId.Count];
			Color c = new Color();
			c.A = 0xff;
			c.R = 0xc3;
			c.G = 0xd7;
			c.B = 0xa4;
			br[(int)BrushId.BG] = new SolidColorBrush(c);
			c.R = 0xa5;
			c.G = 0x1c;
			c.B = 0x30;
			br[(int)BrushId.QID_BG] = new SolidColorBrush(c);
			c.R = c.G = c.B = 0xff;
			br[(int)BrushId.QID_Color] = new SolidColorBrush(c);
			c.R = c.G = c.B = 0xee;
			br[(int)BrushId.Q_BG] = new SolidColorBrush(c);
			c.R = 0xd8;
			c.G = 0x70;
			c.B = 0xb8;
			br[(int)BrushId.Ans_TopLine] = new SolidColorBrush(c);
			c.R = 0x58;
			c.G = 0xa9;
			c.B = 0xb4;
			br[(int)BrushId.Ans_Highlight] = new SolidColorBrush(c);
			c.R = c.G = 0xff;
			c.B = 0xbb;
			br[(int)BrushId.LeftPanel_BG] = new SolidColorBrush(c);
			c.B = 0xdd;
			br[(int)BrushId.Sheet_BG] = new SolidColorBrush(c);
			c.R = 0xf1;
			c.G = 0x5a;
			c.B = 0x23;
			br[(int)BrushId.Button_Hover] = new SolidColorBrush(c);
			vTheme[(int)ThemeId.Harvard] = br;
			vBrush = vTheme[(int)ThemeId.Harvard];
		}
		
		void InitThickness() {
			vThickness = new Thickness[(int)ThicknessId.Count];
			vThickness[(int)ThicknessId.LT] = new Thickness(0, 0, 0, 0);
			vThickness[(int)ThicknessId.MT] = new Thickness(1, 1, 0, 0);
			vThickness[(int)ThicknessId.RT] = new Thickness(1, 1, 1, 0);
			vThickness[(int)ThicknessId.LB] = new Thickness(1, 1, 0, 1);
			vThickness[(int)ThicknessId.MB] = new Thickness(1, 1, 0, 1);
			vThickness[(int)ThicknessId.RB] = new Thickness(1, 1, 1, 1);
        }

        void InitLayout(object sender, RoutedEventArgs e)
        {
            vWidth = new double[5];
            vWidth[0] = gMain.RenderSize.Width;
            vWidth[1] = 2 * em;
            vWidth[2] = 5 * vWidth[1];
            vWidth[3] = 8;
            vWidth[4] = (vWidth[0] - vWidth[2]) / 2 - SystemParameters.ScrollWidth - 2 * vWidth[3] - vWidth[1];
            gMain.ColumnDefinitions.Add(new ColumnDefinition());
			gMain.ColumnDefinitions.Add(new ColumnDefinition());
            InitQuestion();
            InitLeftPanel();
            InitQuestPanel();
			dmsg.Background = vBrush[(int)BrushId.LeftPanel_BG];
			dmsg.Width = (int)gMain.RenderSize.Width / 2;
			dmsg.Height = (int)gMain.RenderSize.Height / 4;
			// gMain.Children.Add(dmsg);
        }

		void InitLeftPanel()
		{
			//left panel
			StackPanel lp = new StackPanel();
			lp.HorizontalAlignment = HorizontalAlignment.Left;
			lp.Background = vBrush[(int)BrushId.LeftPanel_BG];
			//title
			Label l = new Label();
			l.Content = "Answer Sheet";
			l.FontFamily = vFontFml[0];
            l.FontWeight = FontWeights.Bold;
			l.FontSize = em;
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            lp.Children.Add(l);
			//answer sheet
			grdAnswerSheet = new Grid();
			grdAnswerSheet.Background = vBrush[(int)BrushId.Sheet_BG];
			int nAns = 4;//hardcode
            ++nAns;
            int i = 0;
            for (i = 0; i < nAns; ++i)
                grdAnswerSheet.ColumnDefinitions.Add(new ColumnDefinition());
            for (i = 0; i < nAns; ++i)
                grdAnswerSheet.ColumnDefinitions[i].Width = new GridLength(vWidth[1]);
            --nAns;
            AnswerSheet = new Label[vQuest.Count + 10][];
            //top line
            grdAnswerSheet.RowDefinitions.Add(new RowDefinition());
            l = new Label();
            l.Height = vWidth[1];
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, 0);
            grdAnswerSheet.Children.Add(l);
            SolidColorBrush brBK = new SolidColorBrush(Colors.Black);
			for (i = 1; i < nAns; ++i)
			{
				l = new Label();
                l.Content = (char)('@' + i);
                l.BorderBrush = brBK;
				l.BorderThickness = vThickness[(int)ThicknessId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.FontWeight = FontWeights.Bold;
                Grid.SetRow(l, 0);
				Grid.SetColumn(l, i);
				grdAnswerSheet.Children.Add(l);

            }
			l = new Label();
            l.BorderBrush = brBK;
			l.BorderThickness = vThickness[(int)ThicknessId.RT];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.Content = (char)('@' + i);
            //l.FontFamily = vFontFml[1];
            l.FontWeight = FontWeights.Bold;
            Grid.SetRow(l, 0);
			Grid.SetColumn(l, i);
			grdAnswerSheet.Children.Add(l);
			//next lines
            int j = 0;
            for (j = 1, i = 0; j < vQuest.Count; ++j)
            {
                grdAnswerSheet.RowDefinitions.Add(new RowDefinition());
                AnswerSheet[j - 1] = new Label[nAns];
                l = new Label();
                l.Content = j;
                l.BorderBrush = brBK;
                l.BorderThickness = vThickness[(int)ThicknessId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.FontFamily = vFontFml[1];
                l.FontWeight = FontWeights.Bold;
                l.Height = vWidth[1];
                Grid.SetRow(l, j);
                Grid.SetColumn(l, 0);
                grdAnswerSheet.Children.Add(l);
                for (i = 1; i < nAns; ++i)
                {
                    l = new Label();
                    l.BorderBrush = brBK;
                    l.BorderThickness = vThickness[(int)ThicknessId.MT];
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(l, j);
                    Grid.SetColumn(l, i);
                    grdAnswerSheet.Children.Add(l);
                    AnswerSheet[j - 1][i - 1] = l;
                }
                l = new Label();
                l.BorderBrush = brBK;
                l.BorderThickness = vThickness[(int)ThicknessId.RT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, i);
                grdAnswerSheet.Children.Add(l);
                AnswerSheet[j - 1][nAns - 1] = l;
            }
            //bottom lines
            grdAnswerSheet.RowDefinitions.Add(new RowDefinition());
            AnswerSheet[j - 1] = new Label[nAns];
            l = new Label();
            l.Content = j;
            l.BorderBrush = brBK;
            l.BorderThickness = vThickness[(int)ThicknessId.LB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.FontFamily = vFontFml[1];
            l.FontWeight = FontWeights.Bold;
            l.Height = vWidth[1];
            Grid.SetRow(l, j);
            Grid.SetColumn(l, 0);
            grdAnswerSheet.Children.Add(l);
            for (i = 1; i < nAns; ++i)
            {
                l = new Label();
                l.BorderBrush = brBK;
                l.BorderThickness = vThickness[(int)ThicknessId.MB];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, i);
                grdAnswerSheet.Children.Add(l);
                AnswerSheet[j - 1][i - 1] = l;
            }
            l = new Label();
            l.BorderBrush = brBK;
            l.BorderThickness = vThickness[(int)ThicknessId.RB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            Grid.SetRow(l, j);
            Grid.SetColumn(l, i);
            grdAnswerSheet.Children.Add(l);
            AnswerSheet[j - 1][nAns - 1] = l;

            for (j = 0; j <= vQuest.Count; ++j)
                grdAnswerSheet.RowDefinitions[j].Height = new GridLength(1.2 * em);

            ScrollViewer scrlvwr = new ScrollViewer();
            scrlvwr.Height = gMain.RenderSize.Height * 2 / 3;
            scrlvwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrlvwr.Content = grdAnswerSheet;
            lp.Children.Add(scrlvwr);
			
			Grid.SetColumn(lp, 0);
			gMain.ColumnDefinitions[0].Width = GridLength.Auto;
			gMain.Children.Add(lp);
		}

        void InitQuestPanel()
        {
            Grid qs = new Grid();
			qs.Background = vBrush[(int)BrushId.Q_BG];
			qs.ColumnDefinitions.Add(new ColumnDefinition());
			qs.ColumnDefinitions.Add(new ColumnDefinition());
			int nc = vQuest.Count / 2;
			for(int i = 0; i < nc; ++i)
			{
				qs.RowDefinitions.Add(new RowDefinition());
				StackPanel q = CreateQuestion(2*i + 1);
				Grid.SetRow(q, i);
				Grid.SetColumn(q, 0);
				qs.Children.Add(q);
				q = CreateQuestion(2*i + 2);
				Grid.SetRow(q, i);
				Grid.SetColumn(q, 1);
				qs.Children.Add(q);
			}
            qs.Background = vBrush[(int)BrushId.BG];
			ScrollViewer scrlvwr = new ScrollViewer();
            //scrlvwr.Height = gMain.RenderSize.Height * 2 / 3;
            scrlvwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrlvwr.Content = qs;
			Grid.SetColumn(scrlvwr, 1);
			gMain.Children.Add(scrlvwr);
        }
		
		StackPanel CreateQuestion(int idx)
		{
			StackPanel q = new StackPanel();
			q.Orientation = Orientation.Horizontal;
            q.Margin = new Thickness(vWidth[3], vWidth[3], 0, vWidth[3]);
			Label l = new Label();
            l.HorizontalAlignment = HorizontalAlignment.Left;
			l.VerticalAlignment = VerticalAlignment.Top;
            l.FontSize = em;
            l.FontFamily = vFontFml[0];
            l.Content = idx;
            l.Background = vBrush[(int)BrushId.QID_BG];
            l.Foreground = vBrush[(int)BrushId.QID_Color];
            l.Width = vWidth[1];
			l.Height = 1.5f * em;
			l.HorizontalContentAlignment = HorizontalAlignment.Center;
			l.VerticalContentAlignment = VerticalAlignment.Center;
			l.Padding = new Thickness(0);
            q.Children.Add(l);
            StackPanel con = new StackPanel();
			TextBlock stmt = new TextBlock();
			stmt.FontSize = em;
			Question quest = vQuest[idx - 1];
			stmt.Text = quest.mStmt;
			stmt.TextWrapping = TextWrapping.Wrap;
            // dmsg.Content += "_" + idx + stmt.Text + vQuest.Count + "\n";
            stmt.Width = vWidth[4];
            stmt.Background = vBrush[(int)BrushId.Q_BG];
            Label stmtCon = new Label();
            stmtCon.Content = stmt;
            stmtCon.BorderBrush = vBrush[(int)BrushId.QID_BG];
            stmtCon.BorderThickness = new Thickness(0, 4, 0, 0);
            Thickness zero = new Thickness(0);
            stmtCon.Margin = stmtCon.Padding = zero;
            con.Children.Add(stmtCon);
            ListBox answers = new ListBox();
            answers.Width = vWidth[4];
            answers.Name = "_" + idx;
            answers.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            for (int i = 0; i < quest.vAns.Length; ++i)
            {
                ListBoxItem ans = new ListBoxItem();
                TextBlock ansTxt = new TextBlock();
                ansTxt.FontSize = em;
                ansTxt.Text = quest.vAns[i];
                ansTxt.TextWrapping = TextWrapping.Wrap;
                ansTxt.Width = vWidth[4] - SystemParameters.ScrollWidth;//minus is a trick
                ans.Content = ansTxt;
                ans.Name = "_" + i;
                ans.MouseLeftButtonUp += Ans_MouseLeftButtonUp;
                answers.Items.Add(ans);
            }
            answers.BorderBrush = vBrush[(int)BrushId.Ans_TopLine];
            answers.BorderThickness = new Thickness(0, 4, 0, 0);
            //answers.Margin = answers.Padding = zero;
            con.Children.Add(answers);
            q.Children.Add(con);
            q.Background = vBrush[(int)BrushId.BG];
            return q;
		}

        private void Ans_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem ans = (ListBoxItem)sender;
            ListBox answers = (ListBox)ans.Parent;
            int qid = Convert.ToInt32(answers.Name.Substring(1));
            for (int i = 0; i < answers.Items.Count; ++i)
            {
                ListBoxItem it = (ListBoxItem)answers.Items[i];
                if(it.IsSelected)
                    AnswerSheet[qid - 1][i].Content = 'X';
                else
                    AnswerSheet[qid - 1][i].Content = string.Empty;
            }
        }
    }
}
