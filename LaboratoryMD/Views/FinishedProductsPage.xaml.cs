using LaboratoryMD.Services;
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

namespace LaboratoryMD.Views
{
    /// <summary>
    /// Логика взаимодействия для FinishedProductsPage.xaml
    /// </summary>
    public partial class FinishedProductsPage : Page
    {
        private List<dynamic> _finishedProducts;

        public FinishedProductsPage()
        {
            InitializeComponent();
            LoadFinishedProducts();
        }

        private async void LoadFinishedProducts()
        {
            try
            {
                // Получаем партии готовой продукции, требующие контроля
                var products = await ApiClient.GetAsync<List<dynamic>>("qualitytests/pending?type=finished_product");
                _finishedProducts = products;
                dgFinishedProducts.ItemsSource = _finishedProducts;
                txtTotalCount.Text = _finishedProducts?.Count.ToString() ?? "0";

                ApplyFilter();
            }
            catch (Exception ex)
            {
                txtInfo.Text = $"Ошибка загрузки: {ex.Message}";
                txtInfo.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void ApplyFilter()
        {
            if (_finishedProducts == null) return;

            var status = (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
            var filtered = _finishedProducts.AsEnumerable();

            switch (status)
            {
                case "Ожидает контроля":
                    filtered = filtered.Where(p => p.quality_status?.ToString() == "pending");
                    break;
                case "В работе":
                    filtered = filtered.Where(p => p.quality_status?.ToString() == "in_progress");
                    break;
                case "Одобрено":
                    filtered = filtered.Where(p => p.quality_status?.ToString() == "approved");
                    break;
                case "Забраковано":
                    filtered = filtered.Where(p => p.quality_status?.ToString() == "blocked");
                    break;
            }

            dgFinishedProducts.ItemsSource = filtered.ToList();
            txtTotalCount.Text = filtered.Count().ToString();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadFinishedProducts();
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button).Tag;
            var testWindow = new TestWindow(product, "finished_product");
            testWindow.Owner = Window.GetWindow(this);
            testWindow.ShowDialog();
            LoadFinishedProducts();
        }

        private void DgFinishedProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var product = dgFinishedProducts.SelectedItem;
            if (product != null)
            {
                var testWindow = new TestWindow(product, "finished_product");
                testWindow.Owner = Window.GetWindow(this);
                testWindow.ShowDialog();
                LoadFinishedProducts();
            }
        }
    }
}
