using VidyoClient;
using static VidyoClient.LocalWindowShare;

namespace VidyoConnector.Model
{
    public class LocalWindowShareModel : DeviceModelBase
    {
        public LocalWindowShareModel(LocalWindowShare window)
        {
            Object = window;
            if (window != null) {
                _displayName = window.GetName();
                _id = window.GetId();
            }
        }

        public LocalWindowShare Object { get; private set; }

        private bool _isSelected;
        /// <summary>
        /// Indicates whether specific resource is selected for using.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates whether a window share window is minimized.
        /// </summary>
        public bool IsMinimized
        {
            get
            {
                return Object.GetWindowState() == LocalWindowShareState.LocalwindowsharestateMinimized;
            }
        }
    }
}