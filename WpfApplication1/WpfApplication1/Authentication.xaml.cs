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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Authentication.xaml
    /// </summary>
    public partial class Authentication : Page
    {
        public Authentication()
        {
            InitializeComponent();
        }

        private void SignIn(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TakeExam());
            //txMessage.Text += "_" + txtUsername.Text + "_" + txtPassword + "\n";
            //ClientInstance.Connect("127.0.0.1", "hello world");
        }

        private void btnStartSer_Click(object sender, RoutedEventArgs e)
        {
            ServerInstance.Start();
        }

        private void btnStopSer_Click(object sender, RoutedEventArgs e)
        {
            ServerInstance.Stop();
        }
    }
}
