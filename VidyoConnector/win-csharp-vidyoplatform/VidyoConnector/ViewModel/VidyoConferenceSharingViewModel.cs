using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using VidyoClient;
using VidyoConnector.Listeners;
using VidyoConnector.Model;

namespace VidyoConnector.ViewModel
{
    public class ShareDeviceItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string shareDeviceName { get; set; }
        public string ShareDeviceName
        {
            get { return shareDeviceName; }
            set
            {
                shareDeviceName = value; 
                RaiseOnPropertyChanged("ShareDeviceName");
            }
        }

        private string shareDeviceId { get; set; }
        public string ShareDeviceId
        {
            get { return shareDeviceId; }
            set
            {
                shareDeviceId = value; 
                RaiseOnPropertyChanged("ShareDeviceId");
            }
        }

        private bool shareDeviceSelectStatus { get; set; }
        public bool ShareDeviceSelectStatus
        {
            get { return shareDeviceSelectStatus; }
            set
            {
                shareDeviceSelectStatus = value; 
                RaiseOnPropertyChanged("ShareDeviceSelectStatus");
            }
        }

        private bool shareDeviceExcludeStatus { get; set; }
        public bool ShareDeviceExcludeStatus
        {
            get { return shareDeviceExcludeStatus; }
            set
            {
                shareDeviceExcludeStatus = value;
                RaiseOnPropertyChanged("ShareDeviceExcludeStatus");
            }
        }

        private bool shareDeviceExcludeEnabled { get; set; }
        public bool ShareDeviceExcludeEnabled
        {
            get { return shareDeviceExcludeEnabled; }
            set
            {
                shareDeviceExcludeEnabled = value;
                RaiseOnPropertyChanged("ShareDeviceExcludeEnabled");
            }
        }        

        private bool shareDeviceCanBeShared { get; set; }
        public bool ShareDeviceCanBeShared
        {
            get { return shareDeviceCanBeShared; }
            set
            {
                shareDeviceCanBeShared = value;
                RaiseOnPropertyChanged("ShareDeviceCanBeShared");
            }
        }

        public ShareDeviceItem(string deviceName, string deviceId, bool deviceSelectStatus, bool deviceExcludeEnabled, bool canBeShared)
        {
            this.ShareDeviceName = deviceName;
            this.ShareDeviceId = deviceId;
            this.ShareDeviceSelectStatus = deviceSelectStatus;
            this.ShareDeviceExcludeEnabled = deviceExcludeEnabled;
            this.ShareDeviceExcludeStatus = false;
            this.ShareDeviceCanBeShared = canBeShared;
        }

