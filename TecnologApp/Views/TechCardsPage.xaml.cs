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
using TecnologApp.Helpers;
using TecnologApp.Models;
using TecnologApp.Services;

namespace TecnologApp.Views
{
    /// <summary>
    /// Логика взаимодействия для TechCardsPage.xaml
    /// </summary>
    public partial class TechCardsPage : Page
    {
        private List<TechCard> _allTechCards;

        public TechCardsPage()
        {
            InitializeComponent();
            LoadTechCards();
        }

        private async void LoadTechCards()
        {
            try
            {
                var status = (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
                string endpoint = status == "Все" ? "techcards" : "techcards/active";

                _allTechCards = await ApiClient.GetAsync<List<TechCard>>(endpoint);
                dgTechCards.ItemsSource = _allTechCards;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки техкарт: {ex.Message}");
            }
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTechCards();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTechCards();
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TechCardDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadTechCards();
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var techCard = (sender as Button).Tag as TechCard;
            var dialog = new TechCardDialog(techCard);
            if (dialog.ShowDialog() == true)
            {
                LoadTechCards();
            }
        }

        private void BtnSteps_Click(object sender, RoutedEventArgs e)
        {
            var techCard = (sender as Button).Tag as TechCard;
            var stepsWindow = new TechCardStepsWindow(techCard.id);
            stepsWindow.ShowDialog();
            LoadTechCards();
        }

        private async void BtnActivate_Click(object sender, RoutedEventArgs e)
        {
            var techCard = (sender as Button).Tag as TechCard;

            if (techCard.status == "active")
            {
                MessageBox.Show("Техкарта уже активна", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var request = new { ApprovedBy = AppSettings.CurrentUserId };
                var result = await ApiClient.PostAsync<dynamic>($"techcards/{techCard.id}/activate", request);

                MessageBox.Show($"Техкарта активирована!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTechCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка активации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var techCard = (sender as Button).Tag as TechCard;

            if (techCard.status == "active")
            {
                MessageBox.Show("Нельзя удалить активную техкарту", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить техкарту для продукта {techCard.product_name} версии {techCard.version}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await ApiClient.DeleteAsync($"techcards/{techCard.id}");
                    LoadTechCards();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }
    }
}
