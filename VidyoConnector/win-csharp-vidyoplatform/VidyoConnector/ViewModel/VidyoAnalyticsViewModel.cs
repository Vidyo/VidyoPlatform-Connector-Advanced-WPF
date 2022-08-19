using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VidyoAnalytics.Listeners;
using VidyoAnalytics.ViewModel;
using VidyoClient;
using VidyoConnector.Commands;
using VidyoConnector.Listeners;
using VidyoConnector.ViewModel;
using static VidyoClient.Connector;

namespace VidyoAnalytics.Listeners
{
    class AnalyticsListener : ListenerBase, IGetGoogleAnalyticsEventTable
    {
        public AnalyticsListener(VidyoAnalyticsViewModel viewModel) : base(viewModel) { }

        public void OnGetGoogleAnalyticsEventTable(List<ConnectorGoogleAnalyticsEventTable> eventTable)
        {
            AnalyticsViewModel.OnGetGoogleAnalyticsEventTable(eventTable);
        }
    }
}

namespace VidyoAnalytics.ViewModel
{
    public class EventActionItem : VidyoConnectorViewModel, INotifyPropertyChanged
    {
        public ConnectorGoogleAnalyticsEventCategory EventCategory { get; set; }
        public string EventCategoryDescription { get; set; }

        public ConnectorGoogleAnalyticsEventAction EventAction { get; set; }
        public string EventActionDescription { get; set; }

        private bool _actionStatus;
        public bool ActionStatus
        {
            get { return _actionStatus; }
            set
            {
                _actionStatus = value;
                GetConnectorInstance.GoogleAnalyticsControlEventAction(GetEventCategoryFromEventAction(this.EventAction), this.EventAction, value);
                OnPropertyChanged();
            }
        }

        public EventActionItem(ConnectorGoogleAnalyticsEventCategory eventCategory, string eventCategoryDescription, ConnectorGoogleAnalyticsEventAction eventAction, 
            string eventActionDescription, bool enable)
        {
            this.EventCategory = eventCategory;
            this.EventCategoryDescription = eventCategoryDescription;
            this.EventAction = eventAction;
            this.EventActionDescription = eventActionDescription;
            this.ActionStatus = enable;
        }

        public void EventActionItem_SetAction(bool enable)
        {
            this.ActionStatus = enable;
        }

