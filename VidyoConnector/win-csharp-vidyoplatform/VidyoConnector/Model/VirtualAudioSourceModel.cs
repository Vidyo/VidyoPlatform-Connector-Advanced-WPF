using VidyoClient;

namespace VidyoConnector.Model
{

    public class VirtualAudioSourceModel : DeviceModelBase
    {
        public VirtualAudioSourceModel(VirtualAudioSource source)
        {
            Object = source;
            if (source != null) {
                _displayName = source.GetName();
                _id = source.GetId();
            }
        }

        public VirtualAudioSource Object { get; private set; }

        private bool _isStreamingAudio;
        /// <summary>
        /// Indicates whether this microphone is currently streaming audio.
        /// </summary>
        public bool IsStreamingAudio
        {
            get { return _isStreamingAudio; }
            set
            {
                _isStreamingAudio = value;
                OnPropertyChanged();
                OnPropertyChanged("CanShareContent");
            }
        }

        private bool _isSharingContent;
        /// <summary>
        /// Indicates whether this microphone is currently sharing audio content.
        /// </summary>
        public bool IsSharingContent
        {
            get { return _isSharingContent; }
            set
            {
                _isSharingContent = value;
                OnPropertyChanged();
                OnPropertyChanged("CanStreamAudio");
            }
        }

        /// <summary>
        /// Indicates whether this microphone is available for audio streaming.
        /// microphone is available to stream audio if it is not sharing audio content OR selected microphone is 'NONE'.
        /// </summary>
        public bool CanStreamAudio { get { return !IsSharingContent || Object == null; } }

        /// <summary>
        /// Indicates whether this microphone is available for sharing audio content.
        /// Microphone is available to share content if it is not streaming audio OR selected microphone is 'NONE'.
        /// </summary>
        public bool CanShareContent { get { return !IsStreamingAudio || Object == null; } }
    }
}
