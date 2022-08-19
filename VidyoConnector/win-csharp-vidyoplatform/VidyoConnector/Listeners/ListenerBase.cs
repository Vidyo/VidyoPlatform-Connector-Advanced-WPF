using System;
using VidyoConnector.ViewModel;
using VidyoConferenceModeration.ViewModel;
using SearchUsersDialog.ViewModel;
using VidyoAnalytics.ViewModel;
using VidyoConnector.Model;

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
        protected readonly VidyoAnalyticsViewModel AnalyticsViewModel;
        protected readonly VidyoConnectorShareViewModel SharingViewModel;
        protected readonly VidyoConnector.Model.RemoteCameraModel RemoteCameraModel;

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

        public ListenerBase(VidyoAnalyticsViewModel viewModel)
        {
            AnalyticsViewModel = viewModel;
        }

        public ListenerBase(VidyoConnectorShareViewModel viewModel)
        {
            SharingViewModel = viewModel;
        }

       public ListenerBase(VidyoConnector.Model.RemoteCameraModel viewModel)
        {
            RemoteCameraModel = viewModel;
        }

        public void LogCallback(string name)
        {
            ViewModel.Log.Debug(string.Format("Recieved callback: {0}", name));
        }
    }
}