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

        private void onLoaded(object sender, RoutedEventArgs e)
        {
            int nQuest = 30;
            int nAns = 4;
            ++nAns;
            char[] vAnsTit = new char[nAns];
            for (int i = 1; i < nAns; ++i)
                vAnsTit[i] = (char)('@' + i);
            Size sz = gMain.RenderSize;
            StackPanel lp = new StackPanel();
            lp.HorizontalAlignment = HorizontalAlignment.Left;
            lp.Width = sz.Width / 5;
            Color c = new Color();
            c.A = c.R = c.G = 255;
            c.B = 187;
            lp.Background = new SolidColorBrush(c);
            gMain.Children.Add(lp);
            Label lblSh = new Label();
            lblSh.Content = "Answer Sheet";
            lblSh.FontFamily = new FontFamily("Times New Roman");
            lblSh.FontSize = 1.2 * em;
            lblSh.HorizontalAlignment = HorizontalAlignment.Center;
            lp.Children.Add(lblSh);
            Grid ansSh = new Grid();
            for (int i = 0; i < nQuest; ++i)
            {
                RowDefinition rd = new RowDefinition();
                //rd.Height = new GridLength(20);
                ansSh.RowDefinitions.Add(rd);
            }
            for (int i = 0; i < nAns; ++i)
                ansSh.ColumnDefinitions.Add(new ColumnDefinition());
            c.B = 221;
            ansSh.Background = new SolidColorBrush(c);
            SolidColorBrush bbk = new SolidColorBrush(Colors.Black);
            Thickness lt = new Thickness(1, 1, 0, 0);
            Label l = new Label();
            ansSh.Children.Add(l);
            for (int i = 1; i < nAns; ++i)
            {
                l = new Label();
                l.BorderBrush = bbk;
                l.BorderThickness = lt;
                l.Content = i;
                Grid.SetRow(l, 0);
                Grid.SetColumn(l, i);
                ansSh.Children.Add(l);
            }
            //for(int i = 0; i < nQuest; ++i)
            //    for (int j = 0; j < nAns; ++j)
            //    {
            //        l = new Label();
            //        l.BorderBrush = new SolidColorBrush(Colors.Black);
            //        l.BorderThickness = new Thickness(1, 1, 0, 0);
            //        l.Background = new SolidColorBrush(Colors.AliceBlue);
            //        l.Content = i;
            //        Grid.SetRow(l, i);
            //        Grid.SetColumn(l, j);
            //        ansSh.Children.Add(l);
            //    }
            lp.Children.Add(ansSh);
        }
    }
}
