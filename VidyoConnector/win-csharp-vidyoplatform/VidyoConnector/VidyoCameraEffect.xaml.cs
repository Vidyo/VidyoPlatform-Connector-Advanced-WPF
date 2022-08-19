using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using VidyoCameraEffect.ViewModel;

namespace VidyoConnector
{
    public partial class VidyoCameraEffect : Window
    {
        VidyoCameraEffectViewModel cameraEffectView;
        List<VirtualBackgroundPreview> virtualBackgroundPreviews;

        public VidyoCameraEffect()
        {
            InitializeComponent();
            cameraEffectView = new VidyoCameraEffectViewModel();
            virtualBackgroundPreviews = new List<VirtualBackgroundPreview>();
            DataContext = cameraEffectView;
        }

        public void Init()
        {
            cameraEffectView.Init();
        }

        private void ButtonApplyBackground_Click(object sender, RoutedEventArgs e)
        {
            if (this.RadioButtonNone.IsChecked.Value)
            {
                cameraEffectView.NoneBackgroundSelect();
            }
            else if (this.RadioButtonBlur.IsChecked.Value)
            {
                cameraEffectView.BlurBackgroundSelect((uint)this.SliderBlurIntensity.Value);
            }
            else if (this.RadioVirutalBackground.IsChecked.Value)
            {
                VirtualBackgroundPreview preview = virtualBackgroundPreviews.FirstOrDefault(p => p.IsSelected);
                if (preview == null)
                {
                    MessageBox.Show("Please select a virtual background", "Virtual Background", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                string virtualBackgroundPath = preview.ImagePath;
                cameraEffectView.VirtualBackgroundSelect(virtualBackgroundPath);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void RadioVirutalBackground_Checked(object sender, RoutedEventArgs e)
        {
            List<string> virtualBackgrounds = cameraEffectView.GetAvailableVirtualBackgrounds();
            StackPanelVirtualBackgrounds.Children.Clear();
            virtualBackgroundPreviews.Clear();
            foreach (string virtualBackgroundPath in virtualBackgrounds)
            {
                string virtualBackgroundName = Path.GetFileNameWithoutExtension(virtualBackgroundPath);
                VirtualBackgroundPreview virtualBackgroundPreview = new VirtualBackgroundPreview(virtualBackgroundName, virtualBackgroundPath);
                virtualBackgroundPreviews.Add(virtualBackgroundPreview);
                StackPanelVirtualBackgrounds.Children.Add(virtualBackgroundPreview);
            }
        }
    }
}
