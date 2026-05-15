using OperatorMD.Models;
using OperatorMD.Services;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OperatorMD.Views
{
    /// <summary>
    /// Логика взаимодействия для ActiveBatchesPage.xaml
    /// </summary>
    public partial class ActiveBatchesPage : Page
    {
        private List<ActiveBatch> _batches;
        private System.Windows.Threading.DispatcherTimer _refreshTimer;

        public ActiveBatchesPage()
        {
            InitializeComponent();
            LoadActiveBatches();

            // Автообновление каждые 10 секунд
            _refreshTimer = new System.Windows.Threading.DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(10);
            _refreshTimer.Tick += (s, e) => LoadActiveBatches();
            _refreshTimer.Start();
        }

        private async void LoadActiveBatches()
        {
            try
            {
                _batches = await ApiClient.GetAsync<List<ActiveBatch>>("batches/active");
                itemsBatches.ItemsSource = _batches;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки активных партий: {ex.Message}");
            }
        }

        private void BatchCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var batch = border?.Tag as ActiveBatch;

            if (batch != null)
            {
                var programPage = new BatchProgramPage(batch.id);
                NavigationService?.Navigate(programPage);
            }
        }
    }
}
