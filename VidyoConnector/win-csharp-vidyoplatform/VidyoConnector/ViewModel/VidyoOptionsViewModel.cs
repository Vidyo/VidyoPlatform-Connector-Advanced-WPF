using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace VidyoConnector.ViewModel
{
    public class OptionItem : INotifyPropertyChanged
    {
        public string OptionName { get; set;  }
        public string OptionDescription { get; set; }
        public string OptionValue { get; set; }
        public bool OptionStatus { get; set; }

        public OptionItem(string optionName, string optionDescription, string optionValue, bool optionStatus)
        {
            this.OptionName = optionName;
            this.OptionDescription = optionDescription;
            this.OptionValue = optionValue;
            this.OptionStatus = optionStatus;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
