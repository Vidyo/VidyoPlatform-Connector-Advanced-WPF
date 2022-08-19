using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VidyoConnector.ViewModel
{
    public class PresetItem : INotifyPropertyChanged
    {
        public uint PresetIndex { get; set; }
        public string PresetName { get; set; }
        public bool PresetStatus { get; set; }

        public PresetItem(uint presetIndex, string presetName, bool presetStatus)
        {
            this.PresetIndex = presetIndex;
            this.PresetName = presetName;
            this.PresetStatus = presetStatus;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
