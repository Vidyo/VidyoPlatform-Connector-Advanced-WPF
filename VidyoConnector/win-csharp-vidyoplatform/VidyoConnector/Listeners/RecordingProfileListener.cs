using System;
using System.Reflection;
using System.Windows;
using VidyoClient;
using VidyoConnector.Listeners;
using VidyoConferenceModeration.ViewModel;
using System.Collections.Generic;
using static VidyoClient.Connector;

namespace VidyoConferenceModeration.Listeners
{
    class RecordingProfileListener : ListenerBase, IGetRecordingServiceProfiles
    {
        public RecordingProfileListener(VidyoConferenceModerationViewModel viewModel) : base(viewModel) { }

        public void OnGetRecordingServiceProfiles(List<string> profiles, List<string> prefixes, Connector.ConnectorRecordingServiceResult result)
        {
            ConferenceViewModel.OnGetRecordingServiceProfiles(profiles, prefixes, result);
        }
    }
}
