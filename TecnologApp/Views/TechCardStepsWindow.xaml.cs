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
    /// Логика взаимодействия для TechCardStepsWindow.xaml
    /// </summary>
    public partial class TechCardStepsWindow : Window
    {
        private int _techCardId;
        private List<TechCardStep> _steps;

        public TechCardStepsWindow(int techCardId)
        {
            InitializeComponent();
            _techCardId = techCardId;
            txtTitle.Text = $"Шаги техкарты #{techCardId}";
            LoadSteps();
        }

        private async void LoadSteps()
        {
            try
            {
                _steps = await ApiClient.GetAsync<List<TechCardStep>>($"techcardsteps/bycard/{_techCardId}");
                dgSteps.ItemsSource = _steps.OrderBy(s => s.step_order).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки шагов: {ex.Message}");
            }
        }

        private async void BtnAddStep_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStepName.Text))
            {
                MessageBox.Show("Введите название шага");
                return;
            }

            try
            {
                var newStep = new
                {
                    card_id = _techCardId,
                    step_name = txtStepName.Text,
                    planned_temp_c = string.IsNullOrEmpty(txtTemp.Text) ? (decimal?)null : decimal.Parse(txtTemp.Text),
                    planned_duration_min = string.IsNullOrEmpty(txtDuration.Text) ? (int?)null : int.Parse(txtDuration.Text),
                    planned_pressure_bar = string.IsNullOrEmpty(txtPressure.Text) ? (decimal?)null : decimal.Parse(txtPressure.Text),
                    temp_tolerance_max = string.IsNullOrEmpty(txtTempTolerance.Text) ? (decimal?)null : decimal.Parse(txtTempTolerance.Text),
                    is_mandatory = chkMandatory.IsChecked ?? true
                };

                await ApiClient.PostAsync<dynamic>("techcardsteps", newStep);
                LoadSteps();

                // Очистка формы
                txtStepName.Clear();
                txtTemp.Clear();
                txtDuration.Clear();
                txtPressure.Clear();
                txtTempTolerance.Clear();
                chkMandatory.IsChecked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления шага: {ex.Message}");
            }
        }

        private async void BtnDeleteStep_Click(object sender, RoutedEventArgs e)
        {
            var step = (sender as Button).Tag as TechCardStep;

            if (MessageBox.Show($"Удалить шаг '{step.step_name}'?",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await ApiClient.DeleteAsync($"techcardsteps/{step.id}");
                    LoadSteps();
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
