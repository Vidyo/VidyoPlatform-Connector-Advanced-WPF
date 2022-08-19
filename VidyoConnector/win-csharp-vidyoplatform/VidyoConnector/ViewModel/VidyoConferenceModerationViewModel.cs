using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Input;
using VidyoClient;
using VidyoConnector.Commands;
using VidyoConnector.Listeners;
using VidyoConnector.ViewModel;
using static VidyoClient.Connector;
using VidyoConferenceModeration.Listeners;
using System.Windows.Media;
using VidyoConnector;
using System.Data;
using SearchUsersDialog;
using static VidyoClient.Participant;
using VidyoConnector.Model;

namespace VidyoConferenceModeration.ViewModel
{
    public enum ParticipantCommandType
    {
        SendPrivateMsgCommand,
        HandCommand,
        MicCommand,
        CameraCommand,
        CameraPresetCommand,
        CameraViscaCommand,
        DisconnectCommand,
        UnknownCommand
    }
    public enum UserLoginType
    {
        AsUser,
        AsGuest
    }

    public class RecordingItemElemt
    {
        public string RecordingProfileDescription { get; set; }
    }

    public class ParticipantItemElemt : INotifyPropertyChanged
    {
        public string ParticipantName { get; set; }
        public string ParticipantUserId { get; set; }
        public string ParticipantType { get; set; }

        public bool ParticipantMicStatus { get; set; }
        public bool ParticipantCameraStatus { get; set; }

        public bool ParticipantIsPresenter { get; set; }
        public bool ParticipantHandStatus { get; set; }
        public bool ParticipantHardMuteUnmuteMicStatus { get; set; }
        public bool ParticipantHardMuteUnmuteCameraStatus { get; set; }
        public bool ParticipantConnectStatus { get; set; }
        public RemoteCameraModel Camera { get; set; }
        public bool ParticipantCameraPresetStatus { get; set; }
        public string ParticipantCameraPresetAvailable { get; set; }
        public bool ParticipantCameraViscaStatus { get; set; }
        public string ParticipantCameraViscaSupported { get; set; }
        public ParticipantItemElemt(string participantName, string participantUserId, string participantType,
            bool participantMicStatus, bool participantCameraStatus,
            bool participantIsPresenter, bool participantHandStatus, bool participantHardMuteUnmuteMicStatus,
            bool participantHardMuteUnmuteCameraStatus, bool participantConnectStatus)
        {
            this.ParticipantName = participantName;
            this.ParticipantUserId = participantUserId;
            this.ParticipantType = participantType;

            this.ParticipantMicStatus = participantMicStatus;
            this.ParticipantCameraStatus = participantCameraStatus;

            this.ParticipantIsPresenter = participantIsPresenter;
            this.ParticipantHandStatus = participantHandStatus;
            this.ParticipantHardMuteUnmuteMicStatus = participantHardMuteUnmuteMicStatus;
            this.ParticipantHardMuteUnmuteCameraStatus = participantHardMuteUnmuteCameraStatus;
            this.ParticipantConnectStatus = participantConnectStatus;
            this.Camera = null;
            this.ParticipantCameraPresetStatus = false;
            this.ParticipantCameraPresetAvailable = String.Empty;
            this.ParticipantCameraViscaStatus = false;
            this.ParticipantCameraViscaSupported = String.Empty;
        }

        public void ParticipantItemElemt_SetMicStatus(bool micStatus)
        {
            this.ParticipantMicStatus = micStatus;
            NotifyPropertyChanged("ParticipantMicStatus");
        }

        public void ParticipantItemElemt_SetCameraStatus(bool cameraStatus)
        {
            this.ParticipantCameraStatus = cameraStatus;
            NotifyPropertyChanged("ParticipantCameraStatus");
        }

        public void ParticipantItemElemt_SetPresenter(bool isPresenter)
        {
            this.ParticipantIsPresenter = isPresenter;
            NotifyPropertyChanged("ParticipantIsPresenter");
        }

        public void ParticipantItemElemt_SetHandStatus(bool handStatus)
        {
            this.ParticipantHandStatus = handStatus;
            NotifyPropertyChanged("ParticipantHandStatus");
        }

        public void ParticipantItemElemt_SetHardMuteUnmuteMicStatus(bool micStatus)
        {
            this.ParticipantHardMuteUnmuteMicStatus = micStatus;
            NotifyPropertyChanged("ParticipantHardMuteUnmuteMicStatus");
        }

        public void ParticipantItemElemt_SetHardMuteUnmuteCameraStatus(bool cameraStatus)
        {
            this.ParticipantHardMuteUnmuteCameraStatus = cameraStatus;
            NotifyPropertyChanged("ParticipantHardMuteUnmuteCameraStatus");
        }

        public void ParticipantItemElemt_SetCamera(RemoteCameraModel camera)
        {
            this.Camera = camera;
            NotifyPropertyChanged("Camera");
        }

        public RemoteCameraModel ParticipantItemElemt_GetCamera()
        {
            return this.Camera;
        }

        public void ParticipantItemElemt_SetCameraPresetStatus(bool status)
        {
            this.ParticipantCameraPresetStatus = status;
            NotifyPropertyChanged("ParticipantCameraPresetStatus");
        }

        public void ParticipantItemElemt_SetCameraPresetAvailable(string str)
        {
            this.ParticipantCameraPresetAvailable = str;
            NotifyPropertyChanged("ParticipantCameraPresetAvailable");
        }

        public void ParticipantItemElemt_SetCameraViscaSupport(string str)
        {
            ParticipantCameraViscaStatus = !string.IsNullOrEmpty(str);
            this.ParticipantCameraViscaSupported = str;
            NotifyPropertyChanged("ParticipantCameraViscaSupported");
            NotifyPropertyChanged("ParticipantCameraViscaStatus");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VidyoConferenceModerationViewModel : VidyoConnectorViewModel, INotifyPropertyChanged
    {
        private object DataContext;
        int indexRecordingProfile;
        List<Tuple<int, String, String>> recordingProfileList;
        List <KeyValuePair<String, Participant>> participantList;
        object _itemsLock;
        String presenterUserId;
        bool hasModeratorPin;
        bool hasRoomPin;
        bool localUserMicStatus;
        bool localUserCameraStatus;
        bool localUserHardMicMuteStatus;
        bool localUserHardCameraMuteStatus;
        Participant localParticipantInfo;
        ConferenceModerationWindow confModerationWindow;

        public VidyoConferenceModerationViewModel(object DataContext = null)
        {
            this.DataContext = DataContext;
            _itemsLock = new object();

            RecordingProfileItemList = new ObservableCollection<RecordingItemElemt>();
            ParticipantItemList = new ObservableCollection<ParticipantItemElemt>();
            recordingProfileList = new List<Tuple<int, string, string>>();
            participantList = new List<KeyValuePair<String, Participant>>();

            BindingOperations.EnableCollectionSynchronization(ParticipantItemList, _itemsLock);


            ParticipantCount = "Participants(0)";
            IsRoleNone = true;
            localUserMicStatus = false;
            localUserCameraStatus = false;
            localUserHardMicMuteStatus = false;
            localUserHardCameraMuteStatus = false;
            ParticipantAllHardAudioMuteStatus = false;
            ParticipantAllHardVideoMuteStatus = false;
        }

        public void Init(ConferenceModerationWindow conf)
        {
            GetConnectorInstance.RegisterRemoteCameraEventListener(new RemoteCameraListener(this));
            GetConnectorInstance.RegisterRemoteMicrophoneEventListener(new RemoteMicrophoneListener(this));
            confModerationWindow = conf;
            RecordingProfileItemList.Clear();
            recordingProfileList.Clear();
            if (GetLocalParticipantClearanceType() != ParticipantClearanceType.ParticipantCLEARANCETYPE_None)
            {
                GetConnectorInstance.GetRecordingServiceProfiles(new RecordingProfileListener(this));
            }
            GetConnectorInstance.RegisterModerationResultEventListener(new ModerationResultListener(this));
            confModerationWindow.SetIsModerationPIN(hasModeratorPin);
            confModerationWindow.SetIsRoomPIN(hasRoomPin);
        }

        public void DeInitialize()
        {
            if (GetConnectorInstance != null)
            {
                GetConnectorInstance.UnregisterLectureModeEventListener();
                GetConnectorInstance.UnregisterRemoteCameraEventListener();
                GetConnectorInstance.UnregisterRemoteMicrophoneEventListener();
                GetConnectorInstance.UnregisterModerationResultEventListener();
            }

            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                Clear()));
            }
        }

        public ParticipantClearanceType GetLocalParticipantClearanceType()
        {
            if(localParticipantInfo == null)
                return ParticipantClearanceType.ParticipantCLEARANCETYPE_None;
            return localParticipantInfo.GetClearanceType();
        }

        public String GetLocalParticipantUserId()
        {
            if (localParticipantInfo == null)
                return null;
            return localParticipantInfo.GetUserId();
        }

        public Participant GetLocalParticipantInfo()
        {
            if (localParticipantInfo == null)
                return null;
            return localParticipantInfo;
        }

