using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace VidyoConnector
{
    /// <summary>
    /// Interaction logic for VirtualBackgroundPreview.xaml
    /// </summary>
    public partial class VirtualBackgroundPreview : UserControl
    {
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();

        public VirtualBackgroundPreview()
        {
            InitializeComponent();
        }

        public VirtualBackgroundPreview(string imageName, string imagePath)
        {
            InitializeComponent();
            _imageName = imageName;
            _imagePath = imagePath;
            RadioButtonSelected.Content = "Loading...";

            _backgroundWorker.DoWork += new DoWorkEventHandler(LoadImageInBackground);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LoadImageFinished);
            _backgroundWorker.RunWorkerAsync();
        }

        private string _imageName = "";
        public string ImageName { get { return _imageName; } }
        private string _imagePath = "";
        public string ImagePath { get { return _imagePath; } }

        public bool IsSelected
        {
            get
            {
                bool? selectedValue = RadioButtonSelected.IsChecked;
                return  selectedValue.HasValue ? selectedValue.Value : false;
            }
        }

        private void LoadImageInBackground(object sender, DoWorkEventArgs e)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(_imagePath);
            image.DecodePixelWidth = 72;
            image.EndInit();
            image.Freeze();
            e.Result = image;
        }

        private void LoadImageFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            BitmapImage image = (BitmapImage)e.Result;
            ImagePreview.Source = image;
            RadioButtonSelected.Content = _imageName;
        }
    }
}
