using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using VidyoClient;
using static VidyoClient.Connector;

namespace VidyoConnector
{
    /// <summary>
    /// Interaction logic for VidyoProductInfo.xaml
    /// </summary>
    public partial class VidyoProductInfo : Window
    {
        private string applicationName;
        private string applicationVersion;

        public Func<List<ConnectorProductInformation>, bool> setProductInfo;

        public VidyoProductInfo(string version)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            applicationName = "VidyoConnector";
            applicationVersion = version;
        }

        private void ButtonSetProductInfo_Click(object sender, RoutedEventArgs e)
        {
            List<ConnectorProductInformation> infoList;
            ConnectorProductInformation info;

            if (string.IsNullOrEmpty(TextBoxApplicationName.Text))
            {
                MessageBoxResult msg = MessageBox.Show("Application Name is mandatory.", "Product Information");
                return;
            }

            infoList = new List<ConnectorProductInformation>();
            info = ConnectorProductInformationFactory.Create();
            info.propertyName = Connector.ConnectorProperty.ConnectorpropertyApplicationName;
            applicationName = info.value = TextBoxApplicationName.Text;
            infoList.Add(info);

            info = ConnectorProductInformationFactory.Create();
            info.propertyName = Connector.ConnectorProperty.ConnectorpropertyApplicationVersion;
            applicationVersion = info.value = TextBoxApplicationVersion.Text;
            infoList.Add(info);

            if (!setProductInfo(infoList))
            {
                MessageBoxResult msg = MessageBox.Show("Failed to set Product Information.", "Product Information");
            }
            infoList.Clear();
            this.Close();
        }

        public new void Show()
        {
            TextBoxApplicationName.Text = applicationName;
            TextBoxApplicationVersion.Text = applicationVersion;
            base.ShowDialog();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
    }
}