        public bool GetLocalParticipantHardMicrophoneMuteStatus()
        {
            lock (_itemsLock)
            {
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == GetLocalParticipantUserId());
                if (item != null)
                {
                    return item.ParticipantHardMuteUnmuteMicStatus;
                }
            }
            return false;
        }

        public bool GetLocalParticipantHardCameraMuteStatus()
        {
            lock (_itemsLock)
            {
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == GetLocalParticipantUserId());
                if (item != null)
                {
                    return item.ParticipantHardMuteUnmuteCameraStatus;
                }
            }
            return false;
        }

        public void Clear()
        {
            lock (_itemsLock)
            {
                RecordingProfileItemList.Clear();
                recordingProfileList.Clear();
                ParticipantItemList.Clear();
                participantList.Clear();
            }
        }

        public void SetConnectionState(VidyoConnector.Model.ConnectionState state)
        {
            if(state != VidyoConnector.Model.ConnectionState.Connected)
            {
                IsRoleNone = true;
                IsRoleModerator = false;
                IsRaisedHand = false;
                localUserHardMicMuteStatus = false;
                localUserHardCameraMuteStatus = false;
                ParticipantAllHardAudioMuteStatus = false;
                ParticipantAllHardVideoMuteStatus = false;
                SetControlAccordingToClearance();
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => CloseAction()));
            }
        }

        public void SetLocalMicrophonePrivacy (bool micStatus)
        {
            localUserMicStatus = !micStatus;
        }

        public void SetLocalCameraPrivacy(bool cameraStatus)
        {
            localUserCameraStatus = !cameraStatus;
        }

        public bool IsAllowForModerationOperation()
        {
            ParticipantClearanceType localUserClearanceType = GetLocalParticipantClearanceType();
            if( (IsRoleModerator) || (localUserClearanceType == ParticipantClearanceType.ParticipantCLEARANCETYPE_Admin) ||
                (localUserClearanceType == ParticipantClearanceType.ParticipantCLEARANCETYPE_Moderator) ||
                (localUserClearanceType == ParticipantClearanceType.ParticipantCLEARANCETYPE_Owner))
            {
                return true;
            }
            return false;
        }

        public void SetControlAccordingToClearance()
        {
            bool isEnabled = false;
            ParticipantClearanceType localUserClearanceType = GetLocalParticipantClearanceType();
            if ((IsRoleModerator) || (localUserClearanceType == ParticipantClearanceType.ParticipantCLEARANCETYPE_Admin) ||
                    (localUserClearanceType == ParticipantClearanceType.ParticipantCLEARANCETYPE_Moderator) ||
                    (localUserClearanceType == ParticipantClearanceType.ParticipantCLEARANCETYPE_Owner))
            {
                isEnabled = true;
                ((VidyoConnectorViewModel)DataContext).IsBtnMicrophoneEnabled = isEnabled;
                ((VidyoConnectorViewModel)DataContext).IsBtnCameraEnabled = isEnabled;
            }
            else
            {
                isEnabled = false;
            }
            
            IsTexBoxModeratorPinEnabled = isEnabled;
            IsButtonRemoveModeratorPinEnabled = isEnabled;
            IsButtonSetModeratorPinEnabled = isEnabled;
            IsTextBlockRoomPINEnabled = isEnabled;
            IsButtonRemoveRoomPinEnabled = isEnabled;
            IsButtonSetRoomPinEnabled = isEnabled;

            IsPresenterModeEnabled = isEnabled;
            IsGroupModeEnabled = isEnabled;

            IsLockRoomEnabled = isEnabled;
            IsUnlockRoomEnabled = isEnabled;

            IsImageRecordingStartEnabled = isEnabled;
            IsImageRecordingPausedEnabled = isEnabled;
            IsImageRecordingStopEnabled = isEnabled;
            IsImageRemovePresenterEnabled = isEnabled;
            IsImageDismissAllRaisedHandEnabled = isEnabled;
            IsImageHardMuteAudioAllEnabled = isEnabled;
            IsImageHardMuteVideoAllEnabled = isEnabled;
            IsImageSoftMuteAudioAllEnabled = isEnabled;
            IsImageSoftMuteVideoAllEnabled = isEnabled;
            IsImageDropAllParticipantEnabled = isEnabled;
            IsLabelAddParticipantEnabled = isEnabled;

            if (GetLocalParticipantClearanceType() == ParticipantClearanceType.ParticipantCLEARANCETYPE_Member)
            {
                isEnabled = true;
            }
            IsRoleModeratorEnabled = isEnabled;
            IsRoleNoneEnabled = isEnabled;
        }

        #region General

        public void VidyoConferenceModerationViewModel_SetRecordingProfileIndex(int index)
        {
            indexRecordingProfile = index;
        }

        public void VidyoConferenceModerationViewModel_RaiseUnraiseHand()
        {
            if (!IsRaisedHand)
            {
                if (!GetConnectorInstance.RaiseHand(new HandResponseListener(this), "RaiseHand"))
                {
                    DisplayErrorMessageForAPI("Moderation", "Failed to Raised Hand");
                }
            }
            else
            {
                if (!GetConnectorInstance.UnraiseHand("UnraiseHand"))
                {
                    DisplayErrorMessageForAPI("Moderation", "Failed to Unraise Hand");
                }
            }
        }

        public bool VidyoConferenceModerationViewModel_StartRecording()
        {
            bool retValue = false;

            if(!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Recording Service Start");
                return retValue;
            }

            if(recordingProfileList.Count == 0)
            {
                return retValue;
            }

            if (!IsRecordingStarted)
            {
                if (indexRecordingProfile == -1)
                {
                    retValue = GetConnectorInstance.StartRecording(null, new ModerationResultListener(this));
                }
                else
                {
                    String prefix = recordingProfileList[indexRecordingProfile].Item3;
                    retValue = GetConnectorInstance.StartRecording(prefix, new ModerationResultListener(this));
                }
                if (!retValue)
                {
                    DisplayErrorMessageForAPI("Moderation", "Failed to Start Recording");
                }
            }
            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_PauseResumeRecording()
        {
            bool retValue = false;

            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Recording Service Pause/Resume");
                return retValue;
            }

            if (IsRecordingStarted)
            {
                if (!IsRecordingPaused)
                {
                    retValue = GetConnectorInstance.PauseRecording(new ModerationResultListener(this));
                    if (!retValue)
                    {
                        DisplayErrorMessageForAPI("Moderation", "Failed to Pause Recording");
                    }
                }
                else
                {
                    retValue = GetConnectorInstance.ResumeRecording(new ModerationResultListener(this));
                    if (!retValue)
                    {
                        DisplayErrorMessageForAPI("Moderation", "Failed to Resume Recording");
                    }
                }
            }
            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_StopRecording()
        {
            bool retValue = false;

            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Recording Service Stop");
                return retValue;
            }

            if (IsRecordingStarted)
            {
                retValue = GetConnectorInstance.StopRecording(new ModerationResultListener(this));
                if (!retValue)
                {
                    DisplayErrorMessageForAPI("Moderation", "Failed to Stop Recording");
                }
            }
            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_RemovePresenter()
        {
            bool retValue = false;

            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Remove Presenter");
                return retValue;
            }

            retValue = GetConnectorInstance.RemovePresenter("RemovePresenter");
            if (!retValue)
            {
                DisplayErrorMessageForAPI("Moderation", "Failed to Remove Presenter");
            }
            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_DismissAllRaisedHand()
        {
            bool retValue = false;

            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Dismiss All Raised Hand");
                return retValue;
            }

            retValue = GetConnectorInstance.DismissAllRaisedHands("DismissAllRaisedHand");
            if (!retValue)
            {
                DisplayErrorMessageForAPI("Moderation", "Failed to Dismiss All Raised Hand");
            }
            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_HardMuteAudioAll()
        {
            bool retValue = false;

            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Hard Mute Audio For All");
                return retValue;
            }

            if (!ParticipantAllHardAudioMuteStatus)
            {
                retValue = GetConnectorInstance.DisableAudioForAll(true, "HardMuteAudioForAllEnable");
                if (!retValue)
                {
                    DisplayErrorMessageForAPI("Moderation", "Failed to Enable Hard Mute Audio for All");
                }
            }
            else
            {
                retValue = GetConnectorInstance.DisableAudioForAll(false, "HardMuteAudioForAllDisable");
                if (!retValue)
                {
                    DisplayErrorMessageForAPI("Moderation", "Failed to Disable Hard Mute Audio for All");
                }
            }
            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_HardMuteVideoAll()
        {
            bool retValue = false;

            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Hard Mute Video For All");
                return retValue;
            }

            if (!ParticipantAllHardVideoMuteStatus)
            {
                retValue = GetConnectorInstance.DisableVideoForAll(true, "HardMuteVideoForAllEnable");
                if (!retValue)
                {
                    DisplayErrorMessageForAPI("Moderation", "Failed to Enable Hard Mute Video for All");
                }
            }
            else
            {
                retValue = GetConnectorInstance.DisableVideoForAll(false, "HardMuteVideoForAllDisable");
                if (!retValue)
                {
                    DisplayErrorMessageForAPI("Moderation", "Failed to Disable Hard Mute Video for All");
                }
            }
            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_SoftMuteAudioAll()
        {
            bool retValue = false;

            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Soft Mute Audio For All");
                return retValue;
            }

            retValue = GetConnectorInstance.DisableAudioSilenceForAll("SoftMuteAudioForAll");
            if (!retValue)
            {
                DisplayErrorMessageForAPI("Moderation", "Failed to Soft Mute Audio for All");
            }
            else
            {
                ParticipantAllSoftAudioMuteStatus = true;
            }

            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_SoftMuteVideoAll()
        {
            bool retValue = false;

            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Soft Mute Video For All");
                return retValue;
            }

            retValue = GetConnectorInstance.DisableVideoSilenceForAll("SoftMuteVideoForAll");
            if (!retValue)
            {
                DisplayErrorMessageForAPI("Moderation", "Failed to Soft Mute Video for All");
            }
            else
            {
                ParticipantAllSoftVideoMuteStatus = true;
            }
            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_DropAllParticipant()
        {
            bool retValue = false;

            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Drop All Participant");
                return retValue;
            }

            retValue = GetConnectorInstance.DropAllParticipants("Moderation");
            if (!retValue)
            {
                DisplayErrorMessageForAPI("Moderation", "Failed to Drop All Participant");
            }
            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_ParticipantCommand(String participantUserId, ParticipantCommandType participantCommand)
        {
            bool retValue = false;

            if (!IsAllowForModerationOperation() && 
                !(participantCommand == ParticipantCommandType.CameraPresetCommand || participantCommand == ParticipantCommandType.CameraViscaCommand))
            {
                String msgHeader;
                switch(participantCommand)
                {
                    case ParticipantCommandType.HandCommand:
                        msgHeader = "Dismiss Raised Hand";
                        break;
                    case ParticipantCommandType.MicCommand:
                        msgHeader = "Hard Mute/Unmute Mic For Participant";
                        break;
                    case ParticipantCommandType.CameraCommand:
                        msgHeader = "Hard Mute/Unmute Camera For Participant";
                        break;
                    case ParticipantCommandType.DisconnectCommand:
                        msgHeader = "Disconnect Participant";
                        break;
                    default:
                        msgHeader = "Moderation Command";
                        break;
                }
                DisplayMessageForAuthentication(msgHeader);
                return retValue;
            }

            lock (_itemsLock)
            {
                Participant participant = participantList.First(kvp => kvp.Key == participantUserId).Value;
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participantUserId);
                String message;
                if (item != null)
                {
                    if (participantCommand == ParticipantCommandType.SendPrivateMsgCommand)
                    {
                        if (participant.IsLocal())
                        {
                            return true;
                        }

                        var sendPrivateMsgDialog = new SendPrivateMsgDialog(participant.GetName());
                        if (sendPrivateMsgDialog.ShowDialog() == true)
                        {
                            retValue = GetConnectorInstance.SendPrivateChatMessage(participant, sendPrivateMsgDialog.GetPrivateMsg());
                            if (!retValue)
                            {
                                message = "Failed to Send Private Message to Participant : " + participant.GetName();
                                DisplayErrorMessageForAPI("Moderation", message);
                            }
                        }
                    }
                    else if (participantCommand == ParticipantCommandType.HandCommand)
                    {
                        if (item.ParticipantHandStatus)
                        {
                            var raisedHandDialog = new RaisedHandDialog();
                            if (raisedHandDialog.ShowDialog() == true)
                            {
                                if (raisedHandDialog.GetRaisedHandResponse())
                                {
                                    retValue = GetConnectorInstance.ApproveRaisedHand(participant, "ApprovedRaisedHand");
                                    if (!retValue)
                                    {
                                        DisplayErrorMessageForAPI("Moderation", "Failed to Approve Raised Hand for Participant");
                                    }
                                }
                                else
                                {
                                    List<Participant> list = new List<Participant>();
                                    list.Add(participant);
                                    retValue = GetConnectorInstance.DismissRaisedHand(list, "DismissRaisedHand");
                                    if (!retValue)
                                    {
                                        DisplayErrorMessageForAPI("Moderation", "Failed to Dismiss Raised Hand for Participants");
                                    }
                                    list.Clear();
                                }
                            }
                        }
                    }
                    else if (participantCommand == ParticipantCommandType.MicCommand)
                    {
                        if (item.ParticipantHardMuteUnmuteMicStatus)
                        {
                            retValue = GetConnectorInstance.DisableAudioForParticipant(participant, false, "HardUnmuteAudioForParticipant");
                            if (!retValue)
                            {
                                message = "Failed to Disable Hard Mute Audio for Participant : " + participant.GetName();
                                DisplayErrorMessageForAPI("Moderation", message);
                            }
                        }
                        else
                        {
                            retValue = GetConnectorInstance.DisableAudioForParticipant(participant, true, "HardMuteAudioForParticipant");
                            if (!retValue)
                            {
                                message = "Failed to Enable Hard Mute Audio for Participant : " + participant.GetName();
                                DisplayErrorMessageForAPI("Moderation", message);
                            }
                        }
                    }
                    else if (participantCommand == ParticipantCommandType.CameraCommand)
                    {
                        if (item.ParticipantHardMuteUnmuteCameraStatus)
                        {
                            retValue = GetConnectorInstance.DisableVideoForParticipant(participant, false, "HardUnmuteVideoForParticipant");
                            if (!retValue)
                            {
                                message = "Failed to Disable Hard Mute Video for Participant : " + participant.GetName();
                                DisplayErrorMessageForAPI("Moderation", message);
                            }
                        }
                        else
                        {
                            retValue = GetConnectorInstance.DisableVideoForParticipant(participant, true, "HardMuteVideoForParticipant");
                            if (!retValue)
                            {
                                message = "Failed to Enable Hard Mute Video for Participant : " + participant.GetName();
                                DisplayErrorMessageForAPI("Moderation", message);
                            }
                        }
                    }
                    else if (participantCommand == ParticipantCommandType.DisconnectCommand)
                    {
                        retValue = GetConnectorInstance.DropParticipant(participant, "Moderation");
                        if (!retValue)
                        {
                            message = "Failed to Drop Participant for Participant : " + participant.GetName();
                            DisplayErrorMessageForAPI("Moderation", message);
                        }
                    }
                    else if (participantCommand == ParticipantCommandType.CameraPresetCommand)
                    {
                        if (participant.IsLocal() || !item.ParticipantCameraPresetStatus)
                        {
                            return true;
                        }
                        VidyoCameraPreset presetDialog = new VidyoCameraPreset(item.ParticipantItemElemt_GetCamera());
                        presetDialog.ShowDialog();
                    }
                    else if (participantCommand == ParticipantCommandType.CameraViscaCommand)
                    {
                        if (participant.IsLocal() || !item.ParticipantCameraViscaStatus)
                        {
                            return true;
                        }
                        VidyoCameraViscaCommand viscaCommand = new VidyoCameraViscaCommand(item.ParticipantItemElemt_GetCamera());
                        viscaCommand.ShowDialog();
                    }
                }
            }
            return retValue;
        }

        public bool VidyoConferenceModerationViewModel_SearchUsers()
        {
            SearchUsers searchUsersDialog = new SearchUsers();
            searchUsersDialog.ShowDialog();
            return true;
        }
        #endregion

        #region CallBack Processing

        /*
         * All the Processing related to CallBack,
         * will be done in seperate thread.
         */

        public void ParticipantJoinedCallBackProcess(Participant participant, bool isPresenter)
        {
            String participantName = participant.GetName();
            String participantUserId = participant.GetUserId();
            String participantStatus = "";

            ParticipantItemElemt item;
            lock (_itemsLock)
            {
                item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participantUserId);
            }
            if (item != null)
            {
                return;
            }

            if(participant.IsLocal())
            {
                participantStatus = "me, ";
                localParticipantInfo = participant;
                SetControlAccordingToClearance();
            }

            switch(participant.GetClearanceType())
            {
                case ParticipantClearanceType.ParticipantCLEARANCETYPE_Admin:
                    participantStatus += "Admin";
                    break;

                case ParticipantClearanceType.ParticipantCLEARANCETYPE_Member:
                    participantStatus = participant.IsLocal() ? "me" : "";
                    break;

                case ParticipantClearanceType.ParticipantCLEARANCETYPE_Moderator:
                    participantStatus += "Moderator";
                    break;

                case ParticipantClearanceType.ParticipantCLEARANCETYPE_Owner:
                    participantStatus += "Owner";
                    break;

                default:
                    participantStatus += "Guest";
                    break;
            }

            lock (_itemsLock)
            {
                participantList.Add(new KeyValuePair<String, Participant>(participantUserId, participant));
                bool micStatus = true, cameraStatus = true;
                bool hardMuteMicStatus = ParticipantAllHardAudioMuteStatus;
                bool hardMuteCameraStatus = ParticipantAllHardVideoMuteStatus;
                if (IsLobbyMode || IsPresenterMode)
                {
                    micStatus = ParticipantAllHardAudioMuteStatus;
                    cameraStatus = ParticipantAllHardVideoMuteStatus;
                }

                if (participant.IsLocal())
                {
                    micStatus = localUserMicStatus;
                    cameraStatus = localUserCameraStatus;
                    hardMuteMicStatus = localUserHardMicMuteStatus;
                    hardMuteCameraStatus = localUserHardCameraMuteStatus;
                }

                ParticipantItemList.Add(new ParticipantItemElemt(
                    participantName, participantUserId, participantStatus, micStatus, cameraStatus, isPresenter, false, hardMuteMicStatus, hardMuteCameraStatus, false));
                ParticipantCount = "Participants(" + participantList.Count().ToString() + ")";
            }
        }

        public void ParticipantLeftCallBackProcess(Participant participant)
        {
            String participantName = participant.GetName();
            String participantId = participant.GetId();
            String participantUserId = participant.GetUserId();
            bool participantIsLocal = participant.IsLocal();

            lock (_itemsLock)
            {
                participantList.Remove(new KeyValuePair<String, Participant>(participantUserId, participant));
                ParticipantItemList.Remove(ParticipantItemList.Where(i => i.ParticipantUserId == participantUserId).Single());
                if (participant.IsLocal())
                {
                    localParticipantInfo = null;
                }
                ParticipantCount = "Participants(" + participantList.Count().ToString() + ")";
            }
        }

        public void DynamicParticipantChangedCallBackProcess(List<Participant> participants)
        {
            ;
        }

        public void LoudestParticipantChangedCallBackProcess(Participant participant, bool audioOnly)
        {
            ;
        }

        public void HandRaisedCallBackProcess(List<Participant> participant)
        {
            lock (_itemsLock)
            { 
                for (int iCounter = 0; iCounter < ParticipantItemList.Count; iCounter++)
                {
                    var item = ParticipantItemList.ElementAt(iCounter);
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetHandStatus(false);
                    }
                }

                for (int iCounter = 0; iCounter < participant.Count; iCounter++)
                {
                    String participantUserId = participant[iCounter].GetUserId();
                    var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participantUserId);
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetHandStatus(true);
                    }
                }
            }
        }

        public void PresenterChangedCallBackProcess(Participant participant)
        {
            lock (_itemsLock)
            {
                if (!string.IsNullOrEmpty(presenterUserId))
                {
                    var itemIsPresenter = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == presenterUserId);
                    if (itemIsPresenter != null)
                    {
                        itemIsPresenter.ParticipantItemElemt_SetPresenter(false);
                        itemIsPresenter.ParticipantItemElemt_SetHardMuteUnmuteMicStatus(ParticipantAllHardAudioMuteStatus);
                        presenterUserId = String.Empty;
                    }
                }

                if (participant != null)
                {
                    presenterUserId = participant.GetUserId();

                    var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetPresenter(true);
                        item.ParticipantItemElemt_SetHardMuteUnmuteMicStatus(false);
                    }
                    else 
                    {
                        ParticipantJoinedCallBackProcess(participant, true);
                    }
                }
            }
        }

        public void ModerationResultCallBackProcess(Participant participant, ConnectorModerationResult result, ConnectorModerationActionType action, String requestId)
        {
            switch (action)
            {
                case ConnectorModerationActionType.ConnectormoderationactiontypeMute:
                    {
                        if ( (requestId == "HardMuteAudioForAllEnable") &&
                            (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK))
                        {
                            UpdateHardMuteUnmuteMicStatusForAllParticipants(true);
                            ParticipantAllHardAudioMuteStatus = true;
                        }
                        else if ((requestId == "SoftMuteAudioForAll"))
                        {
                            ParticipantAllSoftAudioMuteStatus = false;
                        }
                        else if ((requestId == "HardMuteVideoForAllEnable") &&
                            (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK))
                        {
                            UpdateHardMuteUnmuteCameraStatusForAllParticipants(true);
                            ParticipantAllHardVideoMuteStatus = true;
                        }
                        else if ((requestId == "SoftMuteVideoForAll"))
                        {
                            ParticipantAllSoftVideoMuteStatus = false;
                        }
                        else if ((requestId == "HardMuteAudioForParticipant") &&
                                (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK))
                        {
                            if(participant!=null)
                            {
                                lock (_itemsLock)
                                {
                                    var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                                    if (item != null)
                                    {
                                        item.ParticipantItemElemt_SetHardMuteUnmuteMicStatus(true);
                                    }
                                }
                            }
                        }
                        else if ((requestId == "HardMuteVideoForParticipant") &&
                                    (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK))
                        {
                            if (participant != null)
                            {
                                lock (_itemsLock)
                                {
                                    var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                                    if (item != null)
                                    {
                                        item.ParticipantItemElemt_SetHardMuteUnmuteCameraStatus(true);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case ConnectorModerationActionType.ConnectormoderationactiontypeUnmute:
                    {
                        if ((requestId == "HardMuteAudioForAllDisable") &&
                            (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK))
                        {
                            UpdateHardMuteUnmuteMicStatusForAllParticipants(false);
                            ParticipantAllHardAudioMuteStatus = false;
                        }
                        else if ((requestId == "HardMuteVideoForAllDisable") &&
                            (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK))
                        {
                            UpdateHardMuteUnmuteCameraStatusForAllParticipants(false);
                            ParticipantAllHardVideoMuteStatus = false;
                        }
                        else if ((requestId == "HardUnmuteAudioForParticipant") &&
                                (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK))
                        {
                            if (participant != null)
                            {
                                lock (_itemsLock)
                                {
                                    var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                                    if (item != null)
                                    {
                                        item.ParticipantItemElemt_SetHardMuteUnmuteMicStatus(false);
                                    }
                                }
                            }
                        }
                        else if ((requestId == "HardUnmuteVideoForParticipant") &&
                                (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK))
                        {
                            if (participant != null)
                            {
                                lock (_itemsLock)
                                {
                                    var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                                    if (item != null)
                                    {
                                        item.ParticipantItemElemt_SetHardMuteUnmuteCameraStatus(false);
                                    }
                                }
                            }
                        }
                        else if ((requestId == "ApprovedRaisedHand") &&
                                (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK))
                        {
                            if (participant != null)
                            {
                                lock (_itemsLock)
                                {
                                    var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                                    if (item != null)
                                    {
                                        item.ParticipantItemElemt_SetHandStatus(false);
                                        item.ParticipantItemElemt_SetHardMuteUnmuteMicStatus(false);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case ConnectorModerationActionType.ConnectormoderationactiontypeStartLectureMode:
                    {
                        if (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK)
                        {
                            IsPresenterMode = true;
                            UpdateHardMuteUnmuteMicStatusForAllParticipants(true);
                            ParticipantAllHardAudioMuteStatus = true;
                        }
                        else
                        {
                            IsGroupMode = true;
                            UpdateHardMuteUnmuteMicStatusForAllParticipants(false);
                            ParticipantAllHardAudioMuteStatus = false;
                        }
                    }
                    break;
                case ConnectorModerationActionType.ConnectormoderationactiontypeStopLectureMode:
                    {
                        if (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK)
                        {
                            IsGroupMode = true;
                            UpdateHardMuteUnmuteMicStatusForAllParticipants(false);
                        }
                        else
                        {
                            IsPresenterMode = true;
                            UpdateHardMuteUnmuteMicStatusForAllParticipants(true);
                        }
                    }
                    break;
                case ConnectorModerationActionType.ConnectormoderationactiontypeSetPresenter:
                    {
                        if (result != Connector.ConnectorModerationResult.ConnectormoderationresultOK)
                        {
                            lock (_itemsLock)
                            {
                                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                                if (item != null)
                                {
                                    if (item.ParticipantIsPresenter)
                                    {
                                        item.ParticipantItemElemt_SetPresenter(false);

                                        if(!string.IsNullOrEmpty(presenterUserId))
                                        {
                                            var presenterItem = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == presenterUserId);
                                            presenterItem.ParticipantItemElemt_SetPresenter(true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case ConnectorModerationActionType.ConnectormoderationactiontypeRemovePresenter:
                    {
                        if (result != Connector.ConnectorModerationResult.ConnectormoderationresultOK)
                        {
                            lock (_itemsLock)
                            {
                                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                                if (item != null)
                                {
                                    if (item.ParticipantIsPresenter)
                                    {
                                        item.ParticipantItemElemt_SetPresenter(true);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case ConnectorModerationActionType.ConnectormoderationactiontypeRaiseHand:
                    {
                        if (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK)
                        {
                            IsRaisedHand = true;
                        }
                        else
                        {
                            IsRaisedHand = false;
                        }
                    }
                    break;
                case ConnectorModerationActionType.ConnectormoderationactiontypeUnraiseHand:
                    {
                        if (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK)
                        {
                            IsRaisedHand = false;
                        }
                        else
                        {
                            IsRaisedHand = true;
                        }
                    }
                    break;
                case ConnectorModerationActionType.ConnectormoderationactiontypeDismissRaisedHand:
                    {
                        lock (_itemsLock)
                        {
                            var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                            if (item != null)
                            {
                                item.ParticipantItemElemt_SetHandStatus(false);
                            }
                        }
                        break;
                    }
                case ConnectorModerationActionType.ConnectormoderationactiontypeDismissAllRaisedHands:
                case ConnectorModerationActionType.ConnectormoderationactiontypeSetModeratorPin:
                    {
                        if (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK)
                        {
                            confModerationWindow.SetIsModerationPIN(true);
                        }
                        else
                        {
                            confModerationWindow.SetIsModerationPIN(hasModeratorPin);
                        }
                    }
                    break;
                case ConnectorModerationActionType.ConnectormoderationactiontypeRemoveModeratorPin:
                    {
                        if (result == Connector.ConnectorModerationResult.ConnectormoderationresultOK)
                        {
                            confModerationWindow.SetIsModerationPIN(false);
                        }
                        else
                        {
                            confModerationWindow.SetIsModerationPIN(hasModeratorPin);
                        }
                    }
                    break;
                case ConnectorModerationActionType.ConnectormoderationactiontypeDisconnectAll:
                case ConnectorModerationActionType.ConnectormoderationactiontypeDisconnectOne:
                case ConnectorModerationActionType.ConnectormoderationactiontypeInvalid:
                    break;
            }
            DisplayModerationMessageForCallBack(participant, result, action, requestId);
        }

        public void RaiseHandResponseCallBackProcess(Participant.ParticipantHandState handState)
        {
            if (handState == Participant.ParticipantHandState.ParticipanthandstateDISMISSED)
            {
                DisplayMessage("Moderation", "Your raised hand request is dismissed");
                IsRaisedHand = false;
            }
            else if (handState == Participant.ParticipantHandState.ParticipanthandstateAPPROVED)
            {
                DisplayMessage("Moderation", "Your raised hand request is approved");
                IsRaisedHand = false;
            }
        }

        public void UpdateHardMuteUnmuteMicStatusForAllParticipants(bool micStatus)
        {
            lock (_itemsLock)
            {
                for (int iCounter = 0; iCounter < ParticipantItemList.Count; iCounter++)
                {
                    var item = ParticipantItemList.ElementAt(iCounter);
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetHardMuteUnmuteMicStatus(micStatus);
                        if( ((micStatus == true) && (item.ParticipantIsPresenter)))
                        {
                            item.ParticipantItemElemt_SetHardMuteUnmuteMicStatus(false);
                        }
                    }
                }
            }
        }

        public void UpdateHardMuteUnmuteCameraStatusForAllParticipants(bool cameraStatus)
        {
            lock (_itemsLock)
            {
                for (int iCounter = 0; iCounter < ParticipantItemList.Count; iCounter++)
                {
                    var item = ParticipantItemList.ElementAt(iCounter);
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetHardMuteUnmuteCameraStatus(cameraStatus);
                        if (((cameraStatus == true) && (item.ParticipantIsPresenter)))
                        {
                            item.ParticipantItemElemt_SetHardMuteUnmuteCameraStatus(false);
                        }
                    }
                }
            }
        }

        public void UpdateMicStatusForAllParticipants(bool micStatus)
        {
            lock (_itemsLock)
            {
                for (int iCounter = 0; iCounter < ParticipantItemList.Count; iCounter++)
                {
                    var item = ParticipantItemList.ElementAt(iCounter);
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetMicStatus(micStatus);
                        if (((micStatus == true) && (item.ParticipantIsPresenter)))
                        {
                            item.ParticipantItemElemt_SetHardMuteUnmuteMicStatus(false);
                        }
                    }
                }
            }
        }

        public void UpdateCameraStatusForAllParticipants(bool cameraStatus)
        {
            lock (_itemsLock)
            {
                for (int iCounter = 0; iCounter < ParticipantItemList.Count; iCounter++)
                {
                    var item = ParticipantItemList.ElementAt(iCounter);
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetCameraStatus(cameraStatus);
                        if (((cameraStatus == true) && (item.ParticipantIsPresenter)))
                        {
                            item.ParticipantItemElemt_SetHardMuteUnmuteCameraStatus(false);
                        }
                    }
                }
            }
        }

        public void ConferenceModeChangedCallBackProcess(ConnectorConferenceMode mode)
        {
            if (mode == ConnectorConferenceMode.ConnectorconferencemodeLOBBY)
            {
                IsLobbyMode = true;
                IsPresenterMode = true;
                IsGroupMode = false;
                UpdateHardMuteUnmuteMicStatusForAllParticipants(true);
                ParticipantAllHardAudioMuteStatus = true;
            }
            else if (mode == ConnectorConferenceMode.ConnectorconferencemodeLECTURE)
            {
                IsLobbyMode = false;
                IsPresenterMode = true;
                IsGroupMode = false;
                UpdateHardMuteUnmuteMicStatusForAllParticipants(true);
                ParticipantAllHardAudioMuteStatus = true;
            }
            else if (mode == ConnectorConferenceMode.ConnectorconferencemodeGROUP)
            {
                IsLobbyMode = false;
                IsGroupMode = true;
                IsPresenterMode = false;
                UpdateHardMuteUnmuteMicStatusForAllParticipants(false);
                ParticipantAllHardAudioMuteStatus = false;
            }
        }

        public void LockRoomResultCallBackProcess(ConnectorModerationResult result)
        {
            if (result == ConnectorModerationResult.ConnectormoderationresultOK)
            {
                IsLockRoom = true;
            }
            else
            {
                IsUnlockRoom = true;
            }
            DisplayMessageForCallBackResult("Lock Room", result);
        }

        public void UnlockRoomResultCallBackProcess(ConnectorModerationResult result)
        {
            if (result == ConnectorModerationResult.ConnectormoderationresultOK)
            {
                IsUnlockRoom = true;
            }
            else
            {
                IsLockRoom = true;
            }

            DisplayMessageForCallBackResult("Unlock Room", result);
        }

        public void SetRoomPINResultCallBackProcess(ConnectorModerationResult result)
        {
            if(result == ConnectorModerationResult.ConnectormoderationresultOK)
            {
                hasRoomPin = true;
            }
            confModerationWindow.SetIsRoomPIN(hasRoomPin);
            DisplayMessageForCallBackResult("Set Room Pin", result);            
        }

        public void RemoveRoomPINResultCallBackProcess(ConnectorModerationResult result)
        {
            if (result == ConnectorModerationResult.ConnectormoderationresultOK)
            {
                hasRoomPin = false;
            }
            confModerationWindow.SetIsRoomPIN(hasRoomPin);
            DisplayMessageForCallBackResult("Remove Room Pin", result);
        }

        public void RequestModeratorRoleResultCallBackProcess(ConnectorModerationResult result)
        {
            if (result == ConnectorModerationResult.ConnectormoderationresultOK)
            {
                IsRoleModerator = true;
                IsRoleNone = false;
            }
            else
            {
                IsRoleModerator = false;
                IsRoleNone = true;
            }
            SetControlAccordingToClearance();
            DisplayMessageForCallBackResult("Request Moderator Role", result);
        }

        public void RemoveModeratorRoleResultCallBackProcess(ConnectorModerationResult result)
        {
            if (result == ConnectorModerationResult.ConnectormoderationresultOK)
            {
                IsRoleNone = true;
                IsRoleModerator = false;
            }
            else
            {
                IsRoleModerator = true;
                IsRoleNone = false;
            }
            SetControlAccordingToClearance();
            DisplayMessageForCallBackResult("Remove Moderator Role", result);
        }

        public void ConnectionPropertiesChangedCallBackProcess(ConnectorConnectionProperties connectionProperties)
        {
            RoomName = connectionProperties.roomName;

            if (connectionProperties.recordingState == ConnectorRecordingState.ConnectorrecordingstateNotRecording)
            {
                IsRecordingStarted = false;
                IsRecordingStopped = true;
                IsRecordingPaused = false;
            }
            else if (connectionProperties.recordingState == ConnectorRecordingState.ConnectorrecordingstateRecording)
            {
                IsRecordingStarted = true;
                IsRecordingStopped = false;
                IsRecordingPaused = false;
            }
            if (connectionProperties.recordingState == ConnectorRecordingState.ConnectorrecordingstateRecordingPaused)
            {
                IsRecordingStarted = true;
                IsRecordingStopped = false;
                IsRecordingPaused = true;
            }
            
            hasModeratorPin = connectionProperties.hasModeratorPin;
            hasRoomPin = connectionProperties.hasRoomPin;
            if (confModerationWindow != null)
            {
                confModerationWindow.SetIsModerationPIN(hasModeratorPin);
                confModerationWindow.SetIsRoomPIN(hasRoomPin);
            }

            if(connectionProperties.isRoomLocked)
            {
                IsLockRoom = true;
            }
            else
            {
                IsUnlockRoom = true;
            } 
        }

        public void GetRecordingServiceProfilesCallBackProcess(List<string> profiles, List<string> prefixes, ConnectorRecordingServiceResult result)
        {
            if (result == ConnectorRecordingServiceResult.ConnectorrecordingserviceresultSuccess)
            {
                for (int iCounter = 0; iCounter < profiles.Count; iCounter++)
                {
                    String recordingProfile = profiles[iCounter];
                    String recordingPrefix = prefixes[iCounter];
                    var tuple = Tuple.Create(iCounter, recordingProfile, recordingPrefix);

                    RecordingProfileItemList.Add(new RecordingItemElemt { RecordingProfileDescription = recordingProfile });
                    recordingProfileList.Add(tuple);
                }
            }
            else
            {
                indexRecordingProfile = -1;
            }
            DisplayMessageForCallBackResult("Recording Service Get Profile", result);
        }

        public void RecordingServiceStartResultCallBackProcess(ConnectorModerationResult result)
        {
            if (result != ConnectorModerationResult.ConnectormoderationresultOK)
            {
                IsRecordingStarted = false;
                IsRecordingStopped = true;
                IsRecordingPaused = false;
            }
            else
            {
                IsRecordingStarted = true;
                IsRecordingStopped = false;
                IsRecordingPaused = false;
            }
            DisplayMessageForCallBackResult("Recording Service Start", result);
        }

        public void RecordingServiceStopResultCallBackProcess(ConnectorModerationResult result)
        {
            if (result != ConnectorModerationResult.ConnectormoderationresultOK)
            {
                IsRecordingStarted = true;
                IsRecordingStopped = false;
                IsRecordingPaused = false;
            }
            else
            {
                IsRecordingStarted = false;
                IsRecordingStopped = true;
                IsRecordingPaused = false;
            }
            DisplayMessageForCallBackResult("Recording Service Stop", result);
        }

        public void RecordingServicePauseResultCallBackProcess(ConnectorModerationResult result)
        {
            if (result != ConnectorModerationResult.ConnectormoderationresultOK)
            {
                IsRecordingPaused = false;
            }
            else
            {
                IsRecordingPaused = true;
            }
            DisplayMessageForCallBackResult("Recording Service Pause", result);
        }

        public void RecordingServiceResumeResultCallBackProcess(ConnectorModerationResult result)
        {
            if (result != ConnectorModerationResult.ConnectormoderationresultOK)
            {
                IsRecordingPaused = true;
            }
            else
            {
                IsRecordingPaused = false;
            }

            DisplayMessageForCallBackResult("Recording Service Resume", result);
        }

        public void RemoteCameraAddedCallBackProcess(RemoteCamera remoteCamera, Participant participant)
        {
            lock (_itemsLock)
            {
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                if (item != null)
                {
                    item.ParticipantItemElemt_SetCameraStatus(false);
                    item.ParticipantItemElemt_SetCamera(new RemoteCameraModel(remoteCamera));
                }
            }
        }

        public void RemoteCameraRemovedCallBackProcess(RemoteCamera remoteCamera, Participant participant)
        {
            lock (_itemsLock)
            {
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                if (item != null)
                {
                    item.ParticipantItemElemt_SetCameraStatus(true);
                    item.ParticipantItemElemt_SetCamera(null);
                    item.ParticipantItemElemt_SetCameraPresetStatus(false);
                    item.ParticipantItemElemt_SetCameraPresetAvailable(string.Empty);
                    item.ParticipantItemElemt_SetCameraViscaSupport(string.Empty);
                }
            }
        }

        public void RemoteCameraStateUpdatedCallBackProcess(RemoteCamera remoteCamera, Participant participant, Device.DeviceState state)
        {
            lock (_itemsLock)
            {
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                if (item != null)
                {
                    if (state == Device.DeviceState.DevicestatePaused)
                    {
                        item.ParticipantItemElemt_SetCameraStatus(true);
                    }
                    else if (state == Device.DeviceState.DevicestateResumed)
                    {
                        item.ParticipantItemElemt_SetCameraStatus(false);
                    }
                    else if (state == Device.DeviceState.DevicestateNotControllable)
                    {
                        item.ParticipantItemElemt_SetCameraPresetStatus(false);
                        item.ParticipantItemElemt_SetCameraPresetAvailable(string.Empty);
                    }
                    else if (state == Device.DeviceState.DevicestateControllable)
                    {
                        List<CameraPreset> presets = item.ParticipantItemElemt_GetCamera().GetPresetData();
                        if (presets!= null && presets.Count != 0)
                        {
                            item.ParticipantItemElemt_SetCameraPresetStatus(true);
                            item.ParticipantItemElemt_SetCameraPresetAvailable("Available");
                        }
                        else
                        {
                            item.ParticipantItemElemt_SetCameraPresetStatus(false);
                            item.ParticipantItemElemt_SetCameraPresetAvailable(String.Empty);
                        }
                        CameraControlCapabilities cap = remoteCamera.GetControlCapabilities();
                        if(cap.hasViscaSupport)
                        {
                            item.ParticipantItemElemt_SetCameraViscaSupport("Available");
                        }
                        else
                        {
                            item.ParticipantItemElemt_SetCameraViscaSupport(String.Empty);
                        }
                    }
                }
            }
        }

        public void RemoteMicrophoneAddedCallBackProcess(RemoteMicrophone remoteMicrophone, Participant participant)
        {
            lock (_itemsLock)
            {
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                if (item != null)
                {
                    item.ParticipantItemElemt_SetMicStatus(false);
                }
            }
        }

        public void RemoteMicrophoneRemovedCallBackProcess(RemoteMicrophone remoteMicrophone, Participant participant)
        {
            lock (_itemsLock)
            {
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                if (item != null)
                {
                    item.ParticipantItemElemt_SetMicStatus(true);
                }
            }
        }

        public void RemoteMicrophoneStateUpdatedCallBackProcess(RemoteMicrophone remoteMicrophone, Participant participant, Device.DeviceState state)
        {
            lock (_itemsLock)
            {
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == participant.GetUserId());
                if (item != null)
                {
                    if (state == Device.DeviceState.DevicestatePaused)
                    {
                        item.ParticipantItemElemt_SetMicStatus(true);
                    }
                    if (state == Device.DeviceState.DevicestateResumed)
                    {
                        item.ParticipantItemElemt_SetMicStatus(false);
                    }
                }
            }
        }

        public void LocalMicrophoneStateUpdatedCallBackProcess(LocalMicrophone localMicrophone, Device.DeviceState state)
        {
            lock (_itemsLock)
            {
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == GetLocalParticipantUserId());
                if ((state == Device.DeviceState.DevicestateStopped) || (state == Device.DeviceState.DevicestatePaused))
                {
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetMicStatus(true);
                    }
                    localUserMicStatus = true;
                }
                if ((state == Device.DeviceState.DevicestateStarted) || (state == Device.DeviceState.DevicestateResumed))
                {
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetMicStatus(false);
                    }
                    localUserMicStatus = false;
                }
            }
        }

        public void LocalCameraStateUpdatedCallBackProcess(LocalCamera localCamera, Device.DeviceState state)
        {
            lock (_itemsLock)
            {
                var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == GetLocalParticipantUserId());

                if (state == Device.DeviceState.DevicestateStopped)
                {
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetCameraStatus(true);
                    }
                    localUserCameraStatus = true;
                }
                if (state == Device.DeviceState.DevicestateStarted)
                {
                    if (item != null)
                    {
                        item.ParticipantItemElemt_SetCameraStatus(false);
                    }
                    localUserCameraStatus = false;
                }
            }
        }

        public void ModerationCommandReceivedCallBackProcess(Device.DeviceType deviceType, Room.RoomModerationType moderationType, bool state)
        {
            var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == GetLocalParticipantUserId());

            if ((deviceType == Device.DeviceType.DevicetypeLocalMicrophone) &&
            (moderationType == Room.RoomModerationType.RoommoderationtypeHardMute))
            {
                localUserHardMicMuteStatus = state;
                if (item != null)
                {
                    item.ParticipantItemElemt_SetHardMuteUnmuteMicStatus(state);
                }
            }
            else if ((deviceType == Device.DeviceType.DevicetypeLocalCamera) &&
            (moderationType == Room.RoomModerationType.RoommoderationtypeHardMute))
            {
                localUserHardCameraMuteStatus = state;
                if (item != null)
                {
                    item.ParticipantItemElemt_SetHardMuteUnmuteCameraStatus(state);
                }
            }
            else if ((deviceType == Device.DeviceType.DevicetypeLocalMicrophone) &&
                (moderationType == Room.RoomModerationType.RoommoderationtypeSoftMute))
            {
                if (item != null)
                {
                    item.ParticipantItemElemt_SetMicStatus(state);
                }
            }
            else if ((deviceType == Device.DeviceType.DevicetypeLocalCamera) &&
            (moderationType == Room.RoomModerationType.RoommoderationtypeSoftMute))
            {
                if (item != null)
                {
                    item.ParticipantItemElemt_SetCameraStatus(state);
                }
            }
        }
		#endregion

		#region Error Messages
        /*
        * Error Message
        */

        public void DisplayMessage(String msgHeader, String message)
        {
            GeneralDialog generalDialog = new GeneralDialog();
            generalDialog.GeneralDialog_ShowMessage(msgHeader,message);
            generalDialog.ShowDialog();
        }

        public void DisplayErrorMessageForAPI(String msgHeader, String message)
        {
            DisplayMessage(msgHeader, message);
        }

        public void DisplayMessageForAuthentication(String msgHeader)
        {
            DisplayMessage(msgHeader, "Warning : We are sorry, you don't have enough privileges to perform moderation operation");
        }

        public void DisplayMessageForCallBackResult(String message, ConnectorSearchResult result)
        {
            if (result != ConnectorSearchResult.ConnectorsearchresultOk)
            {
                String ModerationMsg = "Result : " + ConvertSearchResultToString(result);
                DisplayMessage(message, ModerationMsg);
            }
        }

        public void DisplayMessageForCallBackResult(String message, ConnectorModerationResult result)
        {
            if (result != ConnectorModerationResult.ConnectormoderationresultOK)
            {
                String ModerationMsg = "Result : " + ConvertModerationResultToString(result);
                DisplayMessage(message, ModerationMsg);
            }
        }

        public void DisplayMessageForCallBackResult(String message, ConnectorRecordingServiceResult result)
        {
            if (result != ConnectorRecordingServiceResult.ConnectorrecordingserviceresultSuccess)
            {
                String ModerationMsg = "Result : " + ConvertRecordingServiceResultToString(result);
                DisplayMessage(message, ModerationMsg);
            }
        }

        public void DisplayModerationMessageForCallBack(Participant participant, ConnectorModerationResult result, ConnectorModerationActionType action, String requestId)
        {
            if (result != ConnectorModerationResult.ConnectormoderationresultOK)
            {
                string moderationMsgHeader = ConvertModerationActionToString(action);
 
                string moderationMsg = "Result : " + ConvertModerationResultToString(result) + ", ";
                moderationMsg += "Request Id : " + requestId + ", ";
                moderationMsg += "Participant : ";
                if (participant != null)
                {
                    moderationMsg += participant.GetName();
                }
                DisplayMessage(moderationMsgHeader, moderationMsg);
            }
        }

        public String ConvertSearchResultToString(ConnectorSearchResult result)
        {
            switch (result)
            {
                case ConnectorSearchResult.ConnectorsearchresultOk:
                    return "Ok";
                case ConnectorSearchResult.ConnectorsearchresultNoRecords:
                    return "No Records";
                case ConnectorSearchResult.ConnectorsearchresultNoResponse:
                    return "No Response";
                case ConnectorSearchResult.ConnectorsearchresultMiscLocalError:
                    return "Misc Local Error";
                case ConnectorSearchResult.ConnectorsearchresultMiscRemoteError:
                default:
                    return "Misc Remote Error";
            }
        }

        public String ConvertRecordingServiceResultToString(ConnectorRecordingServiceResult result)
        {
            switch (result)
            {
                case ConnectorRecordingServiceResult.ConnectorrecordingserviceresultSuccess:
                    return "Success";
                case ConnectorRecordingServiceResult.ConnectorrecordingserviceresultInvalidArgument:
                    return "Invalid Argument";
                case ConnectorRecordingServiceResult.ConnectorrecordingserviceresultGeneralFailure:
                    return "General Failure";
                case ConnectorRecordingServiceResult.ConnectorrecordingserviceresultSeatLicenseExpired:
                    return "Seat License Expired";
                case ConnectorRecordingServiceResult.ConnectorrecordingserviceresultNotLicensed:
                    return "Not Licensed";
                case ConnectorRecordingServiceResult.ConnectorrecordingserviceresultResourceNotAvailable:
                    return "ResourceNotAvailable";
                case ConnectorRecordingServiceResult.ConnectorrecordingserviceresultControlMeetingFailure:
                default:
                    return "Control Meeting Failure";
            }
        }

        public String ConvertModerationResultToString(ConnectorModerationResult result)
        {
            switch (result)
            {
                case ConnectorModerationResult.ConnectormoderationresultOK:
                    return "OK";
                case ConnectorModerationResult.ConnectormoderationresultNoResponse:
                    return "No Response";
                case ConnectorModerationResult.ConnectormoderationresultUnauthorized:
                    return "Unauthorized";
                case ConnectorModerationResult.ConnectormoderationresultNotOwnerOfRoom:
                    return "Not Owner Of Room";
                case ConnectorModerationResult.ConnectormoderationresultNotAcceptable:
                    return "Not Acceptable";
                case ConnectorModerationResult.ConnectormoderationresultNotAllowed:
                    return "Not Allowed";
                case ConnectorModerationResult.ConnectormoderationresultConflict:
                    return "Conflict";
                case ConnectorModerationResult.ConnectormoderationresultInvalidInput:
                    return "Invalid Input";
                case ConnectorModerationResult.ConnectormoderationresultOutOfResources:
                    return "Out Of Resources";
                case ConnectorModerationResult.ConnectormoderationresultUserIsOffline:
                    return "User Is Offline";
                case ConnectorModerationResult.ConnectormoderationresultRoomFull:
                    return "Room Full";
                case ConnectorModerationResult.ConnectormoderationresultMiscLocalError:
                    return "Misc Local Error";
                case ConnectorModerationResult.ConnectormoderationresultMiscRemoteError:
                default:
                    return "Misc Remote Error";
            }
        }

        public String ConvertModerationActionToString(ConnectorModerationActionType action)
        {
            switch (action)
            {
                case ConnectorModerationActionType.ConnectormoderationactiontypeMute:
                    return "Mute";
                case ConnectorModerationActionType.ConnectormoderationactiontypeUnmute:
                    return "Unmute";
                case ConnectorModerationActionType.ConnectormoderationactiontypeStartLectureMode:
                    return "Start Lecture Mode";
                case ConnectorModerationActionType.ConnectormoderationactiontypeStopLectureMode:
                    return "Stop Lecture Mode";
                case ConnectorModerationActionType.ConnectormoderationactiontypeSetPresenter:
                    return "Set Presenter";
                case ConnectorModerationActionType.ConnectormoderationactiontypeRemovePresenter:
                    return "Remove Presenter";
                case ConnectorModerationActionType.ConnectormoderationactiontypeRaiseHand:
                    return "Raise Hand";
                case ConnectorModerationActionType.ConnectormoderationactiontypeUnraiseHand:
                    return "Unraise Hand";
                case ConnectorModerationActionType.ConnectormoderationactiontypeDismissRaisedHand:
                    return "Dismiss Raised Hand";
                case ConnectorModerationActionType.ConnectormoderationactiontypeDismissAllRaisedHands:
                    return "Dismiss All Raised Hands";
                case ConnectorModerationActionType.ConnectormoderationactiontypeSetModeratorPin:
                    return "Set Moderator Pin";
                case ConnectorModerationActionType.ConnectormoderationactiontypeRemoveModeratorPin:
                    return "Remove Moderator Pin";
                case ConnectorModerationActionType.ConnectormoderationactiontypeDisconnectAll:
                    return "Disconnect All";
                case ConnectorModerationActionType.ConnectormoderationactiontypeDisconnectOne:
                    return "Disconnect One";
                case ConnectorModerationActionType.ConnectormoderationactiontypeInvalid:
                default:
                    return "Invalid";
            }
        }
		#endregion

		#region Binding Variables
/*
 * Bindings Variables
 */
        public Action CloseAction { get; set; }

        private string _moderatorPIN;
        public string ModeratorPIN
        {
            get { return _moderatorPIN; }
            set { _moderatorPIN = value; OnPropertyChanged(); }
        }

        private bool _isTexBoxModeratorPinEnabled;
        public bool IsTexBoxModeratorPinEnabled
        {
            get { return _isTexBoxModeratorPinEnabled; }
            set { _isTexBoxModeratorPinEnabled = value; OnPropertyChanged(); }
        }

        private bool _isButtonRemoveModeratorPinEnabled;
        public bool IsButtonRemoveModeratorPinEnabled
        {
            get { return _isButtonRemoveModeratorPinEnabled; }
            set { _isButtonRemoveModeratorPinEnabled = value; OnPropertyChanged(); }
        }

        private bool _isButtonSetModeratorPinEnabled;
        public bool IsButtonSetModeratorPinEnabled
        {
            get { return _isButtonSetModeratorPinEnabled; }
            set { _isButtonSetModeratorPinEnabled = value; OnPropertyChanged(); }
        }

        private string _roomPIN;
        public string RoomPIN
        {
            get { return _roomPIN; }
            set { _roomPIN = value; OnPropertyChanged(); }
        }

        private bool _isTextBlockRoomPINEnabled;
        public bool IsTextBlockRoomPINEnabled
        {
            get { return _isTextBlockRoomPINEnabled; }
            set { _isTextBlockRoomPINEnabled = value; OnPropertyChanged(); }
        }

        private bool _isButtonRemoveRoomPinEnabled;
        public bool IsButtonRemoveRoomPinEnabled
        {
            get { return _isButtonRemoveRoomPinEnabled; }
            set { _isButtonRemoveRoomPinEnabled = value; OnPropertyChanged(); }
        }

        private bool _isButtonSetRoomPinEnabled;
        public bool IsButtonSetRoomPinEnabled
        {
            get { return _isButtonSetRoomPinEnabled; }
            set { _isButtonSetRoomPinEnabled = value; OnPropertyChanged(); }
        }

        private bool _isLobbyMode;
        public bool IsLobbyMode
        {
            get { return _isLobbyMode; }
            set { _isLobbyMode = value; OnPropertyChanged(); }
        }

        private bool _isPresenterMode;
        public bool IsPresenterMode
        {
            get { return _isPresenterMode; }
            set { _isPresenterMode = value; OnPropertyChanged(); }
        }

        private bool _isPresenterModeEnabled;
        public bool IsPresenterModeEnabled
        {
            get { return _isPresenterModeEnabled; }
            set { _isPresenterModeEnabled = value; OnPropertyChanged(); }
        }

        private bool _isGroupMode;
        public bool IsGroupMode
        {
            get { return _isGroupMode; }
            set { _isGroupMode = value; OnPropertyChanged(); }
        }

        private bool _isGroupModeEnabled;
        public bool IsGroupModeEnabled
        {
            get { return _isGroupModeEnabled; }
            set { _isGroupModeEnabled = value; OnPropertyChanged(); }
        }

        private bool _isLockRoom;
        public bool IsLockRoom
        {
            get { return _isLockRoom; }
            set { _isLockRoom = value; OnPropertyChanged(); }
        }

        private bool _isLockRoomEnabled;
        public bool IsLockRoomEnabled
        {
            get { return _isLockRoomEnabled; }
            set { _isLockRoomEnabled = value; OnPropertyChanged(); }
        }

        private bool _isUnlockRoom;
        public bool IsUnlockRoom
        {
            get { return _isUnlockRoom; }
            set { _isUnlockRoom = value; OnPropertyChanged(); }
        }

        private bool _isUnlockRoomEnabled;
        public bool IsUnlockRoomEnabled
        {
            get { return _isUnlockRoomEnabled; }
            set { _isUnlockRoomEnabled = value; OnPropertyChanged(); }
        }

        private bool _isRoleModerator;
        public bool IsRoleModerator
        {
            get { return _isRoleModerator; }
            set { _isRoleModerator = value; OnPropertyChanged(); }
        }

        private bool _isRoleModeratorEnabled;
        public bool IsRoleModeratorEnabled
        {
            get { return _isRoleModeratorEnabled; }
            set { _isRoleModeratorEnabled = value; OnPropertyChanged(); }
        }

        private bool _isRoleNone;
        public bool IsRoleNone
        {
            get { return _isRoleNone; }
            set { _isRoleNone = value; OnPropertyChanged(); }
        }

        private bool _isRoleNoneEnabled;
        public bool IsRoleNoneEnabled
        {
            get { return _isRoleNoneEnabled; }
            set { _isRoleNoneEnabled = value; OnPropertyChanged(); }
        }

        private string _roomName;
        public string RoomName
        {
            get { return _roomName; }
            set
            {
                _roomName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<RecordingItemElemt> RecordingProfileItemList { get; set; }
        public ObservableCollection<ParticipantItemElemt> ParticipantItemList { get; set; }

        private bool _isRecordingStarted;
        public bool IsRecordingStarted
        {
            get { return _isRecordingStarted; }
            set
            {
                _isRecordingStarted = value;
                OnPropertyChanged();
            }
        }

        private bool _isImageRecordingStartEnabled;
        public bool IsImageRecordingStartEnabled
        {
            get { return _isImageRecordingStartEnabled; }
            set
            {
                _isImageRecordingStartEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isRecordingStopped;
        public bool IsRecordingStopped
        {
            get { return _isRecordingStopped; }
            set
            {
                _isRecordingStopped = value;
                OnPropertyChanged();
            }
        }

        private bool _isImageRecordingStopEnabled;
        public bool IsImageRecordingStopEnabled
        {
            get { return _isImageRecordingStopEnabled; }
            set
            {
                _isImageRecordingStopEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isRecordingPaused;
        public bool IsRecordingPaused
        {
            get { return _isRecordingPaused; }
            set
            {
                _isRecordingPaused = value;
                OnPropertyChanged();
            }
        }

        private bool _isImageRecordingPausedEnabled;
        public bool IsImageRecordingPausedEnabled
        {
            get { return _isImageRecordingPausedEnabled; }
            set
            {
                _isImageRecordingPausedEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isHandRaised;
        public bool IsRaisedHand
        {
            get { return _isHandRaised; }
            set
            {
                _isHandRaised = value;
                OnPropertyChanged();
            }
        }

        private string _moderationStatus;
        public string ModerationStatus
        {
            get { return _moderationStatus; }
            set
            {
                _moderationStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _isImageRemovePresenterEnabled;
        public bool IsImageRemovePresenterEnabled
        {
            get { return _isImageRemovePresenterEnabled; }
            set
            {
                _isImageRemovePresenterEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isImageDismissAllRaisedHandEnabled;
        public bool IsImageDismissAllRaisedHandEnabled
        {
            get { return _isImageDismissAllRaisedHandEnabled; }
            set
            {
                _isImageDismissAllRaisedHandEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _participantAllHardVideoMuteStatus;
        public bool ParticipantAllHardVideoMuteStatus
        {
           get { return _participantAllHardVideoMuteStatus; }
           set
            {
                _participantAllHardVideoMuteStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _isImageHardMuteVideoAllEnabled;
        public bool IsImageHardMuteVideoAllEnabled
        {
            get { return _isImageHardMuteVideoAllEnabled; }
            set
            {
                _isImageHardMuteVideoAllEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _participantAllHardAudioMuteStatus;
        public bool ParticipantAllHardAudioMuteStatus
        {
            get { return _participantAllHardAudioMuteStatus; }
            set
            {
                _participantAllHardAudioMuteStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _isImageHardMuteAudioAllEnabled;
        public bool IsImageHardMuteAudioAllEnabled
        {
            get { return _isImageHardMuteAudioAllEnabled; }
            set
            {
                _isImageHardMuteAudioAllEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _participantAllSoftVideoMuteStatus;
        public bool ParticipantAllSoftVideoMuteStatus
        {
            get { return _participantAllSoftVideoMuteStatus; }
            set
            {
                _participantAllSoftVideoMuteStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _isImageSoftMuteAudioAllEnabled;
        public bool IsImageSoftMuteAudioAllEnabled
        {
            get { return _isImageSoftMuteAudioAllEnabled; }
            set
            {
                _isImageSoftMuteAudioAllEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _participantAllSoftAudioMuteStatus;
        public bool ParticipantAllSoftAudioMuteStatus
        {
            get { return _participantAllSoftAudioMuteStatus; }
            set
            {
                _participantAllSoftAudioMuteStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _isImageSoftMuteVideoAllEnabled;
        public bool IsImageSoftMuteVideoAllEnabled
        {
            get { return _isImageSoftMuteVideoAllEnabled; }
            set
            {
                _isImageSoftMuteVideoAllEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isImageDropAllParticipantEnabled;
        public bool IsImageDropAllParticipantEnabled
        {
            get { return _isImageDropAllParticipantEnabled; }
            set
            {
                _isImageDropAllParticipantEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _participantCount;
        public string ParticipantCount
        {
            get { return _participantCount; }
            set
            {
                _participantCount = value;
                OnPropertyChanged();
            }
        }

        private bool _isLabelAddParticipantEnabled;
        public bool IsLabelAddParticipantEnabled
        {
            get { return _isLabelAddParticipantEnabled; }
            set
            {
                _isLabelAddParticipantEnabled = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Command Function
        /*
         * Command Action
         */
        private void SetModeratorPIN()
        {
            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Set Moderator Pin");
                return;
            }

            bool retValue = GetConnectorInstance.SetModeratorPIN(ModeratorPIN, "SetModeratorPIN");
            if (!retValue)
            {
                DisplayErrorMessageForAPI("Moderator Pin", "Failed to Set Moderator PIN");
            }
            confModerationWindow.SetIsModerationPIN(hasModeratorPin);
        }

        private void RemoveModeratorPIN()
        {
            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Remove Moderator Pin");
                return;
            }

            if (!GetConnectorInstance.RemoveModeratorPIN("RemoveModeratorPIN"))
            {
                DisplayErrorMessageForAPI("Moderator Pin", "Failed to Remove Moderator PIN");
            }
            confModerationWindow.SetIsModerationPIN(hasModeratorPin);
        }

        private void SetRoomPIN()
        {
            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Set Room Pin");
                return;
            }

            if (!GetConnectorInstance.SetRoomPIN(RoomPIN, new ModerationResultListener(this)))
            {
                DisplayErrorMessageForAPI("Room Pin", "Failed to Set Room PIN");
            }
            confModerationWindow.SetIsRoomPIN(hasRoomPin);
        }

        private void RemoveRoomPIN()
        {
            if (!IsAllowForModerationOperation())
            {
                DisplayMessageForAuthentication("Set Room Pin");
                return;
            }

            if (!GetConnectorInstance.RemoveRoomPIN(new ModerationResultListener(this)))
            {
                DisplayErrorMessageForAPI("Room Pin", "Failed to Remove Room PIN");
            }
            confModerationWindow.SetIsRoomPIN(hasRoomPin);
            RoomPIN = "";
        }

        private void SetConferenceModeAsPresenterMode()
        {
            if (!IsAllowForModerationOperation())
            {
                IsGroupMode = true;
                DisplayMessageForAuthentication("Set Presenter Mode");
                return;
            }

            if (!IsPresenterMode)
            { 
                if (!GetConnectorInstance.StartLectureMode("StartLectureMode"))
                {
                    IsGroupMode = true;
                    DisplayErrorMessageForAPI("Lecture Mode", "Failed to Start Lecture Mode");
                    
                }
                else
                {
                    IsPresenterMode = true;
                }
            }
        }

        private void SetConferenceModeAsGroupMode()
        {
            if (!IsAllowForModerationOperation())
            {
                IsPresenterMode = true;
                DisplayMessageForAuthentication("Set Group Mode");
                return;
            }

            if (!IsGroupMode)
            {
                if (!GetConnectorInstance.StopLectureMode("StopLectureMode"))
                {
                    IsPresenterMode = true;
                    DisplayErrorMessageForAPI("Lecture Mode", "Failed to Stop Lecture Mode");
                }
                else
                {
                    IsGroupMode = true;
                }
            }
        }

        private void SetConferenceRoomAsLockRoom()
        {
            if (!IsAllowForModerationOperation())
            {
                IsUnlockRoom = true;
                DisplayMessageForAuthentication("Lock Room");
                return;
            }

            if (!GetConnectorInstance.LockRoom(new ModerationResultListener(this)))
            {
                IsUnlockRoom = true;
                DisplayErrorMessageForAPI("Lock Room", "Failed to Lock Room");
            }
            else
            {
                IsLockRoom = true;
            }
        }

        private void SetConferenceRoomAsUnlockRoom()
        {
            if (!IsAllowForModerationOperation())
            {
                IsLockRoom = true;
                DisplayMessageForAuthentication("Unlock Room");
                return;
            }

            if (!GetConnectorInstance.UnlockRoom(new ModerationResultListener(this)))
            {
                IsLockRoom = true;
                DisplayErrorMessageForAPI("Lock Room", "Failed to Unlock Room");
            }
            else
            {
                IsUnlockRoom = true;
            }
        }

        private void SetModeratorRoleAsModerator()
        {
            ParticipantClearanceType localUserClearanceType = GetLocalParticipantClearanceType();
            if (localUserClearanceType == ParticipantClearanceType.ParticipantCLEARANCETYPE_None)
            {
                IsRoleNone = true;
                DisplayMessageForAuthentication("Request For Moderator Role");
                return;
            }

            bool retValue = false;
            if (localUserClearanceType == ParticipantClearanceType.ParticipantCLEARANCETYPE_Member)
            {
                ModeratorPinDialog moderatorDialog = new ModeratorPinDialog();
                if (moderatorDialog.ShowDialog() == true)
                {
                    retValue = GetConnectorInstance.RequestModeratorRole(moderatorDialog.ModeratorPin, new ModerationResultListener(this));
                }
                else
                {
                    IsRoleNone = true;
                    return ;
                }
            }
            else
            {
                retValue = GetConnectorInstance.RequestModeratorRole(null, new ModerationResultListener(this));
            }

            if (!retValue)
            {
                IsRoleNone = true;
                DisplayErrorMessageForAPI("Moderator Role", "Failed to Request Moderator Role");
            }
        }

        private void SetModeratorRoleAsNone()
        {
            if (GetLocalParticipantClearanceType() == ParticipantClearanceType.ParticipantCLEARANCETYPE_None)
            {
                IsRoleModerator = true;
                DisplayMessageForAuthentication("Request For Remove Moderator Role");
                return;
            }
            
            if (!GetConnectorInstance.RemoveModeratorRole(new ModerationResultListener(this)))
            {
                IsRoleModerator = true;
                DisplayErrorMessageForAPI("Moderator Role", "Failed to Remove Moderator Role");
            }
        }

        private void SetPresenter()
        {
            if (!IsAllowForModerationOperation() || !IsPresenterMode)
            {
                var itemPresenter = ParticipantItemList.FirstOrDefault(i => i.ParticipantIsPresenter == true);
                if (itemPresenter != null)
                {
                    if (itemPresenter.ParticipantIsPresenter)
                    {
                        itemPresenter.ParticipantItemElemt_SetPresenter(false);

                        if (!string.IsNullOrEmpty(presenterUserId))
                        {
                            var presenterItem = ParticipantItemList.FirstOrDefault(i => i.ParticipantUserId == presenterUserId);
                            presenterItem.ParticipantItemElemt_SetPresenter(true);
                        }
                    }
                }
                DisplayMessageForAuthentication("Set Presenter");
                return;
            }

            var item = ParticipantItemList.FirstOrDefault(i => i.ParticipantIsPresenter == true);
            if (item != null)
            {
                Participant participant;
                lock (_itemsLock)
                {
                    participant = participantList.First(kvp => kvp.Key == item.ParticipantUserId).Value;
                }

                if (!GetConnectorInstance.SetPresenter(participant, "SetPresenter"))
                {
                    DisplayErrorMessageForAPI("Presenter", "Failed to Set Presenter");
                }
            }
        }

        #endregion

        #region CallBack
        /*
         * Callback function
         */
        public void OnConferenceModeChanged(ConnectorConferenceMode mode)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        ConferenceModeChangedCallBackProcess(mode)));
        }

        public void OnLockRoomResult(ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        LockRoomResultCallBackProcess(result)));
        }

        public void OnUnlockRoomResult(ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        UnlockRoomResultCallBackProcess(result)));
        }

        public void OnSetRoomPINResult(ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        SetRoomPINResultCallBackProcess(result)));
        }

        public void OnRemoveRoomPINResult(ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        RemoveRoomPINResultCallBackProcess(result)));
        }

        public void OnRequestModeratorRoleResult(ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        RequestModeratorRoleResultCallBackProcess(result)));
        }

        public void OnRemoveModeratorRoleResult(ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        RemoveModeratorRoleResultCallBackProcess(result)));
        }

        public void OnModerationResult(Participant participant, ConnectorModerationResult result, ConnectorModerationActionType action, String requestId)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        ModerationResultCallBackProcess(participant, result, action, requestId)));
        }

        public void OnConnectionPropertiesChanged(ConnectorConnectionProperties connectionProperties)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        ConnectionPropertiesChangedCallBackProcess(connectionProperties)));
        }

        public void OnGetRecordingServiceProfiles(List<string> profiles, List<string> prefixes, ConnectorRecordingServiceResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        GetRecordingServiceProfilesCallBackProcess(profiles, prefixes, result)));
        }

        public void OnRecordingServiceStartResult(ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        RecordingServiceStartResultCallBackProcess(result)));
        }

        public void OnRecordingServiceStopResult(ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        RecordingServiceStopResultCallBackProcess(result)));
        }

        public void OnRecordingServicePauseResult(ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        RecordingServicePauseResultCallBackProcess(result)));
        }

        public void OnRecordingServiceResumeResult(ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        RecordingServiceResumeResultCallBackProcess(result)));
        }

        public void OnRaiseHandResponse(Participant.ParticipantHandState handState)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    RaiseHandResponseCallBackProcess(handState)));
        }

        public void OnParticipantJoined(Participant participant)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    ParticipantJoinedCallBackProcess(participant, false)));
        }

        public void OnParticipantLeft(Participant participant)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    ParticipantLeftCallBackProcess(participant)));
        }

        public void OnDynamicParticipantChanged(List<Participant> participants)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    DynamicParticipantChangedCallBackProcess(participants)));
        }

        public void OnLoudestParticipantChanged(Participant participant, bool audioOnly)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    LoudestParticipantChangedCallBackProcess(participant, audioOnly)));
        }

        public void OnHandRaised(List<Participant> participant)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    HandRaisedCallBackProcess(participant)));
        }

        public void OnPresenterChanged(Participant participant)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => 
                    PresenterChangedCallBackProcess(participant)));
        }

        public void OnRemoteCameraAdded(RemoteCamera remoteCamera, Participant participant)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                RemoteCameraAddedCallBackProcess(remoteCamera, participant)));
        }

        public void OnRemoteCameraRemoved(RemoteCamera remoteCamera, Participant participant)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                RemoteCameraRemovedCallBackProcess(remoteCamera, participant)));
        }

        public void OnRemoteCameraStateUpdated(RemoteCamera remoteCamera, Participant participant, Device.DeviceState state)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                RemoteCameraStateUpdatedCallBackProcess(remoteCamera, participant, state)));
        }

        public void OnRemoteMicrophoneAdded(RemoteMicrophone remoteMicrophone, Participant participant)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                RemoteMicrophoneAddedCallBackProcess(remoteMicrophone, participant)));
        }

        public void OnRemoteMicrophoneRemoved(RemoteMicrophone remoteMicrophone, Participant participant)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                RemoteMicrophoneRemovedCallBackProcess(remoteMicrophone, participant)));
        }

        public void OnRemoteMicrophoneStateUpdated(RemoteMicrophone remoteMicrophone, Participant participant, Device.DeviceState state)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                RemoteMicrophoneStateUpdatedCallBackProcess(remoteMicrophone, participant, state)));
        }

        public void OnLocalCameraStateUpdated(LocalCamera localCamera, Device.DeviceState state)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                LocalCameraStateUpdatedCallBackProcess(localCamera, state)));
        }

        public void OnLocalMicrophoneStateUpdated(LocalMicrophone localMicrophone, Device.DeviceState state)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                LocalMicrophoneStateUpdatedCallBackProcess(localMicrophone, state)));
        }

        public void OnModerationCommandReceived(Device.DeviceType deviceType, Room.RoomModerationType moderationType, bool state)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                ModerationCommandReceivedCallBackProcess(deviceType, moderationType, state)));
        }
        #endregion

        #region Commands
        private System.Windows.Input.ICommand _commandRemoveModeratorPIN;
        public ICommand CommandRemoveModeratorPIN
        {
            get { return GetCommand(ref _commandRemoveModeratorPIN, x => RemoveModeratorPIN()); }
        }

        private System.Windows.Input.ICommand _commandSetModeratorPIN;
        public ICommand CommandSetModeratorPIN
        {
            get { return GetCommand(ref _commandSetModeratorPIN, x => SetModeratorPIN()); }
        }

        private System.Windows.Input.ICommand _commandSetRoomPIN;
        public ICommand CommandSetRoomPIN
        {
            get { return GetCommand(ref _commandSetRoomPIN, x => SetRoomPIN()); }
        }

        private System.Windows.Input.ICommand _commandRemoveRoomPIN;
        public ICommand CommandRemoveRoomPIN
        {
            get { return GetCommand(ref _commandRemoveRoomPIN, x => RemoveRoomPIN()); }
        }

        private System.Windows.Input.ICommand _commandSetIsPresenterMode;
        public ICommand CommandSetPresenterMode
        {
            get { return GetCommand(ref _commandSetIsPresenterMode, x => SetConferenceModeAsPresenterMode()); }
        }

        private System.Windows.Input.ICommand _commandSetGroupMode;
        public ICommand CommandSetGroupMode
        {
            get { return GetCommand(ref _commandSetGroupMode, x => SetConferenceModeAsGroupMode()); }
        }

        private System.Windows.Input.ICommand _commandSetLockRoom;
        public ICommand CommandSetLockRoom
        {
            get { return GetCommand(ref _commandSetLockRoom, x => SetConferenceRoomAsLockRoom()); }
        }

        private System.Windows.Input.ICommand _commandSetUnlockRoom;
        public ICommand CommandSetUnlockRoom
        {
            get { return GetCommand(ref _commandSetUnlockRoom, x => SetConferenceRoomAsUnlockRoom()); }
        }

        private System.Windows.Input.ICommand _commandSetRoleModerator;
        public ICommand CommandSetRoleModerator
        {
            get { return GetCommand(ref _commandSetRoleModerator, x => SetModeratorRoleAsModerator()); }
        }

        private System.Windows.Input.ICommand _commandSetRoleNone;
        public ICommand CommandSetRoleNone
        {
            get { return GetCommand(ref _commandSetRoleNone, x => SetModeratorRoleAsNone()); }
        }

        private System.Windows.Input.ICommand _commandSetPresenter;
        public ICommand CommandSetPresenter
        {
            get { return GetCommand(ref _commandSetPresenter, x => SetPresenter()); }
        }

        private ICommand GetCommand(ref ICommand command, Action<object> action, bool isCanExecute = true)
        {
            if (command != null) return command;

            var cmd = new BindableCommand { IsCanExecute = isCanExecute };
            cmd.ExecuteAction += action;
            command = cmd;

            return command;
        }
         #endregion
    }
}