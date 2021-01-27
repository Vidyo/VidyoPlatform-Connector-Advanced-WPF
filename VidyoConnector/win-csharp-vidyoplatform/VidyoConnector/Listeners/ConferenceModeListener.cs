using System;
using System.Reflection;
using System.Windows;
using VidyoClient;
using VidyoConnector.Listeners;
using VidyoConnector.ViewModel;

namespace VidyoConnector.Listeners
{
    public class ConferenceModeListener : ListenerBase, Connector.IRegisterConferenceModeEventListener
    {
        public ConferenceModeListener(VidyoConnectorViewModel viewModel) : base(viewModel) { }

        void Connector.IRegisterConferenceModeEventListener.OnConferenceModeChanged(Connector.ConnectorConferenceMode mode)
        {
            ViewModel.OnConferenceModeChanged(mode);
        }
    }
}