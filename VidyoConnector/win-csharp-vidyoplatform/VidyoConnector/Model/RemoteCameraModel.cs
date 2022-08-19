using System;
using System.Collections.Generic;
using VidyoClient;
using VidyoConferenceModeration.Listeners;
using VidyoConferenceModeration.ViewModel;
using VidyoConnector.Listeners;
using VidyoConnector.ViewModel;
using static VidyoClient.RemoteCamera;

namespace VidyoConnector.Model
{
    class RemoteCameraPresetListner : ListenerBase, IRegisterPresetEventListener
    {
        public RemoteCameraPresetListner(VidyoConnector.Model.RemoteCameraModel viewModel) : base(viewModel) { }

        public void OnPresetUpdated(List<CameraPreset> presets)
        {
            RemoteCameraModel.SetPresetData(presets);
        }
    }

    public class RemoteCameraModel : DeviceModelBase
    {
        private List<CameraPreset> _presets;

        public RemoteCameraModel(RemoteCamera camera)
        {
            camera.RegisterPresetEventListener(new RemoteCameraPresetListner(this));
            Object = camera;
        }

        public RemoteCamera Object { get; private set; }

        public void SetPresetData(List<CameraPreset> presets)
        {
            _presets = presets;
        }

        public List<CameraPreset> GetPresetData()
        {
            return _presets;
        }

        public bool isPresetAvailable()
        {
            return _presets.Count == 0 ? false : true;
        }

        public bool RemoteCamera_ActivatePreset(uint index)
        {
            return Object.ActivatePreset(index);
        }

        public bool RemoteCamera_SendViscaCommand(string command, string commandId)
        {
            return Object.ViscaControl(command,commandId);
        }
    }
}