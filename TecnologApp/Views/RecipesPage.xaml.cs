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
using TecnologApp.Helpers;
namespace TecnologApp.Views
{
    /// <summary>
    /// Логика взаимодействия для RecipesPage.xaml
    /// </summary>
    public partial class RecipesPage : Page
    {
        private List<Recipe> _allRecipes;
        private int? _productIdFilter = null;

        public RecipesPage(int? productId = null)
        {
            InitializeComponent();
            _productIdFilter = productId;
            LoadRecipes();
        }

        private async void LoadRecipes()
        {
            try
            {
                var status = (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
                string endpoint = status == "Все" ? "recipes" : $"recipes?status={status}";

                _allRecipes = await ApiClient.GetAsync<List<Recipe>>(endpoint);

                if (_productIdFilter.HasValue)
                {
                    _allRecipes = _allRecipes.Where(r => r.product_id == _productIdFilter.Value).ToList();
                }

                dgRecipes.ItemsSource = _allRecipes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки рецептур: {ex.Message}");
            }
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadRecipes();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRecipes();
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RecipeDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadRecipes();
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var recipe = (sender as Button).Tag as Recipe;
            var dialog = new RecipeDialog(recipe);
            if (dialog.ShowDialog() == true)
            {
                LoadRecipes();
            }
        }

        private void BtnComponents_Click(object sender, RoutedEventArgs e)
        {
            var recipe = (sender as Button).Tag as Recipe;
            var componentsWindow = new RecipeComponentsWindow(recipe.id);
            componentsWindow.ShowDialog();
            LoadRecipes(); // Обновляем для проверки суммы
        }

        private async void BtnActivate_Click(object sender, RoutedEventArgs e)
        {
            var recipe = (sender as Button).Tag as Recipe;

            if (recipe.status == "active")
            {
                MessageBox.Show("Рецептура уже активна", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var request = new { ApprovedBy = AppSettings.CurrentUserId };
                var result = await ApiClient.PostAsync<dynamic>($"recipes/{recipe.id}/activate", request);

                MessageBox.Show($"Рецептура активирована! {result.message}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRecipes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка активации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var recipe = (sender as Button).Tag as Recipe;

            if (recipe.status == "active")
            {
                MessageBox.Show("Нельзя удалить активную рецептуру", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить рецептуру {recipe.product_name} версии {recipe.version}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await ApiClient.DeleteAsync($"recipes/{recipe.id}");
                    LoadRecipes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }
    }
}
