using VidyoClient;
using VidyoConnector.Model;
using VidyoConnector.ViewModel;

namespace VidyoConnector.Listeners
{
    public class VirtualAudioSourceListener : ListenerBase, Connector.IRegisterVirtualAudioSourceEventListener
    {
        public VirtualAudioSourceListener(VidyoConnectorViewModel viewModel) : base(viewModel) {}

        void Connector.IRegisterVirtualAudioSourceEventListener.OnVirtualAudioSourceAdded(VirtualAudioSource virtualAudioSource)
        {
            ViewModel.AddVirtualAudioSource(new VirtualAudioSourceModel(virtualAudioSource));
        }

        void Connector.IRegisterVirtualAudioSourceEventListener.OnVirtualAudioSourceRemoved(VirtualAudioSource virtualAudioSource)
        {
            ViewModel.RemoveVirtualAudioSource(new VirtualAudioSourceModel(virtualAudioSource));
        }

        void Connector.IRegisterVirtualAudioSourceEventListener.OnVirtualAudioSourceStateUpdated(VirtualAudioSource virtualAudioSource, Device.DeviceState state)
        {
            ViewModel.OnVirtualAudioSourceStateUpdated(new VirtualAudioSourceModel(virtualAudioSource), state);
        }

        void Connector.IRegisterVirtualAudioSourceEventListener.OnVirtualAudioSourceSelected(VirtualAudioSource virtualAudioSource)
        {
            ViewModel.SetSelectedVirtualAudioSource(new VirtualAudioSourceModel(virtualAudioSource));
        }

        void Connector.IRegisterVirtualAudioSourceEventListener.OnVirtualAudioSourceExternalMediaBufferReleased(VirtualAudioSource virtualAudioSource, byte[] buffer, SizeT size)
        {
            ViewModel.ReleasedVirtualAudioSourcExternalMediaBuffer(new VirtualAudioSourceModel(virtualAudioSource), buffer, size);
        }
    }
}
