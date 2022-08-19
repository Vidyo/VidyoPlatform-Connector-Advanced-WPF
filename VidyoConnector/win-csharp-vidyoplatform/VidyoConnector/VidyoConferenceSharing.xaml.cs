using System.Windows;
using System.Windows.Data;
using VidyoConnector.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using VidyoConnector.Model;
using System.Linq;
using VidyoClient;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace VidyoConnector
{
    /// <summary>
    /// Interaction logic for VidyoConferenceSharing.xaml
    /// </summary>
    public partial class VidyoConferenceSharing : Window
    {
        private readonly object _itemsLock;

        VidyoConnectorShareViewModel shareViewModel;
        public ObservableCollection<ShareDeviceItem> ShareMonitors { get; set; }
        public ObservableCollection<ShareDeviceItem> ShareWindows { get; set; }

        public VidyoConferenceSharing()
        {
            InitializeComponent();
            DataContext = this;

            _itemsLock = new object();

            ShareMonitors = new ObservableCollection<ShareDeviceItem>();
            ShareWindows = new ObservableCollection<ShareDeviceItem>();

            BindingOperations.EnableCollectionSynchronization(ShareMonitors, _itemsLock);
            BindingOperations.EnableCollectionSynchronization(ShareWindows, _itemsLock);            

            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        public bool Init()
        {
            if (shareViewModel != null) {
                ResetConferenceShareConfigs();
                return true;
            }
            else
                MessageBox.Show("The share view model is not constructed", "Vidyo Connector");
            return false;
        }

        public void SetConferenceShareViewModel(VidyoConnectorShareViewModel view)
        {
            if ((shareViewModel = view) != null) {
                shareViewModel.notifyMonitorAdded = AddMonitor;
                shareViewModel.notifyWindowAdded = AddWindow;
                shareViewModel.notifyMonitorRemoved = RemoveMonitor;
                shareViewModel.notifyWindowRemoved = RemovedWindow;
                shareViewModel.notifyWindowStateUpdated = UpdateWindowState;
            }
        }

        private void AddMonitor(LocalMonitorModel monitor)
        {
            if (ShareMonitors.FirstOrDefault(x => x.ShareDeviceId.Equals(monitor.Id)) == null)
                ShareMonitors.Add(new ShareDeviceItem(monitor.DisplayName, monitor.Id, monitor.IsSelected, false, true));
        }

        private void AddWindow(LocalWindowShareModel window)
        {
            if (ShareWindows.FirstOrDefault(x => x.ShareDeviceId.Equals(window.Id)) == null) {
                ShareDeviceItem item = new ShareDeviceItem(window.DisplayName, window.Id, window.IsSelected, false, !window.IsMinimized);
                item.ShareDeviceExcludeEnabled = (shareViewModel.isMonitorSharing && IsAvailableForExclude(item));
                ShareWindows.Add(item);
            }
        }

        private void RemoveMonitor(LocalMonitorModel monitor)
        {
            ShareDeviceItem obj = ShareMonitors.FirstOrDefault(x => x.ShareDeviceId.Equals(monitor.Id));

            if (obj != null)
                ShareMonitors.Remove(obj);
        }

        private void RemovedWindow(LocalWindowShareModel window)
        {
            ShareDeviceItem obj = ShareWindows.FirstOrDefault(x => x.ShareDeviceId.Equals(window.Id));

            if (obj != null)
                ShareWindows.Remove(obj);
        }

        private void UpdateWindowState(LocalWindowShareModel window)
        {
            var windowShare = ShareWindows.FirstOrDefault(x => x.ShareDeviceId.Equals(window.Id));
            if (windowShare != null)
            {
                windowShare.ShareDeviceCanBeShared = !window.IsMinimized;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void MonitorSelectStatus_Click(object sender, RoutedEventArgs e)
        {
            ShareDeviceItem item = ShareMonitors.FirstOrDefault(x => x.ShareDeviceSelectStatus);

            if (item != null)
                shareViewModel.SelectMonitor(item.ShareDeviceId);
        }

        private void WindowSelectStatus_Click(object sender, RoutedEventArgs e)
        {
            ShareDeviceItem item = ShareWindows.FirstOrDefault(x => x.ShareDeviceSelectStatus);
            
            if (item != null)
            {
                LocalWindowShareModel windowShare = shareViewModel.LocalWindows.FirstOrDefault(x => x.Id == item.ShareDeviceId);
                if (windowShare != null)
                {
                    if (windowShare.IsMinimized)
                    {
                        item.ShareDeviceSelectStatus = false;
                        MessageBox.Show("Cannot share a minimized window", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        shareViewModel.SelectWindow(item.ShareDeviceId);
                    }
                }
            }
        }

        private void ResetWindowsShareConfigs()
        {
            System.Collections.Generic.List<ShareDeviceItem> items = ShareWindows.Where(x => x.ShareDeviceSelectStatus == true).ToList();

            foreach (ShareDeviceItem item in items) {
                item.ShareDeviceSelectStatus = false;
            }
        }

        private void ResetExcludeForWindows()
        {
            ExcludeFromCapturer.IsChecked = false;
            foreach (ShareDeviceItem window in ShareWindows) {

                window.ShareDeviceExcludeEnabled = false;
                if (window.ShareDeviceExcludeStatus) {
                    shareViewModel.ExcludeWindow(window.ShareDeviceId, false);
                    window.ShareDeviceExcludeStatus = false;
                }
            }

        }

        private void ResetMonitorsShareConfigs()
        {
            ResetExcludeForWindows();

            System.Collections.Generic.List<ShareDeviceItem> monitors = ShareMonitors.Where(x => x.ShareDeviceSelectStatus == true).ToList();
            foreach (ShareDeviceItem item in monitors)
                item.ShareDeviceSelectStatus = false;
        }

        private void ResetConferenceShareConfigs()
        {
            if (!shareViewModel.isWindowSharing && !shareViewModel.isMonitorSharing) {
                ResetWindowsShareConfigs();
                ResetMonitorsShareConfigs();

                EnableSystemAudio.IsChecked = false;
                EnableHighFrameRateSharing.IsChecked = false;
            }
        }

        private void StartStopWindowShare_Click(object sender, RoutedEventArgs e)
        {
            if (!shareViewModel.isWindowSharing && ShareWindows.FirstOrDefault(x => x.ShareDeviceSelectStatus) != null)
                shareViewModel.isWindowSharing = shareViewModel.StartLocalWindowShare(EnableSystemAudio.IsChecked.Value, EnableHighFrameRateSharing.IsChecked.Value);
            else {
                ShareDeviceItem item = ShareWindows.FirstOrDefault(x => x.ShareDeviceSelectStatus);
                if (item != null) {
                    ResetWindowsShareConfigs();
                    shareViewModel.StopWindowShare();
                    item.ShareDeviceSelectStatus = false;
                    shareViewModel.isWindowSharing = false;
                }
            }

            StartStopWindowShare.Content = shareViewModel.isWindowSharing ? "Stop window share" : "Start window share";
        }

        private void StartStopMonitorShare_Click(object sender, RoutedEventArgs e)
        {
            if (!shareViewModel.isMonitorSharing && ShareMonitors.FirstOrDefault(x => x.ShareDeviceSelectStatus) != null)
                shareViewModel.isMonitorSharing = shareViewModel.StartLocalMonitorShare(EnableSystemAudio.IsChecked.Value, EnableHighFrameRateSharing.IsChecked.Value);
            else {
                ShareDeviceItem item = ShareMonitors.FirstOrDefault(x => x.ShareDeviceSelectStatus);
                if (item != null) {
                    ResetMonitorsShareConfigs();
                    shareViewModel.StopMonitorShare();
                    item.ShareDeviceSelectStatus = false;
                    shareViewModel.isMonitorSharing = false;
                }
            }

            ExcludeFromCapturer.IsEnabled = shareViewModel.isMonitorSharing;
            StartStopMonitorShare.Content = shareViewModel.isMonitorSharing ? "Stop monitor share" : "Start monitor share";
        }

        private bool IsAvailableForExclude(ShareDeviceItem window)
        {
            bool ret = false;
            if (ret = shareViewModel.ExcludeWindow(window.ShareDeviceId, true))
                shareViewModel.ExcludeWindow(window.ShareDeviceId, false);
            return ret;
        }

        private void ExcludeFromCapturer_Click(object sender, RoutedEventArgs e)
        {
            System.Collections.Generic.List<ShareDeviceItem> windows = ShareWindows.Where(x => (x.ShareDeviceExcludeEnabled == !ExcludeFromCapturer.IsChecked.Value)).ToList();
            foreach (ShareDeviceItem window in windows)
                window.ShareDeviceExcludeEnabled = (ExcludeFromCapturer.IsChecked.Value && IsAvailableForExclude(window));
        }

        private void WindowExclude_Click(object sender, RoutedEventArgs e)
        {
            ShareDeviceItem window = (sender as CheckBox).DataContext as ShareDeviceItem;
            if (window != null) {
                if (!shareViewModel.ExcludeWindow(window.ShareDeviceId, window.ShareDeviceExcludeStatus)) {
                    window.ShareDeviceExcludeStatus = false;
                    MessageBox.Show("Not able to exclude current top-level window from capturer.");
                }
            }
        }
    }
}
