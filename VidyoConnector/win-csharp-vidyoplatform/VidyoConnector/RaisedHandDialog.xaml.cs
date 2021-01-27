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
    /// Interaction logic for RaisedHandDialog.xaml
    /// </summary>
    /// 

    public partial class RaisedHandDialog : Window
    {
        private bool RaisedHandResponse;
        public RaisedHandDialog()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            RaisedHandResponse = true;
        }

        public bool GetRaisedHandResponse()
        {
            return RaisedHandResponse;
        }

        private void RadioButtonApprove_Checked(object sender, RoutedEventArgs e)
        {
            RaisedHandResponse =  true;
        }

        private void RadioButtonDismiss_Checked(object sender, RoutedEventArgs e)
        {
            RaisedHandResponse = false;
        }

        private void ButtonSubmitRaisedHandResponse_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancelRaisedHandResponse_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
