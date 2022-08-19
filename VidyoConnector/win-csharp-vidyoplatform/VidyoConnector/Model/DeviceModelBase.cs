using System.ComponentModel;
using System.Runtime.CompilerServices;
using VidyoConnector.Annotations;

namespace VidyoConnector.Model
{
    /// <summary>
    /// Describes common members of all resources used by application (devices, monitors, applications, etc.)
    /// </summary>
    public abstract class DeviceModelBase : INotifyPropertyChanged
    {
        public DeviceModelBase()
        {
            _displayName = "None";
            _id = string.Empty;
        }

        protected string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
        }

        protected string _id;
        public string Id
        {
            get { return _id; }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}