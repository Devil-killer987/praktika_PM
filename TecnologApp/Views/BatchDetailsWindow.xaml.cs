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
using System.Windows.Shapes;
using TecnologApp.Models;
using TecnologApp.Services;

namespace TecnologApp.Views
{
    /// <summary>
    /// Логика взаимодействия для BatchDetailsWindow.xaml
    /// </summary>
    public partial class BatchDetailsWindow : Window
    {
        private int _batchId;

        public BatchDetailsWindow(int batchId)
        {
            InitializeComponent();
            _batchId = batchId;
            LoadBatchDetails();
        }

        private async void LoadBatchDetails()
        {
            try
            {
                var batch = await ApiClient.GetAsync<Batch>($"batches/{_batchId}");

                txtBatchNumber.Text = batch.batch_number;
                txtProduct.Text = batch.product_name;
                txtStatus.Text = batch.status;
                txtDeviations.Text = batch.deviation_count.ToString();
                txtStartTime.Text = batch.start_time?.ToString() ?? "-";
                txtEndTime.Text = batch.end_time?.ToString() ?? "-";
                txtPlannedQty.Text = "-";
                txtActualQty.Text = batch.actual_quantity_kg?.ToString() ?? "-";
                txtTitle.Text = $"Детали партии №{batch.batch_number}";

                var steps = await ApiClient.GetAsync<List<BatchStep>>($"batches/{_batchId}/steps");
                dgSteps.ItemsSource = steps;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
