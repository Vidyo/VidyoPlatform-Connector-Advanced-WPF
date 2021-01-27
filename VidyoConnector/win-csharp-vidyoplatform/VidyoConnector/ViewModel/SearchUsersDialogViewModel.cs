using SearchUsersDialog.Listeners;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using VidyoClient;
using VidyoConferenceModeration.ViewModel;
using VidyoConnector.ViewModel;
using static VidyoClient.Connector;

namespace SearchUsersDialog.ViewModel
{
    public class UserItemElemt
    {
        public string SearchUserName { get; set; }
        public string SearchUserStatus { get; set; }
        public bool IsUserSelected { get; set; }

        public UserItemElemt(string userName, string userStatus, bool select )
        {
            this.SearchUserName = userName;
            this.SearchUserStatus = userStatus;
            this.IsUserSelected = select;
        }
    }

    public class SearchUsersDialogViewModel : VidyoConferenceModerationViewModel, INotifyPropertyChanged
    {
        List<KeyValuePair<int, ContactInfo>> searchUsersList;
        uint RecordsRequested;
        uint RecordsReceived;
        uint StartIndex;
        String SearchText;
        List<ContactInfo> inviteParticipantlist;
        object _itemsLock;

        public SearchUsersDialogViewModel()
        {
            SearchUserItemList = new ObservableCollection<UserItemElemt>();
            searchUsersList = new List<KeyValuePair<int, ContactInfo>>();
            inviteParticipantlist = new List<ContactInfo>();
            SearchUserResults = "Results(0)";
            RecordsRequested = 0;
            RecordsReceived = 0;
            StartIndex = 0;
            SearchText = "";

            _itemsLock = new object();
            BindingOperations.EnableCollectionSynchronization(SearchUserItemList, _itemsLock);
        }

        public void SearchUsersDialogViewModel_SearchUsers(string searchText)
        {
            RecordsRequested = 100;
            RecordsReceived = 0;
            SearchUserItemList.Clear();
            searchUsersList.Clear();
            inviteParticipantlist.Clear();
            SearchUserResults = "Results(0)";
            InviteUserStatus = "";
            SearchText = searchText;
            StartIndex = 0;

            Thread thread = new Thread(SearchUser);
            thread.Start();
        }

        public void SearchUsersDialogViewModel_InviteParticipant()
        {
            lock (_itemsLock)
            {
                inviteParticipantlist.Clear();

                int iCounter = 0;
                foreach (var item in SearchUserItemList)
                {
                    if ( (item.IsUserSelected) && ((item.SearchUserStatus != "Offline") || (item.SearchUserStatus != "Do Not Disturb")) )
                    {
                        inviteParticipantlist.Add((searchUsersList.First(kvp => kvp.Key == iCounter).Value));
                    }
                    iCounter++;
                }
            }

            InviteUser();
        }

        public void SearchUser()
        {
            if(!GetConnectorInstance.SearchUsers(SearchText, StartIndex, RecordsRequested, new SearchUsersListener(this)))
            {
                return;
            }
        }

        public void InviteUser()
        {
            bool retValue = false;
            String InvitationMessage = "Please Join";
            ContactInfo contactInfo;

            lock (_itemsLock)
            {
                if(inviteParticipantlist.Count == 0)
                {
                    return;
                }
                contactInfo = inviteParticipantlist.First();
                inviteParticipantlist.RemoveAt(0);
            }

            retValue = GetConnectorInstance.InviteParticipant(contactInfo, InvitationMessage, new InviteUserListener(this));
            if (!retValue)
            {
                String msg = "Failed to Invite User : " + contactInfo.name;
                DisplayErrorMessageForAPI("Invite User", msg);
            }
        }

        public void UserSearchResultsCallBackProcess(string searchText, uint startIndex, ConnectorSearchResult searchResult, List<ContactInfo> contacts, SizeT numRecords)
        {
            if (searchResult == ConnectorSearchResult.ConnectorsearchresultOk)
            { 
                for (int iCounter = (int)startIndex ; iCounter < contacts.Count; iCounter++)
                {
                    String status;

                    switch(contacts[iCounter].presenceState)
                    {
                        case ContactInfo.ContactInfoPresenceState.ContactinfopresencestateAvailable:
                        case ContactInfo.ContactInfoPresenceState.ContactinfopresencestateInterestedInChat:
                            status = "Online";
                            break;
                        case ContactInfo.ContactInfoPresenceState.ContactinfopresencestateAway:
                        case ContactInfo.ContactInfoPresenceState.ContactinfopresencestateExtendedAway: 
                            status = "Away";
                            break;
                        case ContactInfo.ContactInfoPresenceState.ContactinfopresencestateDoNotDisturb:
                            status = "Do Not Disturb";
                            break;
                        default:
                            status = "Offline";
                            break;
                    }

                    lock (_itemsLock)
                    {
                        searchUsersList.Add(new KeyValuePair<int, ContactInfo>(iCounter, contacts[iCounter]));
                        SearchUserItemList.Add(new UserItemElemt(contacts[iCounter].name, status, false));
                    }
                }
                
                RecordsReceived += (uint)contacts.Count;
                SearchUserResults = "Results(" + RecordsReceived.ToString() + ")";

                if ((int)numRecords > RecordsReceived)
                {
                    StartIndex = RecordsReceived;
                    Thread thread = new Thread(SearchUser);
                    thread.Start();
                }
            }
            else
            {
                String msg = "Failed to Search User User";
                DisplayMessageForCallBackResult(msg, searchResult);
            }
        }

        public void InviteResultCallBackProcess(string inviteeId, ConnectorModerationResult result)
        {
            if(result != ConnectorModerationResult.ConnectormoderationresultOK)
            {
                DisplayMessageForCallBackResult("Invite User", result);
            }
            Thread thread = new Thread(InviteUser);
            thread.Start();
        }

        public void OnUserSearchResults(string searchText, uint startIndex, ConnectorSearchResult searchResult, List<ContactInfo> contacts, SizeT numRecords)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                UserSearchResultsCallBackProcess(searchText, startIndex, searchResult, contacts, numRecords)));
        }

        public void OnInviteResult(string inviteeId, ConnectorModerationResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                InviteResultCallBackProcess(inviteeId, result)));
        }

        private string _searchUserResults;
        public string SearchUserResults
        {
            get { return _searchUserResults; }
            set
            {
                _searchUserResults = value;
                OnPropertyChanged();
            }
        }

        private string _inviteUserStatus;
        public string InviteUserStatus
        {
            get { return _inviteUserStatus; }
            set
            {
                _inviteUserStatus = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UserItemElemt> SearchUserItemList { get; set; }
    }
}
