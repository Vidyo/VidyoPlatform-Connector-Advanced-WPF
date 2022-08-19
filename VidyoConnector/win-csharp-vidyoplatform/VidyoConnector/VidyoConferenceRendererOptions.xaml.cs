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
    public partial class VidyoConferenceRendererOptions : Window
    {
        public Func<string, bool> setRendererOptions;
        public Func<string> getRendererOptions;

        public VidyoConferenceRendererOptions()
        {
            InitializeComponent();
            DataContext = this;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void ButtonApplyRendererOptions_Click(object sender, RoutedEventArgs e)
        {
            string options = "{\"";
            bool isSelected = false;

            if ((bool)CheckboxFECCIconCustomLayout.IsChecked)
            {
                options += "EnableFECCIconCustomLayout" + "\":";
                options += ComboBoxFECCIconCustomLayout.SelectionBoxItem.ToString();
                isSelected = true;
            }
            
            if ((bool)CheckboxVerticalVideoCentering.IsChecked)
            {
                if (isSelected)
                {
                    options += ",\"";
                }
                options += "EnableVerticalVideoCentering" + "\":";
                options += ComboBoxVerticalVideoCentering.SelectionBoxItem.ToString();
                isSelected = true;
            }

            if ((bool)CheckboxViewingDistance.IsChecked)
            {
                if (isSelected)
                {
                    options += ",\"";
                }
                options += "ViewingDistance" + "\":";
                options += TextBoxViewingDistance.Text;
                isSelected = true;
            }

            if ((bool)CheckboxShowAudioTiles.IsChecked)
            {
                if (isSelected)
                {
                    options += ",\"";
                }
                options += "ShowAudioTiles" + "\":";
                options += ComboBoxShowAudioTiles.SelectionBoxItem.ToString();
                isSelected = true;
            }

            if ((bool)CheckboxPixelDensity.IsChecked)
            {
                if (isSelected)
                {
                    options += ",\"";
                }
                options += "SetPixelDensity" + "\":";
                options += TextBoxPixelDensity.Text;
                isSelected = true;
            }

            if ((bool)CheckboxTouchInputDevice.IsChecked)
            {
                if (isSelected)
                {
                    options += ",\"";
                }
                options += "SetTouchAsInputDevice" + "\":";
                options += ComboBoxTouchInputDevice.SelectionBoxItem.ToString();
                isSelected = true;
            }


            if ((bool)CheckboxExpandedCameraControl.IsChecked)
            {
                if (isSelected)
                {
                    options += ",\"";
                }
                options += "EnableExpandedCameraControl" + "\":";
                options += ComboBoxExpandedCameraControl.SelectionBoxItem.ToString();
                isSelected = true;
            }

            if ((bool)CheckboxSetPipPosition.IsChecked)
            {
                if (isSelected)
                {
                    options += ",\"";
                }
                options += "SetPipPosition" + "\":{";
                options += "\"x" + "\":\"";
                options += ComboBoxSetPipPositionX.SelectionBoxItem.ToString();
                options += "\",\"";
                options += "y" + "\":\"";
                options += ComboBoxSetPipPositionY.SelectionBoxItem.ToString();
                options += "\",\"";
                options += "lockPip" + "\":";
                options += ComboBoxSetPipPositionLockPin.SelectionBoxItem.ToString();
                options += "}";
                isSelected = true;
            }

            if ((bool)CheckboxSetBorderStyle.IsChecked)
            {
                if (isSelected)
                {
                    options += ",\"";
                }
                options += "BorderStyle" + "\":\"";
                options += ComboBoxSetBorderStyle.SelectionBoxItem.ToString();
                options += "\"";
            }

            if ((bool)CheckboxPreviewMirroring.IsChecked)
            {
                if (isSelected)
                {
                    options += ",\"";
                }
                options += "EnablePreviewMirroring" + "\":";
                options += ComboBoxPreviewMirroring.SelectionBoxItem.ToString();
                isSelected = true;
            }
            options += "}";

            if (isSelected)
            {
                if (!setRendererOptions(options))
                {
                    MessageBox.Show("Failed to set renderer options.", "Renderer Options");
                    return;
                }
                this.Visibility = Visibility.Hidden;
                return;
            }
            MessageBox.Show("Please select any one option to set.", "Renderer Options");
        }

        private void UpdateRendererOptions()
        {
            var list = Regex.Replace(getRendererOptions(), @"[^\w\:\,.]", "").Split(',').ToList();
            foreach (var item in list)
            {
                var obj = item.Split(':');
                if (obj[0] == "EnableVerticalVideoCentering")
                {
                    ComboBoxVerticalVideoCentering.SelectedIndex = obj[1] == "true" ? 1 : 0;
                }
                else if (obj[0] == "EnableFECCIconCustomLayout")
                {
                    ComboBoxFECCIconCustomLayout.SelectedIndex = obj[1] == "true" ? 1 : 0;
                }
                else if (obj[0] == "ViewingDistance")
                {
                    TextBoxViewingDistance.Text = obj[1];
                }
                else if (obj[0] == "ShowAudioTiles")
                {
                    ComboBoxShowAudioTiles.SelectedIndex = obj[1] == "true" ? 1 : 0;
                }
                else if (obj[0] == "SetTouchAsInputDevice")
                {
                    ComboBoxTouchInputDevice.SelectedIndex = obj[1] == "true" ? 1 : 0;
                }
                else if (obj[0] == "SetPixelDensity")
                {
                    TextBoxPixelDensity.Text = obj[1];
                }
                else if (obj[0] == "EnableExpandedCameraControl")
                {
                    ComboBoxExpandedCameraControl.SelectedIndex = obj[1] == "true" ? 1 : 0;
                }
                else if (obj[0] == "EnablePreviewMirroring")
                {
                    ComboBoxPreviewMirroring.SelectedIndex = obj[1] == "true" ? 1 : 0;
                }
            }

            /* TO DO: Parse the JSON formatted string for setPipPosition and update options.*/

            CheckboxFECCIconCustomLayout.IsChecked = false;
            CheckboxVerticalVideoCentering.IsChecked = false;
            CheckboxViewingDistance.IsChecked = false;
            CheckboxShowAudioTiles.IsChecked = false;
            CheckboxPixelDensity.IsChecked = false;
            CheckboxTouchInputDevice.IsChecked = false;
            CheckboxExpandedCameraControl.IsChecked = false;
            CheckboxPreviewMirroring.IsChecked = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        public new void Show()
        {
            UpdateRendererOptions();
            base.ShowDialog();
        }
    }
}
