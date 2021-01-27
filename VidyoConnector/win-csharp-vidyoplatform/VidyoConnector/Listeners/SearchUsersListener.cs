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
    class SearchUsersListener : ListenerBase, ISearchUsers
    {
        public SearchUsersListener(SearchUsersDialogViewModel viewModel) : base(viewModel) { }

        public void OnUserSearchResults(string searchText, uint startIndex, ConnectorSearchResult searchResult, List<ContactInfo> contacts, SizeT numRecords)
        {
            SearchUsersDialogViewModel.OnUserSearchResults(searchText, startIndex, searchResult, contacts, numRecords);
        }
    }
}