        public void RaiseOnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VidyoConnectorShareViewModel : VidyoConnectorViewModel, INotifyPropertyChanged
    {
        object _itemsLock;
        public delegate void ShareModelMonitorNotify(LocalMonitorModel monitor);
        public delegate void ShareModelWindowNotify(LocalWindowShareModel window);

        public bool isMonitorSharing;
        public bool isWindowSharing;

        public ObservableCollection<LocalWindowShareModel> LocalWindows { get; set; }
        public ObservableCollection<LocalMonitorModel> LocalMonistors { get; set; }
        public ShareModelMonitorNotify notifyMonitorAdded { get; set; }
        public ShareModelMonitorNotify notifyMonitorRemoved { get; set; }
        public ShareModelWindowNotify notifyWindowAdded { get; set; }
        public ShareModelWindowNotify notifyWindowRemoved { get; set; }
        public ShareModelWindowNotify notifyWindowStateUpdated { get; set; }

        public VidyoConnectorShareViewModel()
        {
            _itemsLock = new object();

            LocalWindows = new ObservableCollection<LocalWindowShareModel>();
            LocalMonistors = new ObservableCollection<LocalMonitorModel>();

            BindingOperations.EnableCollectionSynchronization(LocalWindows, _itemsLock);
            BindingOperations.EnableCollectionSynchronization(LocalMonistors, _itemsLock);
        }

        internal void Init()
        {
            isMonitorSharing = false;
            isWindowSharing = false;
            GetConnectorInstance.RegisterLocalMonitorEventListener(new LocalMonitorListener(this));
            GetConnectorInstance.RegisterLocalWindowShareEventListener(new LocalWindowShareListener(this));
        }

        internal void Uninit()
        {
            if (isMonitorSharing)
                StopMonitorShare();

            if (isWindowSharing)
                StopWindowShare();

            isMonitorSharing = false;
            isWindowSharing = false;

            GetConnectorInstance.UnregisterLocalMonitorEventListener();
            GetConnectorInstance.UnregisterLocalWindowShareEventListener();
        }

        public bool SelectMonitor(string monitorId)
        {
            LocalMonitorModel monitor = LocalMonistors.FirstOrDefault(x => x.Id.Equals(monitorId));

            if (monitor != null) {
                SetSelectedLocalMonitor(monitor);
            }
            return (monitor != null);
        }

        public bool SelectWindow(string windowId)
        {
            LocalWindowShareModel window = LocalWindows.FirstOrDefault(x => x.Id.Equals(windowId));

            if (window != null)
            {
                SetSelectedLocalWindow(window);
            }
            return (window != null);
        }

        public bool ExcludeWindow(string windowId, bool exclude)
        {
            var monitor = LocalMonistors.FirstOrDefault(x => x.IsSelected);
            if (monitor != null && monitor.Object != null) {

                LocalWindowShareModel window = LocalWindows.FirstOrDefault(x => x.Id.Equals(windowId));
                if (window != null && window.Object != null)
                    return monitor.Object.ExcludeWindow(window.Object, exclude);                
            }

            return false;
        }

        /*
         * This section contains functionality for local monitors (sreens) control:
         *  -   Adding / removing monitor if such has been plugged in / plugged out
         *  -   Selecting / deselecting specific monitor share on UI and on API level
         *  -   Starting share (used in command binding)
         */
        #region LocalMonitors

        public void AddLocalMonitor(LocalMonitorModel monitor)
        {
            if (LocalMonistors.FirstOrDefault(x => x.Id.Equals(monitor.Id)) == null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => LocalMonistors.Add(monitor));
                notifyMonitorAdded?.Invoke(monitor);

                Log.Info(string.Format("Added local monitor: name={0} id={1}", monitor.DisplayName, monitor.Id));
            }
        }

        public void RemoveLocalMonitor(LocalMonitorModel monitor)
        {
            var monitorToRemove = LocalMonistors.FirstOrDefault(x => x.Id.Equals(monitor.Id));
            if (monitorToRemove != null)
            {
                System.Windows.Application.Current.Dispatcher.InvokeAsync(() => { LocalMonistors.Remove(monitorToRemove); });
                notifyMonitorRemoved?.Invoke(monitor);

                Log.Info(string.Format("Removed local monitor: name={0} id={1}", monitorToRemove.DisplayName, monitorToRemove.Id));
            }            
        }

        private void SetSelectedLocalMonitor(LocalMonitorModel monitor)
        {
            LocalMonistors.Select(x =>
            {
                x.IsSelected = false;
                return x;
            }).ToList();

            var monitorToSelect = LocalMonistors.FirstOrDefault(x => x.Id.Equals(monitor.Id));
            if (monitorToSelect != null)
            {
                monitorToSelect.IsSelected = true;
                Log.Info(string.Format("Local window selected: name={0} id={1}", monitorToSelect.DisplayName, monitorToSelect.Id));
            }
        }

        public void SetSelectedLocalMonitor(object monitorModelObj)
        {
            var monitor = monitorModelObj as LocalMonitorModel;
            if (monitor != null)
            {
                SetSelectedLocalMonitor(monitor);
            }
        }
       
