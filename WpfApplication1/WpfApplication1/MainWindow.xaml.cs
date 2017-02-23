using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
		int AppWidth = 0;
		int LeftPanelWidth = 0;
		int QuestionWidth = 0;
        Grid grdAnswerSheet;
        Label[][] AnswerSheet;
		
		public MainWindow()
		{
			InitializeComponent();
            InitBrush();
            InitThickness();
            vFontFml = new FontFamily[2];
            vFontFml[0] = new FontFamily("Arial");
            vFontFml[1] = new FontFamily("Arial");
        }
		
		SolidColorBrush[] vBrush;
		SolidColorBrush[][] vTheme;
		Thickness[] vThickness;
        FontFamily[] vFontFml;
		int nQuest = 20;
		
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
			AppWidth = (int)gMain.RenderSize.Width;
			gMain.ColumnDefinitions.Add(new ColumnDefinition());
			gMain.ColumnDefinitions.Add(new ColumnDefinition());
            InitLeftPanel();
            InitQuestPanel();
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
			int nAns = 4;
            ++nAns;
            int i = 0;
            for (i = 0; i < nAns; ++i)
                grdAnswerSheet.ColumnDefinitions.Add(new ColumnDefinition());
            for (i = 0; i < nAns; ++i)
                grdAnswerSheet.ColumnDefinitions[i].Width = new GridLength(2 * em);
            --nAns;
            AnswerSheet = new Label[nQuest][];
            //top line
            grdAnswerSheet.RowDefinitions.Add(new RowDefinition());
            l = new Label();
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, 0);
            grdAnswerSheet.Children.Add(l);
            SolidColorBrush brBK = new SolidColorBrush(Colors.Black);
			for (i = 1; i < nAns; ++i)
			{
				l = new Label();
				l.BorderBrush = brBK;
				l.BorderThickness = vThickness[(int)ThicknessId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.Content = (char)('@' + i);
                //l.Width = 2 * em;
                l.FontFamily = vFontFml[1];
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
            l.FontFamily = vFontFml[1];
            l.FontWeight = FontWeights.Bold;
            Grid.SetRow(l, 0);
			Grid.SetColumn(l, i);
			grdAnswerSheet.Children.Add(l);
			//next lines
            int j = 0;
            for (j = 1, i = 0; j < nQuest; ++j)
            {
                grdAnswerSheet.RowDefinitions.Add(new RowDefinition());
                AnswerSheet[j - 1] = new Label[nAns];
                l = new Label();
                l.BorderBrush = brBK;
                l.BorderThickness = vThickness[(int)ThicknessId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.Content = j;
                l.FontFamily = vFontFml[1];
                l.FontWeight = FontWeights.Bold;
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
            l.BorderBrush = brBK;
            l.BorderThickness = vThickness[(int)ThicknessId.LB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.Content = j;
            l.FontFamily = vFontFml[1];
            l.FontWeight = FontWeights.Bold;
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

            for (j = 0; j <= nQuest; ++j)
                grdAnswerSheet.RowDefinitions[j].Height = new GridLength(1.2 * em);

            ScrollViewer scrlvwr = new ScrollViewer();
            scrlvwr.Height = gMain.RenderSize.Height * 2 / 3;
            scrlvwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrlvwr.Content = grdAnswerSheet;
            lp.Children.Add(scrlvwr);
			
			Grid.SetColumn(lp, 0);
			gMain.ColumnDefinitions[0].Width = GridLength.Auto;
			gMain.Children.Add(lp);
			LeftPanelWidth = (int)lp.ActualWidth;
			QuestionWidth = (AppWidth - LeftPanelWidth) / 2;
		}

        void InitQuestPanel()
        {
            Grid qs = new Grid();
			qs.Background = vBrush[(int)BrushId.Q_BG];
			qs.ColumnDefinitions.Add(new ColumnDefinition());
			qs.ColumnDefinitions.Add(new ColumnDefinition());
			int nc = nQuest / 2;
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
			q.Orientation = System.Windows.Controls.Orientation.Horizontal;
            q.Margin = new Thickness(8, 0, 0, 8);
			Label l = new Label();
            l.HorizontalAlignment = HorizontalAlignment.Left;
			l.VerticalAlignment = VerticalAlignment.Top;
            l.FontSize = em;
            l.FontFamily = vFontFml[0];
            l.Content = idx;
            l.Background = vBrush[(int)BrushId.QID_BG];
            l.Foreground = vBrush[(int)BrushId.QID_Color];
            l.Width = 2 * em;
			l.Height = 1.5f * em;
			l.HorizontalContentAlignment = HorizontalAlignment.Center;
			l.VerticalContentAlignment = VerticalAlignment.Center;
			l.Padding = new Thickness(0);
            q.Children.Add(l);
            StackPanel con = new StackPanel();
			Label stmt = new Label();
			stmt.FontSize = em;
			stmt.Content = "abcd\nabcd\nabcd";
			stmt.Width = QuestionWidth - (int)l.ActualWidth;
			stmt.BorderBrush = vBrush[(int)BrushId.QID_BG];
			stmt.BorderThickness = new Thickness(0, 4, 0, 0);
            stmt.Background = vBrush[(int)BrushId.Q_BG];
            con.Children.Add(stmt);
            ListBox answers = new ListBox();
            answers.Name = "_" + idx;
            for (int i = 0; i < 4; ++i)
            {
                ListBoxItem ans = new ListBoxItem();
                ans.FontSize = em;
                ans.Content = "list box item";
                ans.Name = "_" + i;
                ans.MouseLeftButtonUp += Ans_MouseLeftButtonUp;
                answers.Items.Add(ans);
            }
            answers.BorderBrush = vBrush[(int)BrushId.Ans_TopLine];
            answers.BorderThickness = new Thickness(0, 4, 0, 0);
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
