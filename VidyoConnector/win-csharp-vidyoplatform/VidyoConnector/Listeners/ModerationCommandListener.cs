using System;
using System.Collections.Generic;
using VidyoClient;
using VidyoConnector.ViewModel;

namespace VidyoConnector.Listeners
{
    class ModerationCommandListener : ListenerBase, Connector.IRegisterModerationCommandEventListener
    {
        public ModerationCommandListener(VidyoConnectorViewModel viewModel) : base(viewModel) { }

        public void OnModerationCommandReceived(Device.DeviceType deviceType, Room.RoomModerationType moderationType, bool state)
        {
            ViewModel.OnModerationCommandReceived(deviceType, moderationType, state);
        }
    }
}
