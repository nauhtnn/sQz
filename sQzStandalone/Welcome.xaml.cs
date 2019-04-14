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

namespace sQzStandalone
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Page
    {
        QuestSheet QS;
        ExamPage examPage;

        public Welcome()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            QS = new QuestSheet();
            QS.ReadDocx(System.IO.Directory.GetCurrentDirectory() + "\\QS.docx");
            examPage = new ExamPage();
            examPage.mNee = new ExamineeC("A0001");
            examPage.mQSh = QS;
            NavigationService.Navigate(examPage);
        }
    }
}
