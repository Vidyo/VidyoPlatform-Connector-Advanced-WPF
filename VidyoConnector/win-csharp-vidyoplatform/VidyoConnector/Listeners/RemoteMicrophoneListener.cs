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
    class RemoteMicrophoneListener : ListenerBase, IRegisterRemoteMicrophoneEventListener
    {
        public RemoteMicrophoneListener(VidyoConferenceModerationViewModel viewModel) : base(viewModel) { }

        public void OnRemoteMicrophoneAdded(RemoteMicrophone remoteMicrophone, Participant participant)
        {
            ConferenceViewModel.OnRemoteMicrophoneAdded(remoteMicrophone, participant);
        }

        public void OnRemoteMicrophoneRemoved(RemoteMicrophone remoteMicrophone, Participant participant)
        {
            ConferenceViewModel.OnRemoteMicrophoneRemoved(remoteMicrophone, participant);
        }

        public void OnRemoteMicrophoneStateUpdated(RemoteMicrophone remoteMicrophone, Participant participant, Device.DeviceState state)
        {
            ConferenceViewModel.OnRemoteMicrophoneStateUpdated(remoteMicrophone, participant, state);
        }
    }
}
