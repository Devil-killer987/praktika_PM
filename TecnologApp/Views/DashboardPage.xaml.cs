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
using System.Windows.Threading;
using TecnologApp.Models;
using TecnologApp.Services;

namespace TecnologApp.Views
{
    /// <summary>
    /// Логика взаимодействия для DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private DispatcherTimer _refreshTimer;

        public DashboardPage()
        {
            InitializeComponent();
            LoadDashboardData();

            // Автообновление каждые 30 секунд
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(30);
            _refreshTimer.Tick += (s, e) => LoadDashboardData();
            _refreshTimer.Start();
        }

        private async void LoadDashboardData()
        {
            try
            {
                var stats = await ApiClient.GetAsync<dynamic>("dashboard/stats");

                txtActiveProducts.Text = stats.active_products?.ToString() ?? "0";
                txtActiveRecipes.Text = stats.active_recipes?.ToString() ?? "0";
                txtActiveTechCards.Text = stats.active_tech_cards?.ToString() ?? "0";
                txtOrdersInProgress.Text = stats.orders_in_progress?.ToString() ?? "0";
                txtBatchesInProduction.Text = stats.batches_in_production?.ToString() ?? "0";
                txtBatchesWithDeviations.Text = stats.batches_with_deviations?.ToString() ?? "0";
                txtPendingQualityTests.Text = stats.pending_quality_tests?.ToString() ?? "0";
                txtBlockedBatches.Text = stats.blocked_batches?.ToString() ?? "0";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки дашборда: {ex.Message}");
            }
        }
    }
}
