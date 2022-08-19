using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using VidyoConnector.Model;

namespace VidyoConnector
{
    /// <summary>
    /// Interaction logic for VidyoCameraViscaCommand.xaml
    /// </summary>
    public partial class VidyoCameraViscaCommand : Window
    {
        RemoteCameraModel _camera;
        public VidyoCameraViscaCommand(RemoteCameraModel camera)
        {
            InitializeComponent();
            _camera = camera;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void ButtonSendViscaCommand_Click(object sender, RoutedEventArgs e)
        {
            if(!_camera.RemoteCamera_SendViscaCommand(TextBoxViscaCommand.Text, TextBoxViscaCommandId.Text))
            {
                MessageBox.Show("Failed to send Visca Command.", "Visca Command");
            }
            DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
    }
}
