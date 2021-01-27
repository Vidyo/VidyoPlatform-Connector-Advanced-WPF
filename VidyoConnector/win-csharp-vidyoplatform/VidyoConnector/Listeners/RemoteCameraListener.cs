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
    class RemoteCameraListener : ListenerBase, IRegisterRemoteCameraEventListener
    {
        public RemoteCameraListener(VidyoConferenceModerationViewModel viewModel) : base(viewModel) { }

        public void OnRemoteCameraAdded(RemoteCamera remoteCamera, Participant participant)
        {
            ConferenceViewModel.OnRemoteCameraAdded(remoteCamera, participant);
        }

        public void OnRemoteCameraRemoved(RemoteCamera remoteCamera, Participant participant)
        {
            ConferenceViewModel.OnRemoteCameraRemoved(remoteCamera, participant);
        }

        public void OnRemoteCameraStateUpdated(RemoteCamera remoteCamera, Participant participant, Device.DeviceState state)
        {
            ConferenceViewModel.OnRemoteCameraStateUpdated(remoteCamera, participant, state);
        }
    }
}
