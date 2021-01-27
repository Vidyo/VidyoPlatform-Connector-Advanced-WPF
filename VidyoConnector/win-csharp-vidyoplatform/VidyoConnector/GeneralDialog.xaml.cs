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
using System.Windows.Shapes;

namespace VidyoConnector
{
    /// <summary>
    /// Interaction logic for GeneralDialog.xaml
    /// </summary>
    public partial class GeneralDialog : Window
    {
        public GeneralDialog()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        public void GeneralDialog_ShowMessage(String msgHeader, String msg)
        {
            LabelMessageHeader.Content = msgHeader;
            TextBlockGeneralDialog.Text = msg;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
