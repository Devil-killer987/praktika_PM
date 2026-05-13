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
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        private List<ProductionOrder> _allOrders;

        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private async void LoadOrders()
        {
            try
            {
                var status = (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
                string endpoint = status == "Все" ? "orders" : "orders/active";

                _allOrders = await ApiClient.GetAsync<List<ProductionOrder>>(endpoint);
                dgOrders.ItemsSource = _allOrders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOrders();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OrderDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button).Tag as ProductionOrder;

            if (order.status != "planned")
            {
                MessageBox.Show("Можно запустить только заказ в статусе 'planned'",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Запустить заказ {order.order_number}? Будет создана производственная партия.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var result = await ApiClient.PostAsync<dynamic>($"orders/{order.id}/start", null);
                    MessageBox.Show($"Заказ запущен! Создана партия {result.batch_number}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка запуска: {ex.Message}");
                }
            }
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button).Tag as ProductionOrder;
            var detailsWindow = new OrderDetailsWindow(order.id);
            detailsWindow.ShowDialog();
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button).Tag as ProductionOrder;

            if (order.status != "planned")
            {
                MessageBox.Show("Можно редактировать только заказ в статусе 'planned'",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new OrderDialog(order);
            if (dialog.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var order = (sender as Button).Tag as ProductionOrder;

            if (order.status != "planned")
            {
                MessageBox.Show("Можно удалить только заказ в статусе 'planned'",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить заказ {order.order_number}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await ApiClient.DeleteAsync($"orders/{order.id}");
                    LoadOrders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }
    }
}
