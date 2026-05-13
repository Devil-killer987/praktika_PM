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
    /// Логика взаимодействия для OrderDetailsWindow.xaml
    /// </summary>
    public partial class OrderDetailsWindow : Window
    {
        private int _orderId;

        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            LoadOrderDetails();
        }

        private async void LoadOrderDetails()
        {
            try
            {
                var order = await ApiClient.GetAsync<dynamic>($"orders/{_orderId}/details");

                txtOrderNumber.Text = order.order_number;
                txtProduct.Text = order.product_name;
                txtQuantity.Text = $"{order.planned_quantity_kg} кг";
                txtStatus.Text = order.status;
                txtTitle.Text = $"Детали заказа №{order.order_number}";

                // Загрузка партий
                var batches = await ApiClient.GetAsync<List<Batch>>($"batches?orderId={_orderId}");
                dgBatches.ItemsSource = batches;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void BtnViewBatch_Click(object sender, RoutedEventArgs e)
        {
            var batch = (sender as Button).Tag as Batch;
            var batchDetailsWindow = new BatchDetailsWindow(batch.id);
            batchDetailsWindow.ShowDialog();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
