using VidyoClient;

namespace VidyoConnector.Model
{
    public class LocalSpeakerModel : DeviceModelBase
    {
        public LocalSpeakerModel(LocalSpeaker speaker)
        {
            Object = speaker;
            if (speaker != null) {
                _displayName = speaker.GetName();
                _id = speaker.GetId();
            }
        }

        public LocalSpeaker Object { get; private set; }

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