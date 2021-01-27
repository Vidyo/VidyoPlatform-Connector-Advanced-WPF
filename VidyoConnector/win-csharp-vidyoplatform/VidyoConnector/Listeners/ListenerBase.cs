using System;
using VidyoConnector.ViewModel;
using VidyoConferenceModeration.ViewModel;
using SearchUsersDialog.ViewModel;

namespace VidyoConnector.Listeners
{
    /// <summary>
    /// Represents common members of all Listeners.
    /// </summary>
    public class ListenerBase
    {
        /// <summary>
        /// ViewModel object which operates application data.
        /// </summary>
        protected readonly VidyoConnectorViewModel ViewModel;
        protected readonly VidyoConferenceModerationViewModel ConferenceViewModel;
        protected readonly SearchUsersDialogViewModel SearchUsersDialogViewModel;

        public ListenerBase(VidyoConnectorViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public ListenerBase(VidyoConferenceModerationViewModel viewModel)
        {
            ConferenceViewModel = viewModel;
        }

        public ListenerBase(SearchUsersDialogViewModel viewModel)
        {
            SearchUsersDialogViewModel = viewModel;
        }

        public void LogCallback(string name)
        {
            ViewModel.Log.Debug(string.Format("Recieved callback: {0}", name));
        }
    }
}