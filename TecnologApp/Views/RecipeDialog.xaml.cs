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
    /// Логика взаимодействия для RecipeDialog.xaml
    /// </summary>
    public partial class RecipeDialog : Window
    {
        private Recipe _recipe;

        public RecipeDialog(Recipe recipe = null)
        {
            InitializeComponent();
            _recipe = recipe;
            LoadProducts();

            if (recipe != null)
            {
                Title = "Редактирование рецептуры";
                cmbProduct.SelectedValue = recipe.product_id;
                cmbProduct.IsEnabled = false;
                // Убираем строку с description, так как в модели Recipe его нет
                // txtDescription.Text = recipe.description;
            }
        }

        private async void LoadProducts()
        {
            try
            {
                var products = await ApiClient.GetAsync<List<Product>>("products/active");
                cmbProduct.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProduct.SelectedValue == null)
            {
                MessageBox.Show("Выберите продукт", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var request = new
                {
                    product_id = (int)cmbProduct.SelectedValue
                    // description = txtDescription.Text - убираем, так как API может не принимать
                };

                var result = await ApiClient.PostAsync<dynamic>("recipes", request);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