        private ConnectorGoogleAnalyticsEventCategory GetEventCategoryFromEventAction(ConnectorGoogleAnalyticsEventAction eventAction)
        {
            switch (eventAction)
            {
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUserTypeGuest:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUserTypeRegularExtdata:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUserTypeRegularPassword:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUserTypeRegularSaml:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUserTypeRegularToken:
                    return ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryUserType;

                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginAttempt:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedAuthentication:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedConnect:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedMiscError:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedResponseTimeout:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedUnsupportedTenantVersion:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedWebProxyAuthRequired:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginSuccess:
                    return ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryLogin;

                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceAttempt:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedConferenceLocked:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedConnectionError:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedRoomDisabled:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedRoomFull:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedUnknownError:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedWrongPin:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceReconnectRequests:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceSuccess:
                    return ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryJoinConference;

                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionConferenceEndBooted:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionConferenceEndLeft:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionConferenceEndMediaConnectionLost:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionConferenceEndSignalingConnectionLost:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionConferenceEndUnknownError:
                    return ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryConferenceEnd;

                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionInCallCodecAudioSPEEXRED:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionInCallCodecVideoH264:
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionInCallCodecVideoH264SVC:
                    return ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryInCallCodec;

                default:
                    return ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryNone;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VidyoAnalyticsViewModel : VidyoConnectorViewModel, INotifyPropertyChanged
    {
        bool isGoogleServiceStarted;
        bool isInsightsServiceStarted;

        public VidyoAnalyticsViewModel()
        { 
            EventActionItemList = new ObservableCollection<EventActionItem>();
            isGoogleServiceStarted = false;
            isInsightsServiceStarted = false;
            IsServerUrlBoxEnabled = IsInsightsStartButtonEnabled = true;
            IsInsightsStopButtonEnabled = false;
            IsTrackingIdBoxEnabled = IsGoogleAnalyticsStartButtonEnabled = true;
            IsGoogleAnalyticsStopButtonEnabled = false;
        }

        public void Init()
        {
            EventActionItemList.Clear();
            GetConnectorInstance.GetGoogleAnalyticsEventTable(new AnalyticsListener(this));

            IsGoogleAnalyticsStartButtonEnabled = !GetConnectorInstance.IsGoogleAnalyticsServiceEnabled();
            IsGoogleAnalyticsStopButtonEnabled = !IsGoogleAnalyticsStartButtonEnabled;
            if (!IsGoogleAnalyticsStartButtonEnabled)
            {
                TrackingIdText = GetConnectorInstance.GetGoogleAnalyticsServiceID();               
            }

            IsInsightsStartButtonEnabled = !GetConnectorInstance.IsInsightsServiceEnabled();
            IsInsightsStopButtonEnabled = !IsInsightsStartButtonEnabled;
            if (!IsInsightsStartButtonEnabled)
            {
                ServerlUrlText = GetConnectorInstance.GetInsightsServiceUrl();
            }
        }

        public string ConnectorAnalyticsEventCategoryToString(ConnectorGoogleAnalyticsEventCategory eventCategory)
        {
            switch (eventCategory)
            {
                case ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryLogin:
                    return "Login";
                case ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryUserType:
                    return "User Type";
                case ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryJoinConference:
                    return "Join Conference";
                case ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryConferenceEnd:
                    return "End Conference";
                case ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryInCallCodec:
                    return "In Call Codec";
                case ConnectorGoogleAnalyticsEventCategory.ConnectorgoogleanalyticseventcategoryNone:
                default:
                    return "UnKnown";
            }
        }

        public string ConnectorAnalyticsEventActionToString(ConnectorGoogleAnalyticsEventAction eventAction)
        {
            switch (eventAction)
            {
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginSuccess:
                    return "Success";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginAttempt:
                    return "Attempt";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedAuthentication:
                    return "Failed Authentication";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedConnect:
                    return "Failed Connect";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedResponseTimeout:
                    return "Failed Response Timeout";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedMiscError:
                    return "Failed Misc Error";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedWebProxyAuthRequired:
                    return "Failed WebProxy AuthRequired";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionLoginFailedUnsupportedTenantVersion:
                    return "Failed Unsupported Tenant Version";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUserTypeGuest:
                    return "Guest";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUserTypeRegularToken:
                    return "Regular Token";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUserTypeRegularPassword:
                    return "Regular Password";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUserTypeRegularSaml:
                    return "Regular Saml";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUserTypeRegularExtdata:
                    return "Regular Extdata";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceSuccess:
                    return "Success";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceAttempt:
                    return "Attempt";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceReconnectRequests:
                    return "Reconnect Requests";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedConnectionError:
                    return "Failed Connection Error";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedWrongPin:
                    return "Failed Wrong Pin";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedRoomFull:
                    return "Failed Room Full";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedRoomDisabled:
                    return "Failed Room Disabled";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedConferenceLocked:
                    return "Failed Conference Locked";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionJoinConferenceFailedUnknownError:
                    return "Failed Unknown Error";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionConferenceEndLeft:
                    return "Left";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionConferenceEndBooted:
                    return "Booted";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionConferenceEndSignalingConnectionLost:
                    return "Connection Lost";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionConferenceEndMediaConnectionLost:
                    return "Media Connection Lost";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionConferenceEndUnknownError:
                    return "Unknown Error";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionInCallCodecAudioSPEEXRED:
                    return "Audio-Speex Red";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionInCallCodecVideoH264:
                    return "Video-H264";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionInCallCodecVideoH264SVC:
                    return "Video-H264 SVC";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionAll:
                    return "All";
                case ConnectorGoogleAnalyticsEventAction.ConnectorgoogleanalyticseventactionUnknown:
                default:
                    return "UnKnown";
            }
        }

        public void StartGoogleAnalyticsService()
        {
            if (GetConnectorInstance.StartGoogleAnalyticsService(String.IsNullOrEmpty(TrackingIdText) ? System.Configuration.ConfigurationManager.AppSettings["GoogleAnalyticId"] : TrackingIdText))
            {
                IsTrackingIdBoxEnabled = IsGoogleAnalyticsStartButtonEnabled = false;
                isGoogleServiceStarted = IsGoogleAnalyticsStopButtonEnabled = true;
            }
            else
            {
                MessageBox.Show("Failed to start Google Analytics service.", "Vidyo Analytics");
            }
        }

        public void StopGoogleAnalyticsService()
        {
            if(GetConnectorInstance.StopGoogleAnalyticsService())
            {
                IsTrackingIdBoxEnabled = IsGoogleAnalyticsStartButtonEnabled = true;
                isGoogleServiceStarted = IsGoogleAnalyticsStopButtonEnabled = false;
            }
            else
            {
                MessageBox.Show("Failed to stop Google Analytics service.", "Vidyo Analytics");
            }
        }

        public void StartInsightsService()
        {
            if(GetConnectorInstance.StartInsightsService(ServerlUrlText))
            {
                IsServerUrlBoxEnabled = IsInsightsStartButtonEnabled = false;
                isInsightsServiceStarted = IsInsightsStopButtonEnabled = true;
            }
            else
            {
                MessageBox.Show("Failed to start Insights service.", "Vidyo Analytics");
            }
        }

        public void StopInsightsService()
        {
            if(GetConnectorInstance.StopInsightsService())
            {
                IsServerUrlBoxEnabled = IsInsightsStartButtonEnabled = true;
                isInsightsServiceStarted = IsInsightsStopButtonEnabled = false;
            }
            else
            {
                MessageBox.Show("Failed to stop Insights service.", "Vidyo Analytics");
            }
        }

        public void GetAnalyticsEventTable(List<ConnectorGoogleAnalyticsEventTable> eventActions)
        {
            for (int iCounter = 0; iCounter < eventActions.Count(); iCounter++)
            {
                EventActionItemList.Add(new EventActionItem(eventActions[iCounter].eventCategory, 
                    ConnectorAnalyticsEventCategoryToString(eventActions[iCounter].eventCategory),
                    eventActions[iCounter].eventAction,
                    ConnectorAnalyticsEventActionToString(eventActions[iCounter].eventAction), eventActions[iCounter].enable));
            }
        }

        public void OnGetGoogleAnalyticsEventTable(List<ConnectorGoogleAnalyticsEventTable> eventActions)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                GetAnalyticsEventTable(eventActions)));
        }

