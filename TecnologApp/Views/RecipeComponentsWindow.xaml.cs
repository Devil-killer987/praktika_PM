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
    /// Логика взаимодействия для RecipeComponentsWindow.xaml
    /// </summary>
    public partial class RecipeComponentsWindow : Window
    {
        private int _recipeId;
        private List<RecipeComponent> _components;
        private List<Material> _materials;

        public RecipeComponentsWindow(int recipeId)
        {
            InitializeComponent();
            _recipeId = recipeId;
            txtTitle.Text = $"Компоненты рецептуры #{recipeId}";
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // Загрузка компонентов
                _components = await ApiClient.GetAsync<List<RecipeComponent>>($"recipecomponents/byrecipe/{_recipeId}");
                dgComponents.ItemsSource = _components;
                UpdateTotalPercentage();

                // Загрузка материалов для выбора
                _materials = await ApiClient.GetAsync<List<Material>>("materials");
                cmbMaterial.ItemsSource = _materials;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void UpdateTotalPercentage()
        {
            if (_components == null) return;

            decimal total = _components.Sum(c => c.percentage);
            txtTotalPercentage.Text = $"{total}%";

            if (total == 100)
            {
                txtStatus.Text = "✓ Сумма корректна (100%)";
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                txtStatus.Text = $"⚠ Сумма должна быть 100% (сейчас {total}%)";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async void BtnAddComponent_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMaterial.SelectedValue == null)
            {
                MessageBox.Show("Выберите материал");
                return;
            }

            if (!decimal.TryParse(txtPercentage.Text, out decimal percentage))
            {
                MessageBox.Show("Введите корректный процент");
                return;
            }

            if (!int.TryParse(txtLoadOrder.Text, out int loadOrder))
            {
                loadOrder = (_components?.Count ?? 0) + 1;
            }

            try
            {
                var newComponent = new
                {
                    recipe_id = _recipeId,
                    material_id = (int)cmbMaterial.SelectedValue,
                    percentage = percentage,
                    load_order = loadOrder
                };

                await ApiClient.PostAsync<dynamic>("recipecomponents", newComponent);
                LoadData(); // Перезагрузка

                txtPercentage.Clear();
                txtLoadOrder.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}");
            }
        }

        private async void BtnDeleteComponent_Click(object sender, RoutedEventArgs e)
        {
            var component = (sender as Button).Tag as RecipeComponent;

            if (MessageBox.Show($"Удалить компонент {component.material_name}?",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await ApiClient.DeleteAsync($"recipecomponents/{component.id}");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
