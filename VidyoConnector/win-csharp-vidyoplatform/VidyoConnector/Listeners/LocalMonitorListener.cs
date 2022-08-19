using VidyoClient;
using VidyoConnector.Model;
using VidyoConnector.ViewModel;

namespace VidyoConnector.Listeners
{
    public class LocalMonitorListener : ListenerBase, Connector.IRegisterLocalMonitorEventListener
    {
        public LocalMonitorListener(VidyoConnectorShareViewModel viewModel) : base(viewModel) { }

        public void OnLocalMonitorAdded(LocalMonitor localMonitor)
        {
            if (!string.IsNullOrEmpty(localMonitor.GetName()))
            {
                SharingViewModel.AddLocalMonitor(new LocalMonitorModel(localMonitor));
            }
        }

        public void OnLocalMonitorRemoved(LocalMonitor localMonitor)
        {
            SharingViewModel.RemoveLocalMonitor(new LocalMonitorModel(localMonitor));
        }

        public void OnLocalMonitorSelected(LocalMonitor localMonitor)
        {
            SharingViewModel.SetSelectedLocalMonitor(new LocalMonitorModel(localMonitor));
        }

        public void OnLocalMonitorStateUpdated(LocalMonitor localMonitor, Device.DeviceState state)
        {
            SharingViewModel.Log.Info(string.Format("{0}({1}) state updated: {2}", localMonitor.GetName(), localMonitor.GetId(), state.ToString()));
        }
    }
}