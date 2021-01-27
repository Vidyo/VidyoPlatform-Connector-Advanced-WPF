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
using VidyoClient;

namespace VidyoConnector
{
    /// <summary>
    /// Interaction logic for ModeratorPinDialog.xaml
    /// </summary>
    public partial class ModeratorPinDialog : Window
    {
        public ModeratorPinDialog()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        public string ModeratorPin
        {
            get { return PasswordBoxModeratorPIN.Password; }
            set { PasswordBoxModeratorPIN.Password = value; }
        }

        private void ButtonSetModeratorPIN_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonResetModeratorPIN_Click(object sender, RoutedEventArgs e)
        {
            PasswordBoxModeratorPIN.Clear();
        }
    }
}
