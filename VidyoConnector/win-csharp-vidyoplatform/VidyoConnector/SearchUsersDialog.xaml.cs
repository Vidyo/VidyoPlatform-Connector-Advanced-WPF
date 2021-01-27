using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VidyoConnector.ViewModel;
using VidyoConferenceModeration.ViewModel;
using VidyoConnector;
using System.ComponentModel;
using VidyoClient;
using SearchUsersDialog.ViewModel;

namespace SearchUsersDialog
{
    /// <summary>
    /// Interaction logic for SearchUsersDialog.xaml
    /// </summary>
    public partial class SearchUsers : Window
    {
        SearchUsersDialogViewModel viewModel;

        public SearchUsers()
        {
            InitializeComponent();
            viewModel = new SearchUsersDialogViewModel();
            DataContext = viewModel;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void ButtonSearchUsers_Click(object sender, RoutedEventArgs e)
        {
            ((SearchUsersDialogViewModel)DataContext).SearchUsersDialogViewModel_SearchUsers(TextBoxSearchUsers.Text);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void ButtonInviteParticipants_Click(object sender, RoutedEventArgs e)
        {
            ((SearchUsersDialogViewModel)DataContext).SearchUsersDialogViewModel_InviteParticipant();
            DialogResult = true;
        }
    }
}