        private bool _isTrackingIdBoxEnabled;
        public bool IsTrackingIdBoxEnabled
        {
            get { return _isTrackingIdBoxEnabled; }
            set { _isTrackingIdBoxEnabled = value; OnPropertyChanged(); }
        }
        private bool _isServerUrlBoxEnabled;
        public bool IsServerUrlBoxEnabled
        {
            get { return _isServerUrlBoxEnabled; }
            set { _isServerUrlBoxEnabled = value; OnPropertyChanged(); }
        }

        private bool _isGoogleAnalyticsStartButtonEnabled;
        public bool IsGoogleAnalyticsStartButtonEnabled
        {
            get { return _isGoogleAnalyticsStartButtonEnabled; }
            set { _isGoogleAnalyticsStartButtonEnabled = value; OnPropertyChanged(); }
        }

        private bool _isGoogleAnalyticsStopButtonEnabled;
        public bool IsGoogleAnalyticsStopButtonEnabled
        {
            get { return _isGoogleAnalyticsStopButtonEnabled; }
            set { _isGoogleAnalyticsStopButtonEnabled = value; OnPropertyChanged(); }
        }

        private bool _isInsightsStartButtonEnabled;
        public bool IsInsightsStartButtonEnabled
        {
            get { return _isInsightsStartButtonEnabled; }
            set { _isInsightsStartButtonEnabled = value; OnPropertyChanged(); }
        }

        private bool _isInsightsStopButtonEnabled;
        public bool IsInsightsStopButtonEnabled
        {
            get { return _isInsightsStopButtonEnabled; }
            set { _isInsightsStopButtonEnabled = value; OnPropertyChanged(); }
        }

        private string _trackingIdText;
        public string TrackingIdText
        {
            get { return _trackingIdText; }
            set
            {
                _trackingIdText = value;
                OnPropertyChanged();
            }
        }

        private string _serverlUrlText;
        public string ServerlUrlText
        {
            get { return _serverlUrlText; }
            set
            {
                _serverlUrlText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<EventActionItem> EventActionItemList { get; set; }

        private ICommand GetCommand(ref ICommand command, Action<object> action, bool isCanExecute = true)
        {
            if (command != null) return command;

            var cmd = new BindableCommand { IsCanExecute = isCanExecute };
            cmd.ExecuteAction += action;
            command = cmd;
            return command;
        }
    }
}
