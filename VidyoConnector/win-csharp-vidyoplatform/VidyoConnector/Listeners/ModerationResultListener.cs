using System;
using System.Reflection;
using System.Windows;
using VidyoClient;
using VidyoConnector.Listeners;
using VidyoConferenceModeration.ViewModel;


namespace VidyoConferenceModeration.Listeners
{ 
    public class ModerationResultListener : ListenerBase, Connector.IRegisterModerationResultEventListener,
        Connector.ILockRoom, Connector.IUnlockRoom, Connector.IRemoveModeratorRole, Connector.IRequestModeratorRole,
        Connector.IStartRecording, Connector.IStopRecording, Connector.IPauseRecording, Connector.IResumeRecording,
        Connector.ISetRoomPIN, Connector.IRemoveRoomPIN
    {
        public ModerationResultListener(VidyoConferenceModerationViewModel viewModel) : base(viewModel) { }

        public void OnModerationResult(Participant participant, Connector.ConnectorModerationResult result, Connector.ConnectorModerationActionType action, string requestId)
        {
            ConferenceViewModel.OnModerationResult(participant, result, action, requestId);
        }

        public void OnUnlockRoomResult(Connector.ConnectorModerationResult result)
        {
            ConferenceViewModel.OnUnlockRoomResult(result);
        }

        public void OnLockRoomResult(Connector.ConnectorModerationResult result)
        {
            ConferenceViewModel.OnLockRoomResult(result);
        }

        public void OnRemoveModeratorRoleResult(Connector.ConnectorModerationResult result)
        {
            ConferenceViewModel.OnRemoveModeratorRoleResult(result);
        }

        public void OnRequestModeratorRoleResult(Connector.ConnectorModerationResult result)
        {
            ConferenceViewModel.OnRequestModeratorRoleResult(result);
        }

        public void OnRecordingServiceStartResult(Connector.ConnectorModerationResult result)
        {
            ConferenceViewModel.OnRecordingServiceStartResult(result);
        }

        public void OnRecordingServiceStopResult(Connector.ConnectorModerationResult result)
        {
            ConferenceViewModel.OnRecordingServiceStopResult(result);
        }

        public void OnRecordingServicePauseResult(Connector.ConnectorModerationResult result)
        {
            ConferenceViewModel.OnRecordingServicePauseResult(result);
        }

        public void OnRecordingServiceResumeResult(Connector.ConnectorModerationResult result)
        {
            ConferenceViewModel.OnRecordingServiceResumeResult(result);
        }

        public void OnSetRoomPINResult(Connector.ConnectorModerationResult result)
        {
            ConferenceViewModel.OnSetRoomPINResult(result);
        }

        public void OnRemoveRoomPINResult(Connector.ConnectorModerationResult result)
        {
            ConferenceViewModel.OnRemoveRoomPINResult(result);
        }
    }
}

