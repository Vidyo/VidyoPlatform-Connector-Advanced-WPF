using VidyoClient;

namespace VidyoConnector.Model
{
    public class LocalMonitorModel : DeviceModelBase
    {
        public LocalMonitorModel(LocalMonitor monitor)
        {
            Object = monitor;
            if (monitor != null) {
                _displayName = monitor.GetName();
                _id = monitor.GetId();
            }
        }

        public LocalMonitor Object { get; private set; }

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
    }
}