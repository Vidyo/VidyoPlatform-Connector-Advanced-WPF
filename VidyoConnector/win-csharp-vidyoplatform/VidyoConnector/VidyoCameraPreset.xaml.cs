using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using VidyoClient;
using VidyoConnector.Model;
using VidyoConnector.ViewModel;

namespace VidyoConnector
{
    /// <summary>
    /// Interaction logic for VidyoCameraPreset.xaml
    /// </summary>
    public partial class VidyoCameraPreset : Window
    {
        object _itemsLock;
        RemoteCameraModel _camera;

        public ObservableCollection<PresetItem> VidyoCameraPresets { get; set; }
        public VidyoCameraPreset(RemoteCameraModel camera)
        {
            InitializeComponent();
            DataContext = this;
            _itemsLock = new object();
            VidyoCameraPresets = new ObservableCollection<PresetItem>();
            BindingOperations.EnableCollectionSynchronization(VidyoCameraPresets, _itemsLock);
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            _camera = camera;
            UpdatePresetData();
        }

        public void UpdatePresetData()
        {
            if (_camera.GetPresetData() != null)
            {
                for (int iCounter = 0; iCounter < (_camera.GetPresetData()).Count; iCounter++)
                {
                    CameraPreset p = (_camera.GetPresetData())[iCounter];
                    VidyoCameraPresets.Add(new PresetItem(p.index, p.name, false));
                }
            }
        }

        public void VidyoCameraActivatePresetClick(object sender, RoutedEventArgs e)
        {
            for (int iCounter = 0; iCounter < VidyoCameraPresets.Count; iCounter++)
            {
                PresetItem p = VidyoCameraPresets[iCounter];
                if (p.PresetStatus)
                {
                    if(!_camera.RemoteCamera_ActivatePreset(p.PresetIndex))
                    {
                        MessageBox.Show("Failed to Activate preset.", "Camera Preset");
                    }
                    DialogResult = true;
                }
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
    }
}
