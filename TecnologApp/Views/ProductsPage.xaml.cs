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
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        private List<Product> _products;

        public ProductsPage()
        {
            InitializeComponent();
            LoadProducts();
        }

        private async void LoadProducts()
        {
            try
            {
                _products = await ApiClient.GetAsync<List<Product>>("products");
                dgProducts.ItemsSource = _products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}");
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button).Tag as Product;
            var dialog = new ProductDialog(product);
            if (dialog.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button).Tag as Product;

            if (MessageBox.Show($"Удалить продукт {product.name}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await ApiClient.DeleteAsync($"products/{product.id}");
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void BtnRecipes_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button).Tag as Product;
            // Переход к рецептурам продукта
            var recipesPage = new RecipesPage(product.id);
            NavigationService?.Navigate(recipesPage);
        }
    }
}
