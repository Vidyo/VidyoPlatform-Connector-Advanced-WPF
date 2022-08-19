using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VidyoAnalytics.ViewModel;

namespace VidyoConnector
{
    public partial class VidyoAnalytics : Window
    {
        VidyoAnalyticsViewModel viewModel;

        public VidyoAnalytics()
        {
            InitializeComponent();
            viewModel = new VidyoAnalyticsViewModel();
            DataContext = viewModel;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        public void Init()
        {
            ((VidyoAnalyticsViewModel)DataContext).Init();
        }

        private void ButtonStartGoogleAnalytics_Click(object sender, RoutedEventArgs e)
        {
            ((VidyoAnalyticsViewModel)DataContext).StartGoogleAnalyticsService();
        }

        private void ButtonStopGoogleAnalytics_Click(object sender, RoutedEventArgs e)
        {
            ((VidyoAnalyticsViewModel)DataContext).StopGoogleAnalyticsService();
        }

        private void ButtonStartInsights_Click(object sender, RoutedEventArgs e)
        {
            ((VidyoAnalyticsViewModel)DataContext).StartInsightsService();
        }

        private void ButtonStopInsights_Click(object sender, RoutedEventArgs e)
        {
            ((VidyoAnalyticsViewModel)DataContext).StopInsightsService();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
    }
}
