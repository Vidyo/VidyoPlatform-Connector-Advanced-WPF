using System;
using System.Reflection;
using System.Windows;
using VidyoClient;
using VidyoConnector.Listeners;
using VidyoConferenceModeration.ViewModel;
using System.Collections.Generic;
using static VidyoClient.Connector;
using SearchUsersDialog.ViewModel;

namespace SearchUsersDialog.Listeners
{
    class InviteUserListener : ListenerBase, IInviteParticipant
    {
        public InviteUserListener(SearchUsersDialogViewModel viewModel) : base(viewModel) { }

        public void OnInviteResult(string inviteeId, ConnectorModerationResult result)
        {
            SearchUsersDialogViewModel.OnInviteResult(inviteeId, result);
        }
    }
}
