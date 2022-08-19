using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using VidyoConnector.ViewModel;

namespace VidyoConnector
{
    /// <summary>
    /// Interaction logic for VidyoConferenceOptions.xaml
    /// </summary>
    public partial class VidyoConferenceOptions : Window
    {
        object _itemsLock;

        public Func<string, bool> setOptions;
        public Func<string> getOptions;

        public ObservableCollection<OptionItem> Options { get; set; }
        
        public VidyoConferenceOptions(string options)
        {
            InitializeComponent();
            DataContext = this;
            _itemsLock = new object();
            Options = new ObservableCollection<OptionItem>();
            BindingOperations.EnableCollectionSynchronization(Options, _itemsLock);
			
			WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private string GetOptionsDescription(string optionTag)
        {
            switch(optionTag)
            {
                case "statRate":
                    return "Statistics Interval";
                case "audioSharedModeBoth":
                    return "Audio Shared Mode Both";
                case "audioExclusiveModeBoth":
                    return "Audio Exclusive Mode Both";
                case "audioExclusiveModeMic":
                    return "Audio Exclusive Mode Mic";
                case "AudioPacketInterval":
                    return "Audio Packet Interval";
                case "AudioPacketLossPercentage":
                    return "Audio Packet Loss (%)";
                case "AudioBitrateMultiplier":
                    return "Audio Bitrate Multiplier";
                case "conferenceReferenceNumber":
                    return "Conference Reference Number";
                case "minMicrophoneVolume":
                    return "Microphone Volume (Minimum)";
                case "microphoneMaxBoostLevel":
                    return "Microphone Boost Level (Max)";
                case "preferredAudioCodec":
                    return "Preferred Codec (Audio)";
                case "enableStaticShareSvc":
                    return "Static share SVC enabled";
                case "maxReconnectAttempts":
                    return "Max Reconnect Attempts";
                case "enableAutoReconnect":
                    return "Auto Reconnect enabled";
                case "reconnectBackoff":
                    return "Reconnect Back Off";
                default:
                    return "Invalid Options";
            }
        }

        private bool CheckForAudioSharedAndExclusiveMode(string optionTag)
        {
            if( (optionTag== "audioSharedModeBoth") || (optionTag == "audioExclusiveModeBoth") || (optionTag == "audioExclusiveModeMic"))
            {
                return true;
            }
            return false;
        }

        private void UpdateOptions()
        {
            var list = Regex.Replace(getOptions(), @"[^\w\:\, ]", "").Split(',').ToList();
            Options.Clear();
            SelectAudioMode.IsChecked = false;

            foreach (var item in list)
            {
                var obj = item.Split(':');
                if (!CheckForAudioSharedAndExclusiveMode(obj[0]))
                {
                    Options.Add(new OptionItem(obj[0], GetOptionsDescription(obj[0]), obj[1], false));
                }
                else
                {
                    if ((obj[0] == "audioSharedModeBoth") && (obj[1] == "true"))
                    {
                        RadioButtonAudioShared.IsChecked = true;
                        RadioButtonAudioExclusiveModeMic.Visibility = Visibility.Collapsed;
                        RadioButtonAudioExclusiveModeBoth.Visibility = Visibility.Collapsed;
                    }
                    else if (((obj[0] == "audioExclusiveModeBoth") && (obj[1] == "true")) ||
                        ((obj[0] == "audioExclusiveModeMic") && (obj[1] == "true")))
                    {
                        RadioButtonAudioExclusive.IsChecked = true;
                        RadioButtonAudioExclusiveModeMic.IsChecked = false;
                        RadioButtonAudioExclusiveModeBoth.IsChecked = false;
                        RadioButtonAudioExclusiveModeMic.Visibility = Visibility.Visible;
                        RadioButtonAudioExclusiveModeBoth.Visibility = Visibility.Visible;

                        if ((obj[0] == "audioExclusiveModeMic") && (obj[1] == "true"))
                        {
                            RadioButtonAudioExclusiveModeMic.IsChecked = true;
                        }
                        if ((obj[0] == "audioExclusiveModeBoth") && (obj[1] == "true"))
                        {
                            RadioButtonAudioExclusiveModeBoth.IsChecked = true;
                        }
                    }
                }
            }
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
        
        public new void Show()
        {
            UpdateOptions();
            base.ShowDialog();
        }

        private string ParceStringValue(string optionValue)
        {
            if (bool.TryParse(optionValue, out bool noneBool))
                return optionValue;
            else if (int.TryParse(optionValue, out int noneInt))
                return optionValue;
            return ("\"" + optionValue + "\"");
        }

        private void ParceAndApplySelectedOptions(object sender, RoutedEventArgs e)
        {
            bool isSelect = false;
            string options = "{\"";
            for (int i = 0, selected = 0; i < Options.Count; ++i)
            {
                if (Options[i].OptionStatus)
                {
                    options += (selected != 0 ? ",\"" : "") + Options[i].OptionName + "\":" + ParceStringValue(Options[i].OptionValue);
                    ++selected;
                    isSelect = true;
                }
            }

            if ((bool)SelectAudioMode.IsChecked)
            {
                if(isSelect)
                {
                    options += ",\"";
                }
                options += "audioSharedModeBoth" + "\":";
                options += (bool)RadioButtonAudioShared.IsChecked ? "true" : "false";

                options += ",\"";
                options += "audioExclusiveModeBoth" + "\":";
                options += (bool)RadioButtonAudioExclusiveModeBoth.IsChecked ? "true" : "false";

                options += ",\"";
                options += "audioExclusiveModeMic" + "\":";
                options += (bool)RadioButtonAudioExclusiveModeMic.IsChecked ? "true" : "false";

                isSelect = true;
            }

            options += "}";

            if (isSelect)
            {
                if (setOptions(options))
                {
                    this.Visibility = Visibility.Hidden;
                    return;
                }
                MessageBox.Show("Failed to set options.", "Set Options");
            }
            else
            {
                MessageBox.Show("Please select any one option to set.", "Set Options");
            }
        }

        private void RadioButtonAudioExclusive_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonAudioExclusiveModeMic.Visibility = Visibility.Visible;
            RadioButtonAudioExclusiveModeBoth.Visibility = Visibility.Visible;
            RadioButtonAudioExclusiveModeMic.IsChecked = true;
        }

        private void RadioButtonAudioShared_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonAudioExclusiveModeMic.Visibility = Visibility.Collapsed;
            RadioButtonAudioExclusiveModeBoth.Visibility = Visibility.Collapsed;
            RadioButtonAudioExclusiveModeMic.IsChecked = false;
            RadioButtonAudioExclusiveModeBoth.IsChecked = false;
        }
    }
}
