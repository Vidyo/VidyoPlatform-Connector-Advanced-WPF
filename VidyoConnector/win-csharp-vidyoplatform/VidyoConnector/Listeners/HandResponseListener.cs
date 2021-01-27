using System;
using System.Reflection;
using System.Windows;
using VidyoClient;
using VidyoConnector.Listeners;
using VidyoConferenceModeration.ViewModel;
using static VidyoClient.Connector;

namespace VidyoConferenceModeration.Listeners
{
    public class HandResponseListener : ListenerBase, IRaiseHand
    {
        public HandResponseListener(VidyoConferenceModerationViewModel viewModel) : base(viewModel) { }

        public void OnRaiseHandResponse(Participant.ParticipantHandState handState)
        {
            ConferenceViewModel.OnRaiseHandResponse(handState);
        }
    }
}
