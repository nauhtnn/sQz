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
	enum BrushId {
		BG = 0,
		QID_BG,
		QID_CL,
		Q_BG,
		ST_BT,
		C_HL,
		LP_BG,
		SH_BG,
		BT_HV,
		COUNT
	}
	
	enum ThknsId {
		LT = 0,
		MT,
		RT,
		LB,
		MB,
		RB,
		COUNT
	}
	
	enum ThemeId {
		HARVARD = 0,
		BERKELEY,
		COUNT
	}
	
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		double em = 16 * 1.2;
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
		
		void InitBrush() {
			vTheme = new SolidColorBrush[(int)ThemeId.COUNT][];
            //HARVARD theme
            SolidColorBrush[] br = new SolidColorBrush[(int)BrushId.COUNT];
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
			br[(int)BrushId.QID_CL] = new SolidColorBrush(c);
			c.R = c.G = c.B = 0xee;
			br[(int)BrushId.Q_BG] = new SolidColorBrush(c);
			c.R = 0xd8;
			c.G = 0x70;
			c.B = 0xb8;
			br[(int)BrushId.ST_BT] = new SolidColorBrush(c);
			c.R = 0x58;
			c.G = 0xa9;
			c.B = 0xb4;
			br[(int)BrushId.C_HL] = new SolidColorBrush(c);
			c.R = c.G = 0xff;
			c.B = 0xbb;
			br[(int)BrushId.LP_BG] = new SolidColorBrush(c);
			c.B = 0xdd;
			br[(int)BrushId.SH_BG] = new SolidColorBrush(c);
			c.R = 0xf1;
			c.G = 0x5a;
			c.B = 0x23;
			br[(int)BrushId.BT_HV] = new SolidColorBrush(c);
			vTheme[(int)ThemeId.HARVARD] = br;
			vBrush = vTheme[(int)ThemeId.HARVARD];
		}
		
		void InitThickness() {
			vThickness = new Thickness[(int)ThknsId.COUNT];
			vThickness[(int)ThknsId.LT] = new Thickness(0, 0, 0, 0);
			vThickness[(int)ThknsId.MT] = new Thickness(1, 1, 0, 0);
			vThickness[(int)ThknsId.RT] = new Thickness(1, 1, 1, 0);
			vThickness[(int)ThknsId.LB] = new Thickness(1, 1, 0, 1);
			vThickness[(int)ThknsId.MB] = new Thickness(1, 1, 0, 1);
			vThickness[(int)ThknsId.RB] = new Thickness(1, 1, 1, 1);
        }

        void InitLayout(object sender, RoutedEventArgs e)
        {
            InitLeftPanel();
            InitQuestPanel();
        }

		void InitLeftPanel()
		{
			//left panel
			Size sz = gMain.RenderSize;
			StackPanel lp = new StackPanel();
			lp.HorizontalAlignment = HorizontalAlignment.Left;
			//lp.Width = sz.Width / 5;
			lp.Background = vBrush[(int)BrushId.LP_BG];
			gMain.Children.Add(lp);
			//title
			Label l = new Label();
			l.Content = "Answer Sheet";
			l.FontFamily = vFontFml[0];
            l.FontWeight = FontWeights.Bold;
			l.FontSize = em;
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            lp.Children.Add(l);
			//answer sheet
			Grid ansSh = new Grid();
			ansSh.Background = vBrush[(int)BrushId.SH_BG];
			int nAns = 4;
            ++nAns;
            int i = 0;
            for (i = 0; i < nAns; ++i)
                ansSh.ColumnDefinitions.Add(new ColumnDefinition());
            for (i = 0; i < nAns; ++i)
                ansSh.ColumnDefinitions[i].Width = new GridLength(2 * em);
            --nAns;
            //top line
            ansSh.RowDefinitions.Add(new RowDefinition());
            l = new Label();
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, 0);
            ansSh.Children.Add(l);
            SolidColorBrush brBK = new SolidColorBrush(Colors.Black);
			for (i = 1; i < nAns; ++i)
			{
				l = new Label();
				l.BorderBrush = brBK;
				l.BorderThickness = vThickness[(int)ThknsId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.Content = (char)('@' + i);
                l.Width = 2 * em;
                l.FontFamily = vFontFml[1];
                l.FontWeight = FontWeights.Bold;
                Grid.SetRow(l, 0);
				Grid.SetColumn(l, i);
				ansSh.Children.Add(l);
			}
			l = new Label();
            l.BorderBrush = brBK;
			l.BorderThickness = vThickness[(int)ThknsId.RT];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.Content = (char)('@' + i);
            l.FontFamily = vFontFml[1];
            l.FontWeight = FontWeights.Bold;
            Grid.SetRow(l, 0);
			Grid.SetColumn(l, i);
			ansSh.Children.Add(l);
			//next lines
			int nQuest = 20;
            int j = 0;
            for (j = 1, i = 0; j < nQuest; ++j)
            {
                ansSh.RowDefinitions.Add(new RowDefinition());
                l = new Label();
                l.BorderBrush = brBK;
                l.BorderThickness = vThickness[(int)ThknsId.MT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.Content = j;
                l.FontFamily = vFontFml[1];
                l.FontWeight = FontWeights.Bold;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, 0);
                ansSh.Children.Add(l);
                for (i = 1; i < nAns; ++i)
                {
                    l = new Label();
                    l.BorderBrush = brBK;
                    l.BorderThickness = vThickness[(int)ThknsId.MT];
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(l, j);
                    Grid.SetColumn(l, i);
                    ansSh.Children.Add(l);
                }
                l = new Label();
                l.BorderBrush = brBK;
                l.BorderThickness = vThickness[(int)ThknsId.RT];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, i);
                ansSh.Children.Add(l);
            }
            //bottom lines
            ansSh.RowDefinitions.Add(new RowDefinition());
            l = new Label();
            l.BorderBrush = brBK;
            l.BorderThickness = vThickness[(int)ThknsId.LB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.Content = j;
            l.FontFamily = vFontFml[1];
            l.FontWeight = FontWeights.Bold;
            Grid.SetRow(l, j);
            Grid.SetColumn(l, 0);
            ansSh.Children.Add(l);
            for (i = 1; i < nAns; ++i)
            {
                l = new Label();
                l.BorderBrush = brBK;
                l.BorderThickness = vThickness[(int)ThknsId.MB];
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, j);
                Grid.SetColumn(l, i);
                ansSh.Children.Add(l);
            }
            l = new Label();
            l.BorderBrush = brBK;
            l.BorderThickness = vThickness[(int)ThknsId.RB];
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            Grid.SetRow(l, j);
            Grid.SetColumn(l, i);
            ansSh.Children.Add(l);

            for (j = 0; j <= nQuest; ++j)
                ansSh.RowDefinitions[j].Height = new GridLength(1.2 * em);

            ScrollViewer scrlvwr = new ScrollViewer();
            scrlvwr.Height = gMain.RenderSize.Height * 2 / 3;
            scrlvwr.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrlvwr.Content = ansSh;
            lp.Children.Add(scrlvwr);
		}

        void InitQuestPanel()
        {
            StackPanel q = new StackPanel();
            Label l = new Label();
            l.HorizontalAlignment = HorizontalAlignment.Left;
            l.FontSize = em;
            l.FontFamily = vFontFml[0];
            l.Content = 1;
            l.Background = vBrush[(int)BrushId.QID_BG];
            l.Foreground = vBrush[(int)BrushId.QID_CL];
            l.Width = 2 * em;
            q.Children.Add(l);
            gMain.Children.Add(q);
        }
	}
}
