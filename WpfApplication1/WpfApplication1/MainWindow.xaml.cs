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
		BT_BD,
		BT_HV,
		COUNT
	}
	
	enum BorThkns {
		LT,
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
		}
		
		SolidColorBrush[] vBrush;
		SolidColorBrush[][] vTheme;
		BorderThickness[] vBorThkns;
		
		void InitBrush() {
			vTheme = new SolidColorBrush[ThemeId.COUNT][BrushId.COUNT];
			//HARVARD theme
			br = new SolidColorBrush[BrushId.COUNT];
			Color c = new Color();
			c.A = 0xff;
			c.R = 0xc3;
			c.G = 0xd7;
			c.B = 0xa4;
			br[BrushId.BG] = new SolidColorBrush(c);
			c.R = 0xa5;
			c.G = 0x1c;
			c.B = 0x30;
			br[BrushId.QID_BG] = new SolidColorBrush(c);
			c.R = c.G = c.B = 0xff;
			br[BrushId.QID_CL] = new SolidColorBrush(c);
			c.R = c.G = c.B = 0xee;
			br[BrushId.Q_BG] = new SolidColorBrush(c);
			BG = 0,
			c.R = 0xd8;
			c.G = 0x70;
			c.B = 0xb8;
			br[BrushId.ST_BT] = new SolidColorBrush(c);
			c.R = 0x58;
			c.G = 0xa9;
			c.B = 0xb4;
			br[BrushId.C_HL] = new SolidColorBrush(c);
			c.R = c.G = 0xff;
			c.B = 0xbb;
			br[BrushId.LP_BG] = new SolidColorBrush(c);
			c.B = 0xdd;
			br[BrushId.SH_BG] = new SolidColorBrush(c);
			c.R = 0xf1;
			c.G = 0x5a;
			c.B = 0x23;
			br[BrushId.BT_HV] = new SolidColorBrush(c);
			vTheme[ThemeId.HARVARD] = br;
			vBrush = vTheme[ThemeId.HARVARD];
		}
		
		void InitBorderThickness() {
			vBorThkns = new BorderThickness[BorThkns.COUNT];
			vBorThkns[BorThkns.LT] = new BorderThickness(0, 0, 0, 0);
			vBorThkns[BorThkns.MT] = new BorderThickness(1, 1, 0, 0);
			vBorThkns[BorThkns.RT] = new BorderThickness(1, 1, 1, 0);
			vBorThkns[BorThkns.LB] = new BorderThickness(1, 1, 0, 1);
			vBorThkns[BorThkns.MB] = new BorderThickness(1, 1, 0, 1);
			vBorThkns[BorThkns.RB] = new BorderThickness(1, 1, 1, 1);
		}

		private void InitLayout(object sender, RoutedEventArgs e)
		{
			//left panel
			Size sz = gMain.RenderSize;
			StackPanel lp = new StackPanel();
			lp.HorizontalAlignment = HorizontalAlignment.Left;
			lp.Width = sz.Width / 5;
			lp.Background = br[BrushId.LP_BG];
			gMain.Children.Add(lp);
			//title
			Label lblSh = new Label();
			lblSh.Content = "Answer Sheet";
			lblSh.FontFamily = new FontFamily("Times New Roman");
			lblSh.FontSize = 1.2 * em;
			lblSh.HorizontalAlignment = HorizontalAlignment.Center;
			lp.Children.Add(lblSh);
			//answer sheet
			Grid ansSh = new Grid();
			ansSh.Background = br[BrushId.SH_BG];
			int nAns = 4;
			//top line
			++nAns;
			for (int i = 0; i < nAns; ++i)
				ansSh.ColumnDefinitions.Add(new ColumnDefinition());
			ansSh.ColumnDefinitions[0].Width = 2*em;
			--nAns;
			Label l = new Label();
			ansSh.Children.Add(l);
			brBK = new SolidColorBrush(Colors.Black);
			int i = 1;
			for (; i < nAns; ++i)
			{
				l = new Label();
				l.BorderBrush = brBK;
				l.BorderThickness = vBorThkns[BorThkns.MT];
				l.Content = (char)('@' + i);
				Grid.SetRow(l, 0);
				Grid.SetColumn(l, i);
				ansSh.Children.Add(l);
			}
			l = new Label();
			l.BorderBrush = brBK;
			l.BorderThickness = vBorThkns[BorThkns.RT];
			l.Content = (char)('@' + i);
			Grid.SetRow(l, j);
			Grid.SetColumn(l, i);
			ansSh.Children.Add(l);
			//next lines
			int nQuest = 30;
			l = new Label();
			l.BorderBrush = brBK;
			l.BorderThickness = vBorThkns[BorThkns.RT];
			l.Content = (char)('@' + i);
			Grid.SetRow(l, 0);
			Grid.SetColumn(l, i);
			ansSh.Children.Add(l);
			//bottom lines
			//for(int i = 0; i < nQuest; ++i)
			//	for (int j = 0; j < nAns; ++j)
			//	{
			//		l = new Label();
			//		l.BorderBrush = new SolidColorBrush(Colors.Black);
			//		l.BorderThickness = new Thickness(1, 1, 0, 0);
			//		l.Background = new SolidColorBrush(Colors.AliceBlue);
			//		l.Content = i;
			//		Grid.SetRow(l, i);
			//		Grid.SetColumn(l, j);
			//		ansSh.Children.Add(l);
			//	}
			lp.Children.Add(ansSh);
		}
	}
}
