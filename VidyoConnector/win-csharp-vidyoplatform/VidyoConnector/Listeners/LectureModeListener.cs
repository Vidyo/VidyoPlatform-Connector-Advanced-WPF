using System;
using System.Reflection;
using System.Windows;
using VidyoClient;
using VidyoConnector.Listeners;
using VidyoConferenceModeration.ViewModel;
using System.Collections.Generic;
using static VidyoClient.Connector;
using VidyoConnector.ViewModel;

namespace VidyoConnector.Listeners
{
    class LectureModeListener : ListenerBase, IRegisterLectureModeEventListener
    {
        public LectureModeListener(VidyoConnectorViewModel viewModel) : base(viewModel) { }

        public void OnHandRaised(List<Participant> participant)
        {
            ViewModel.OnHandRaised(participant);
        }

        public void OnPresenterChanged(Participant participant)
        {
            ViewModel.OnPresenterChanged(participant);
        }
    }
}