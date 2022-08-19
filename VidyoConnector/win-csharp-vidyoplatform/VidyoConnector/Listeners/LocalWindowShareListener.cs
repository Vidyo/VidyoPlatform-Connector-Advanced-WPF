using System.Linq;
using VidyoClient;
using VidyoConnector.Model;
using VidyoConnector.ViewModel;

namespace VidyoConnector.Listeners
{
    public class LocalWindowShareListener : ListenerBase, Connector.IRegisterLocalWindowShareEventListener
    {
        public LocalWindowShareListener(VidyoConnectorShareViewModel viewModel) : base(viewModel) { }
        public void OnLocalWindowShareAdded(LocalWindowShare localWindowShare)
        {
            if (!string.IsNullOrEmpty(localWindowShare.GetApplicationName()))
            {
                SharingViewModel.AddLocalWindow(new LocalWindowShareModel(localWindowShare));
            }
        }

        public void OnLocalWindowShareRemoved(LocalWindowShare localWindowShare)
        {
            SharingViewModel.RemoveLocalWindow(new LocalWindowShareModel(localWindowShare));
        }

        public void OnLocalWindowShareSelected(LocalWindowShare localWindowShare)
        {
            SharingViewModel.SetSelectedLocalWindow(new LocalWindowShareModel(localWindowShare));
        }

        public void OnLocalWindowShareStateUpdated(LocalWindowShare localWindowShare, Device.DeviceState state)
        {
            LocalWindowShareModel winToUpdate = SharingViewModel.LocalWindows.FirstOrDefault(x => x.Id.Equals(localWindowShare.GetId()));
            if (winToUpdate != null)
            {
                SharingViewModel.UpdateLocalWindowState(winToUpdate);
            }

            SharingViewModel.Log.Info(string.Format("{0}({1}) state updated: {2}", localWindowShare.GetName(), localWindowShare.GetId(), state.ToString()));
        }

        
    }
}