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
        VidyoConferenceOptions conferenceOptions;
		VidyoAnalytics analytics;
        VidyoConferenceRendererOptions conferenceRendererOptions;
        VidyoCameraEffect cameraEffect;
        VidyoVirtualDevices virtualDevices;
        VidyoConferenceSharing conferenceShareWindow;
        VidyoProductInfo productInfo;

        public MainWindow()
        {
            InitializeComponent();
            RadioBtnGuest.IsChecked = true;
            ((VidyoConnectorViewModel)DataContext).Init(VideoPanel.Handle, (uint)VideoPanel.Width, (uint)VideoPanel.Height);
            conferenceModeration = new ConferenceModerationWindow((VidyoConnectorViewModel)DataContext);            
            analytics = new VidyoAnalytics();            
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            conferenceRendererOptions = new VidyoConferenceRendererOptions();

            ((VidyoConnectorViewModel)DataContext).SetConferenceModerationViewModel(conferenceModeration.GetVidyoConferenceModerationViewModel());
            
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            cameraEffect = new VidyoCameraEffect();
            virtualDevices = new VidyoVirtualDevices(DataContext);

            conferenceShareWindow = new VidyoConferenceSharing();
            conferenceShareWindow.SetConferenceShareViewModel(((VidyoConnectorViewModel)DataContext).GetVidyoConnectorShareViewModel());

            productInfo = new VidyoProductInfo(((VidyoConnectorViewModel)DataContext).GetApplicationVersion());
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
            ((VidyoConnectorViewModel)DataContext).SetLoginType(UserLoginType.AsGuest);
            ((VidyoConnectorViewModel)DataContext).ResetErrorMessage();
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
            ((VidyoConnectorViewModel)DataContext).SetLoginType(UserLoginType.AsUser);
            ((VidyoConnectorViewModel)DataContext).ResetErrorMessage();
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

        private void MenuItem_VirtualMicrophone_Click(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as MenuItem;
            if (selectedMenuItem != null)
            {
                ((VidyoConnectorViewModel)DataContext).SetSelectedVirtualMicrophone(selectedMenuItem.DataContext);
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
                    MessageBoxResult msg = MessageBox.Show("You don't have enough privileges to perform moderation operation. Only FECC is allowed.", "Conference Moderation");
                }

                conferenceModeration.Init();
                conferenceModeration.Show();
            }
            else
            {
                MessageBoxResult msg = MessageBox.Show("You must be in a conference to perform Moderation", "Conference Moderation");
            }
        }

        private void MenuItem_AnalyticsClick(object sender, RoutedEventArgs e)
        {
            analytics.Init();
            analytics.ShowDialog();
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

        private void MenuItem_ConferenceOptionClick(object sender, RoutedEventArgs e)
        {
            conferenceOptions = new VidyoConferenceOptions(((VidyoConnectorViewModel)DataContext).GetOptions());
            conferenceOptions.setOptions = ((VidyoConnectorViewModel)DataContext).SetOptions;
            conferenceOptions.getOptions = ((VidyoConnectorViewModel)DataContext).GetOptions;
            conferenceOptions.Show();
        }

        private void MenuItem_ConferenceRendererOptionClick(object sender, RoutedEventArgs e)
        {
            conferenceRendererOptions.setRendererOptions = ((VidyoConnectorViewModel)DataContext).SetRendererOptions;
            conferenceRendererOptions.getRendererOptions = ((VidyoConnectorViewModel)DataContext).GetRendererOptions;
            conferenceRendererOptions.Show();
        }

        private void MenuItem_BackgroundSelect(object sender, RoutedEventArgs e)
        {
            cameraEffect.Init();
            cameraEffect.Show();
        }

        private void MenuItem_VirtualDevices(object sender, RoutedEventArgs e)
        {
            virtualDevices.Show();
        }

        private void MenuItem_SetProductInfo(object sender, RoutedEventArgs e)
        {
            productInfo.setProductInfo = ((VidyoConnectorViewModel)DataContext).SetProductInfo;
            productInfo.Show();
        }

        private void MenuItem_ShareDevices(object sender, RoutedEventArgs e)
        {
            if (conferenceShareWindow.Init())
                conferenceShareWindow.Show();
        }

        private void MenuItemVirtualAudioContent_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as MenuItem;
            if (selectedMenuItem != null)
            {
                ((VidyoConnectorViewModel)DataContext).SetSelectedVirtualAudioContent(selectedMenuItem.DataContext);
            }
        }
    }
}
