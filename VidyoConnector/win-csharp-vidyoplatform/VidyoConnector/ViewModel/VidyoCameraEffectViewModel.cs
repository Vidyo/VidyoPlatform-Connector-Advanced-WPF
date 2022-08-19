using System.Collections.Generic;
using System.IO;
using System.Linq;
using VidyoClient;
using VidyoConnector.ViewModel;
using static VidyoClient.Connector;

namespace VidyoCameraEffect.ViewModel
{
    class VidyoCameraEffectViewModel : VidyoConnectorViewModel, IGetCameraBackgroundEffect
    {
        private void setCameraEffectparameters(ConnectorCameraEffectInfo info)
        {
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
            info.pathToResources = System.IO.Path.Combine(strWorkPath, "bnb-resources");
            info.token = VidyoCameraEffect.BnbLicenseToken.token;
            switch (info.effectType)
            {
                case ConnectorCameraEffectType.ConnectorcameraeffecttypeBlur:
                    info.pathToEffect = System.IO.Path.Combine(strWorkPath, "effects", "blurred-background");
                    break;
                case ConnectorCameraEffectType.ConnectorcameraeffecttypeVirtualBackground:
                    info.pathToEffect = System.IO.Path.Combine(strWorkPath, "effects", "virtual-background");
                    break;
            }
        }

        public void NoneBackgroundSelect()
        {
            ConnectorCameraEffectInfo info = ConnectorCameraEffectInfoFactory.Create();
            bool ret = GetConnectorInstance.SetCameraBackgroundEffect(info);
            if (ret)
                type_ = ConnectorCameraEffectType.ConnectorcameraeffecttypeNone;

            Log.Info(string.Format("set None background effect return = {0}", ret));
        }

        public void BlurBackgroundSelect(uint blurIntensity)
        {
            if (IsBlurSelected)
            {
                GetConnectorInstance.SetBlurIntensity(blurIntensity);
                return;
            }

            ConnectorCameraEffectInfo info = ConnectorCameraEffectInfoFactory.Create();
            info.effectType = ConnectorCameraEffectType.ConnectorcameraeffecttypeBlur;
            info.blurIntensity = blurIntensity;
            this.setCameraEffectparameters(info);
            bool ret = GetConnectorInstance.SetCameraBackgroundEffect(info);
            if (ret)
                type_ = ConnectorCameraEffectType.ConnectorcameraeffecttypeBlur;

            Log.Info(string.Format("set camera background effect return = {0}", ret));
        }

        public void VirtualBackgroundSelect(string background)
        {
            if (IsVirtualBackground)
            {
                GetConnectorInstance.SetVirtualBackgroundPicture(background);
                return;
            }

            ConnectorCameraEffectInfo info = ConnectorCameraEffectInfoFactory.Create();
            info.effectType = ConnectorCameraEffectType.ConnectorcameraeffecttypeVirtualBackground;
            info.virtualBackgroundPicture = background;
            this.setCameraEffectparameters(info);
            bool ret = GetConnectorInstance.SetCameraBackgroundEffect(info);
            if (ret)
                type_ = ConnectorCameraEffectType.ConnectorcameraeffecttypeVirtualBackground;

            Log.Info(string.Format("set virtual background background effect return = {0}", ret));
        }

        public void Init()
        {
            bool r = GetConnectorInstance.GetCameraBackgroundEffect(this);
        }

        private ConnectorCameraEffectType type_ = ConnectorCameraEffectType.ConnectorcameraeffecttypeNone;

        public bool IsNone
        {
            get { return type_ == ConnectorCameraEffectType.ConnectorcameraeffecttypeNone; }
            set { if (value)
                    type_ = ConnectorCameraEffectType.ConnectorcameraeffecttypeNone; OnPropertyChanged();
            }
        }

        public bool IsBlurSelected
        {
            get { return type_ == ConnectorCameraEffectType.ConnectorcameraeffecttypeBlur; }
            set
            {
                if (value)
                    type_ = ConnectorCameraEffectType.ConnectorcameraeffecttypeBlur; OnPropertyChanged();
            }
        }

        public bool IsVirtualBackground
        {
            get { return type_ == ConnectorCameraEffectType.ConnectorcameraeffecttypeVirtualBackground; }
            set
            {
                if (value)
                    type_ = ConnectorCameraEffectType.ConnectorcameraeffecttypeVirtualBackground; OnPropertyChanged();
            }
        }

        public List<string> GetAvailableVirtualBackgrounds()
        {
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = Path.GetDirectoryName(strExeFilePath);
            string pathToResources = Path.Combine(strWorkPath, "resources", "VirtualBackgrounds");

            List<string> virtualBackgroundImages = Directory.EnumerateFiles(pathToResources, "*", SearchOption.TopDirectoryOnly).Where(p => p.EndsWith(".jpg") || p.EndsWith(".png")).ToList();
            return virtualBackgroundImages;
        }

        void IGetCameraBackgroundEffect.OnGetCameraBackgroundEffectInfo(ConnectorCameraEffectInfo effectInfo)
        {
            switch (effectInfo.effectType)
            {
                case ConnectorCameraEffectType.ConnectorcameraeffecttypeBlur:
                    this.IsBlurSelected = true;
                    break;
                case ConnectorCameraEffectType.ConnectorcameraeffecttypeNone:
                    this.IsNone = true;
                    break;
                case ConnectorCameraEffectType.ConnectorcameraeffecttypeVirtualBackground:
                    this.IsVirtualBackground = true;
                    break;
            }
        }
    }
}
