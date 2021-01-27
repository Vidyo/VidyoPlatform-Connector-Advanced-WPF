using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VidyoConferenceModeration;
using VidyoConferenceModeration.ViewModel;
using VidyoConnector.ViewModel;

namespace VidyoConnector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConferenceModerationWindow conferenceModeration;
		
        public MainWindow()
        {
            InitializeComponent();
            RadioBtnGuest.IsChecked = true;
            ((VidyoConnectorViewModel)DataContext).Init(VideoPanel.Handle, (uint)VideoPanel.Width, (uint)VideoPanel.Height);
            conferenceModeration = new ConferenceModerationWindow();

            ((VidyoConnectorViewModel)DataContext).SetConferenceModerationViewModel(conferenceModeration.GetVidyoConferenceModerationViewModel());
            
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((VidyoConnectorViewModel)DataContext).Password = PasswordBoxLogin.Password;
        }
		
        private void BtnGuest_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockUserName.Visibility = System.Windows.Visibility.Hidden;

            TextBoxUserName.Visibility = System.Windows.Visibility.Hidden;
            TextBoxUserName.Text = String.Empty;
            
            TextBlockPassword.Visibility = System.Windows.Visibility.Hidden;

            PasswordBoxLogin.Visibility = System.Windows.Visibility.Hidden;
            PasswordBoxLogin.Password = String.Empty;
            

            TextBlockDisplayName.Visibility = System.Windows.Visibility.Visible;

            TextBoxDisplayName.Visibility = System.Windows.Visibility.Visible;
            TextBoxDisplayName.Text = String.Empty;
            TextBoxRoomKey.Text = String.Empty;
            TextBoxRoomPin.Text = String.Empty;

            TextBoxPortal.Text = @"vidyocloud.com";
        }
        private void BtnUser_Checked(object sender, RoutedEventArgs e)
        {
            TextBlockUserName.Visibility = System.Windows.Visibility.Visible;
            TextBoxUserName.Visibility = System.Windows.Visibility.Visible;
            TextBlockPassword.Visibility = System.Windows.Visibility.Visible;
            PasswordBoxLogin.Visibility = System.Windows.Visibility.Visible;
            TextBlockDisplayName.Visibility = System.Windows.Visibility.Collapsed;
            TextBoxDisplayName.Visibility = System.Windows.Visibility.Collapsed;

            TextBoxPortal.Text = @"vidyocloud.com";
            TextBoxDisplayName.Text = String.Empty;
            TextBoxRoomKey.Text = String.Empty;
            TextBoxRoomPin.Text = String.Empty;
            TextBoxUserName.Text = String.Empty;
            PasswordBoxLogin.Password = String.Empty;
        }
        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((VidyoConnectorViewModel)DataContext).AdjustVideoPanelSize(VideoPanel.Handle, (uint)wfHost.ActualWidth, (uint)wfHost.ActualHeight/*(uint)VideoPanel.Width, (uint)VideoPanel.Height*/);
        }

        private void MenuItemCameras_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as MenuItem;
            if (selectedMenuItem != null)
            {
                ((VidyoConnectorViewModel)DataContext).SetSelectedLocalCamera(selectedMenuItem.DataContext);
            }
        }

        private void MenuItemVideoContent_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as MenuItem;
            if (selectedMenuItem != null)
            {
                ((VidyoConnectorViewModel)DataContext).SetSelectedVideoContent(selectedMenuItem.DataContext);
            }
        }

        private void MenuItemMicrophones_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as MenuItem;
            if (selectedMenuItem != null)
            {
                ((VidyoConnectorViewModel)DataContext).SetSelectedLocalMicrophone(selectedMenuItem.DataContext);
            }
        }

        private void MenuItemAudioContent_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as MenuItem;
            if (selectedMenuItem != null)
            {
                ((VidyoConnectorViewModel)DataContext).SetSelectedAudioContent(selectedMenuItem.DataContext);
            }
        }

        private void MenuItemSpeakers_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as MenuItem;
            if (selectedMenuItem != null)
            {
                ((VidyoConnectorViewModel)DataContext).SetSelectedLocalSpeaker(selectedMenuItem.DataContext);
            }
        }

        private void MenuItemSharesMonitors_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as MenuItem;
            if (selectedMenuItem != null)
            {
                ((VidyoConnectorViewModel)DataContext).SetSelectedLocalMonitor(selectedMenuItem.DataContext);
            }
        }

        private void MenuItemSharesWindows_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as MenuItem;
            if (selectedMenuItem != null)
            {
                ((VidyoConnectorViewModel)DataContext).SetSelectedLocalWindow(selectedMenuItem.DataContext);
            }
        }

        private void BtnChat_Click(object sender, RoutedEventArgs e)
        {
            var connectionState = ((VidyoConnectorViewModel)DataContext).ConnectionState;
            if (connectionState == Model.ConnectionState.Connected)
            {
                if (gridChat.Visibility == Visibility.Collapsed)
                {
                    gridChat.Visibility = Visibility.Visible;
                }
                else if (gridChat.Visibility == Visibility.Visible)
                {
                    gridChat.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void MenuItem_ConferenceModerationClick(object sender, RoutedEventArgs e)
        {
            var connectionState = ((VidyoConnectorViewModel)DataContext).ConnectionState;
            if (connectionState == Model.ConnectionState.Connected)
            {
                if (((VidyoConnectorViewModel)DataContext).IsLocalUserGuest())
                {
                    MessageBoxResult msg = MessageBox.Show("We are sorry, you don't have enough privileges to perform moderation operation", "Conference Moderation");
                    return;
                }

                conferenceModeration.Init();
                conferenceModeration.Show();
            }
            else
            {
                MessageBoxResult msg = MessageBox.Show("You must be in a conference to perform Moderation", "Conference Moderation");
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void OnMainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((VidyoConnectorViewModel)DataContext).Deinit();
        }
    }
}
