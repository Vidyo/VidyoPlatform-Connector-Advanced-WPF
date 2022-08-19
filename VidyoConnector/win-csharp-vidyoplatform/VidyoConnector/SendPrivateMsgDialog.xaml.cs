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
    /// Interaction logic for SendPrivateMessage.xaml
    /// </summary>
    public partial class SendPrivateMsgDialog : Window
    {

        private string privateMsg;
        public SendPrivateMsgDialog(string partipantName)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            privateMsg = "";
            LabelRecipientName.Content = "Recipient Name : " + partipantName;
        }

        private void ButtonCancelPrivateMsg_Click(object sender, RoutedEventArgs e)
        {
            privateMsg = "";
            DialogResult = false;
        }

        private void ButtonSendPrivateMsg_Click(object sender, RoutedEventArgs e)
        {
            privateMsg = TextBoxSendPrivateMsg.Text;
            DialogResult = TextBoxSendPrivateMsg.Text.Length != 0 ? true : false;
        }
        public string GetPrivateMsg()
        {
            return privateMsg;
        }
    }
}
