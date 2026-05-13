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
    /// Логика взаимодействия для OrderDialog.xaml
    /// </summary>
    public partial class OrderDialog : Window
    {
        private ProductionOrder _order;
        private List<Recipe> _recipes;

        public OrderDialog(ProductionOrder order = null)
        {
            InitializeComponent();
            _order = order;
            LoadRecipes();

            // Устанавливаем дату по умолчанию (сегодня + 1 день)
            dpStartDate.SelectedDate = DateTime.Now.AddDays(1);

            if (order != null)
            {
                Title = "Редактирование заказа";
                cmbRecipe.SelectedValue = order.recipe_id;
                txtQuantity.Text = order.planned_quantity_kg.ToString();
                dpStartDate.SelectedDate = order.planned_start_date;
            }
        }

        private async void LoadRecipes()
        {
            try
            {
                _recipes = await ApiClient.GetAsync<List<Recipe>>("recipes/active");
                cmbRecipe.ItemsSource = _recipes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки рецептур: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRecipe.SelectedValue == null)
            {
                MessageBox.Show("Выберите рецептуру");
                return;
            }

            if (!decimal.TryParse(txtQuantity.Text, out decimal quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество");
                return;
            }

            if (dpStartDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату запуска");
                return;
            }

            try
            {
                if (_order == null)
                {
                    // Создание нового заказа
                    var request = new
                    {
                        recipe_id = (int)cmbRecipe.SelectedValue,
                        planned_quantity_kg = quantity,
                        planned_start_date = dpStartDate.SelectedDate.Value.ToString("yyyy-MM-dd")
                    };
                    await ApiClient.PostAsync<dynamic>("orders", request);
                }
                else
                {
                    // Обновление заказа
                    _order.recipe_id = (int)cmbRecipe.SelectedValue;
                    _order.planned_quantity_kg = quantity;
                    _order.planned_start_date = dpStartDate.SelectedDate.Value;
                    await ApiClient.PutAsync<dynamic>($"orders/{_order.id}", _order);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
