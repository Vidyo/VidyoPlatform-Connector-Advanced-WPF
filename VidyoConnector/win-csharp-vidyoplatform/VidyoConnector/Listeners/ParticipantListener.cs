using System;
using System.Collections.Generic;
using VidyoClient;
using VidyoConnector.ViewModel;

namespace VidyoConnector.Listeners
{
    public class ParticipantListener : ListenerBase, Connector.IRegisterParticipantEventListener
    {
        public ParticipantListener(VidyoConnectorViewModel viewModel) : base(viewModel) { }

        public void OnParticipantJoined(Participant participant)
        {
            ViewModel.OnParticipantJoined(participant);
        }

        public void OnParticipantLeft(Participant participant)
        {
            ViewModel.OnParticipantLeft(participant);
        }

        public void OnDynamicParticipantChanged(List<Participant> participants)
        {
            ViewModel.OnDynamicParticipantChanged(participants);
        }

        public void OnLoudestParticipantChanged(Participant participant, bool audioOnly)
        {
            ViewModel.OnLoudestParticipantChanged(participant, audioOnly);
        }
    }
}
