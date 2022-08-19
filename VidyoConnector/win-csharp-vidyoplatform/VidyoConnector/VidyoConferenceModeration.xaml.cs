using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VidyoConnector.ViewModel;
using VidyoConferenceModeration.ViewModel;
using VidyoConnector;
using System.ComponentModel;
using SearchUsersDialog;

namespace VidyoConferenceModeration
{
    /// <summary>
    /// Interaction logic for VidyoConferenceModeration.xaml
    /// </summary>
    /// 
    public partial class ConferenceModerationWindow : Window
    {
        VidyoConferenceModerationViewModel viewModel;
        ParticipantCommandType participantCommand;


        public ConferenceModerationWindow(object context)
        {
            InitializeComponent();
            viewModel = new VidyoConferenceModerationViewModel(context);
            DataContext = viewModel;
            viewModel.CloseAction = new Action(() => this.Close());
            participantCommand = ParticipantCommandType.UnknownCommand;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }
        public void Init()
        {
            ((VidyoConferenceModerationViewModel)DataContext).Init(this);
        }

        public VidyoConferenceModerationViewModel GetVidyoConferenceModerationViewModel()
        {
            return viewModel;
        }

        private void RecordingProfile_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Recording clicked: " + sender.ToString());
        }

        private void ListBoxRecordingProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_SetRecordingProfileIndex(ListBoxRecordingProfile.SelectedIndex);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void ImageHandStatus_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_RaiseUnraiseHand();
        }

        private void ImageRecordingStart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_StartRecording();
        }

        private void ImageRecordingPauseResume_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_PauseResumeRecording();
        }

        private void ImageRecordingStop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_StopRecording())
            {
                ListBoxRecordingProfile.SelectedIndex = -1;
            }
        }

        private void ImageRemovePresenter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_RemovePresenter();
        }

        private void ImageDismissAllRaisedHand_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_DismissAllRaisedHand();
        }

        private void ImageHardMuteAudioAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_HardMuteAudioAll();
        }

        private void ImageHardMuteVideoAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_HardMuteVideoAll();
        }

        private void ImageSoftMuteAudioAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_SoftMuteAudioAll();
        }

        private void ImageSoftMuteVideoAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_SoftMuteVideoAll();
        }
        private void ImageDropAllParticipant_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_DropAllParticipant();
        }

        private void ListBoxParticipantList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ListViewParticipantList.SelectedIndex == -1)
            {
                return;
            }
            if (participantCommand != ParticipantCommandType.UnknownCommand)
            { 
                String participantUserId;

                ParticipantItemElemt obj = ListViewParticipantList.SelectedItem as ParticipantItemElemt;
                if (obj != null)
                {
                    participantUserId = obj.ParticipantUserId;
                    ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_ParticipantCommand(participantUserId, participantCommand);
                }
            }
            ListViewParticipantList.SelectedIndex = -1;
            participantCommand = ParticipantCommandType.UnknownCommand;
        }

        private void ImageParticipantSendPrivateMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            participantCommand = ParticipantCommandType.SendPrivateMsgCommand;
        }

        private void ImageParticipantHand_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            participantCommand = ParticipantCommandType.HandCommand;
        }

        private void ImageParticipantMicrophone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            participantCommand = ParticipantCommandType.MicCommand;
        }

        private void ImageParticipantCamera_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            participantCommand = ParticipantCommandType.CameraCommand;
        }

        private void LabelCameraPreset_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            participantCommand = ParticipantCommandType.CameraPresetCommand;
        }

        private void LabelCameraVisca_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            participantCommand = ParticipantCommandType.CameraViscaCommand;
        }

        private void ImageParticipantDisconnect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            participantCommand = ParticipantCommandType.DisconnectCommand;
        }

        private void LabelAddParticipant_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).VidyoConferenceModerationViewModel_SearchUsers();
        }

        private void PasswordBoxModeratorPIN_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).ModeratorPIN = PasswordBoxModeratorPIN.Password;
        }

        private void PasswordBoxRoomPIN_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((VidyoConferenceModerationViewModel)DataContext).RoomPIN = PasswordBoxRoomPIN.Password;
        }

        public void SetIsModerationPIN(bool status)
        {
            if(status)
                PasswordBoxModeratorPIN.Password = "******";
            else
                PasswordBoxModeratorPIN.Password = "";
        }

        public void SetIsRoomPIN(bool status)
        {
            if (status)
                PasswordBoxRoomPIN.Password = "******";
            else
                PasswordBoxRoomPIN.Password = "";
        }
    }
}