        public bool StartLocalMonitorShare(bool enableAudioCapturing, bool enableHighFramerate)
        {
            bool ret = false;
            var monitor = LocalMonistors.FirstOrDefault(x => x.IsSelected);

            if (monitor != null) {
                ConnectorShareOptions options = new ConnectorShareOptions(IntPtr.Zero);
                options.enableAudio = enableAudioCapturing;
                options.enableHighFramerate = enableHighFramerate;

                if ((ret = GetConnectorInstance.SelectLocalMonitorAdvanced(monitor.Object, options)))
                {
                    isMonitorSharing = true;
                    Log.Info(string.Format("Local monitor {0}({1}) sharing started", monitor.DisplayName, monitor.Id));
                }
                else
                    Log.Info("Start local window sharing failed");
            }
            return ret;
        }

        public void StopMonitorShare()
        {
            GetConnectorInstance.SelectLocalMonitorAdvanced(null, null);
        }

        #endregion

        /*
         * This section contains functionality for local application windows control:
         *  -   Adding / removing application windows if such has been opened / closed
         *  -   Selecting / deselecting specific windows share on UI and on API level
         *  -   Starting share (used in command binding)
         */
        #region LocalWindows

        public void AddLocalWindow(LocalWindowShareModel window)
        {
            if (LocalWindows.FirstOrDefault(x => x.Id.Equals(window.Id)) == null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => LocalWindows.Add(window));
                notifyWindowAdded?.Invoke(window);

                Log.Info(string.Format("Added local window: name={0} id={1}", window.DisplayName, window.Id));
            }
        }

        public void RemoveLocalWindow(LocalWindowShareModel window)
        {
            var winToRemove = LocalWindows.FirstOrDefault(x => x.Id.Equals(window.Id));
            if (winToRemove != null)
            {
                System.Windows.Application.Current.Dispatcher.InvokeAsync(() => { LocalWindows.Remove(winToRemove); });
                notifyWindowRemoved?.Invoke(window);

                Log.Info(string.Format("Removed local window: name={0} id={1}", winToRemove.DisplayName, winToRemove.Id));
            }
        }

        public void UpdateLocalWindowState(LocalWindowShareModel window)
        {
            var winToUpdate = LocalWindows.FirstOrDefault(x => x.Id.Equals(window.Id));

            if (winToUpdate != null)
            {
                notifyWindowStateUpdated?.Invoke(window);
            }
        }

        private void SetSelectedLocalWindow(LocalWindowShareModel window)
        {
            LocalWindows.Select(x =>
            {
                x.IsSelected = false;
                return x;
            }).ToList();

            var winToSelect = LocalWindows.FirstOrDefault(x => x.Id.Equals(window.Id));
            if (winToSelect != null)
            {
                winToSelect.IsSelected = true;
                Log.Info(string.Format("Local window selected: name={0} id={1}", winToSelect.DisplayName, winToSelect.Id));
            }
        }

        public void SetSelectedLocalWindow(object winModelObj)
        {
            var win = winModelObj as LocalWindowShareModel;
            if (win != null)
                SetSelectedLocalWindow(win);
        }

        public bool StartLocalWindowShare(bool enableAudioCapturing, bool enableHighFramerate)
        {
            bool ret = false;
            var window = LocalWindows.FirstOrDefault(x => x.IsSelected);

            if (window != null) {

                ConnectorShareOptions options = new ConnectorShareOptions(IntPtr.Zero);
                options.enableAudio = enableAudioCapturing;
                options.enableHighFramerate = enableHighFramerate;

                if ((ret = GetConnectorInstance.SelectLocalWindowShareAdvanced(window.Object, options)))
                {
                    isWindowSharing = true;
                    Log.Info(string.Format("Local window {0}({1}) sharing started", window.DisplayName, window.Id));
                }
                else
                    Log.Info("Start local window sharing failed");
            }
            return ret;
        }

        public void StopWindowShare()
        {
            GetConnectorInstance.SelectLocalWindowShareAdvanced(null, null);
        }

        #endregion

    }
}
