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
using TecnologApp.Models;
using TecnologApp.Services;

namespace TecnologApp.Views
{
    /// <summary>
    /// Логика взаимодействия для BatchesPage.xaml
    /// </summary>
    public partial class BatchesPage : Page
    {
        private List<Batch> _allBatches;

        public BatchesPage()
        {
            InitializeComponent();
            LoadBatches();
        }

        private async void LoadBatches()
        {
            try
            {
                var status = (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
                string endpoint = status == "Все" ? "batches" : $"batches?status={status}";

                _allBatches = await ApiClient.GetAsync<List<Batch>>(endpoint);

                // Фильтр по отклонениям
                if (chkOnlyWithDeviations.IsChecked == true)
                {
                    _allBatches = _allBatches.Where(b => b.deviation_count > 0).ToList();
                }

                dgBatches.ItemsSource = _allBatches;
                txtInfo.Text = $"Всего партий: {_allBatches.Count}";
            }
            catch (Exception ex)
            {
                txtInfo.Text = $"Ошибка загрузки: {ex.Message}";
                txtInfo.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBatches();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadBatches();
        }

        private void ChkFilter_Changed(object sender, RoutedEventArgs e)
        {
            LoadBatches();
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            var batch = (sender as Button).Tag as Batch;
            var detailsWindow = new BatchDetailsWindow(batch.id);
            detailsWindow.ShowDialog();
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var batch = (sender as Button).Tag as Batch;

            if (batch.status != "planned")
            {
                MessageBox.Show("Можно запустить только партию в статусе 'planned'",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Запустить партию {batch.batch_number}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await ApiClient.PostAsync<dynamic>($"batches/{batch.id}/start", null);
                    MessageBox.Show($"Партия {batch.batch_number} запущена!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadBatches();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка запуска: {ex.Message}");
                }
            }
        }

        private void DgBatches_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var batch = dgBatches.SelectedItem as Batch;
            if (batch != null)
            {
                var detailsWindow = new BatchDetailsWindow(batch.id);
                detailsWindow.ShowDialog();
            }
        }
    }
}
