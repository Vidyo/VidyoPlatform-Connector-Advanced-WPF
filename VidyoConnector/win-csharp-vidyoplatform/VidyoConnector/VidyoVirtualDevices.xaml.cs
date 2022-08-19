using System;
using System.Windows;
using System.ComponentModel;
using VidyoConnector.ViewModel;

namespace VidyoConnector
{
    /// <summary>
    /// Interaction logic for VidyoVirtualDevices.xaml
    /// </summary>
    public partial class VidyoVirtualDevices : Window
    {

        private VidyoVirtualDeviceViewModel virtualDeviceViewModel;

        public VidyoVirtualDevices(object DataContext)
        {
            InitializeComponent();
            this.DataContext = DataContext;
            string filePath = Environment.CurrentDirectory + "\\resources\\audio\\simpleCount32000Hz.wav";
            virtualDeviceViewModel = new VidyoVirtualDeviceViewModel(DataContext, filePath, FeedingStateChanged);
        }

        private void FeedingStateChanged(string message)
        {
            this.Dispatcher.InvokeAsync(new Action(() =>
            {
                FeedData.Content = message;
            }));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void CreateDevice_Click(object sender, RoutedEventArgs e)
        {
            string id = System.DateTime.Now.Ticks.ToString().Substring(0, 10);
            string name = RadioButtonVirtualAudioDevice.IsChecked.Value ? ("VirtualMicrophone - " + id) : ("VirtualCamera - " + id);

            if (RadioButtonVirtualAudioDevice.IsChecked.Value)
                virtualDeviceViewModel.CreateVirtualAudioDevice(name, id);
            else
                MessageBox.Show("Not able to create Virtual video source now.");
        }

        private void RadioButtonVirtualVideoDevice_Checked(object sender, RoutedEventArgs e)
        {
            GridVirtualVideoDeviceType.Visibility = Visibility.Visible;
            RadioButtonVirtualVideoDevice.Visibility = Visibility.Hidden;
        }

        private void RadioButtonVirtualAudioDevice_Checked(object sender, RoutedEventArgs e)
        {
            GridVirtualVideoDeviceType.Visibility = Visibility.Hidden;
            RadioButtonVirtualVideoDevice.Visibility = Visibility.Visible;
        }

        private void FeedData_Click(object sender, RoutedEventArgs e)
        {
            if (!virtualDeviceViewModel.IsAudioFeeding())
                virtualDeviceViewModel.StartAudioFeeding();
            else
                virtualDeviceViewModel.StopAudioFeeding();
        }
    }
}
