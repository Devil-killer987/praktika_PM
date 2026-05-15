using OperatorMD.Models;
using OperatorMD.Services;
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

namespace OperatorMD.Views
{
    /// <summary>
    /// Логика взаимодействия для BatchProgramPage.xaml
    /// </summary>
    public partial class BatchProgramPage : Page
    {
        private int _batchId;
        private BatchProgram _program;
        private BatchStep _currentStep;
        private System.Windows.Threading.DispatcherTimer _refreshTimer;

        public BatchProgramPage(int batchId = 0)
        {
            InitializeComponent();
            _batchId = batchId;
            LoadBatchProgram();

            // Автообновление каждые 5 секунд
            _refreshTimer = new System.Windows.Threading.DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(5);
            _refreshTimer.Tick += (s, e) => LoadBatchProgram();
            _refreshTimer.Start();
        }

        private async void LoadBatchProgram()
        {
            try
            {
                if (_batchId == 0)
                {
                    var activeBatches = await ApiClient.GetAsync<List<ActiveBatch>>("batches/active");
                    if (activeBatches != null && activeBatches.Count > 0)
                    {
                        _batchId = activeBatches[0].id;
                    }
                    else
                    {
                        txtBatchInfo.Text = "Нет активных партий";
                        return;
                    }
                }

                _program = await ApiClient.GetAsync<BatchProgram>($"batches/{_batchId}/program");

                txtBatchInfo.Text = $"Партия: {_program.batch_number}";
                txtProductInfo.Text = $"Продукт: {_program.product_name}";
                txtStatusInfo.Text = $"Статус: {_program.status}";

                itemsSteps.ItemsSource = _program.steps;

                // Находим текущий активный шаг
                _currentStep = _program.steps.FirstOrDefault(s => s.status == "in_progress");
                if (_currentStep == null)
                {
                    _currentStep = _program.steps.FirstOrDefault(s => s.status == "pending");
                }

                if (_currentStep != null)
                {
                    DisplayStepDetails(_currentStep);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки программы: {ex.Message}");
            }
        }

        private void DisplayStepDetails(BatchStep step)
        {
            txtStepTitle.Text = $"Шаг {step.step_order}: {step.step_name}";
            txtStepInstruction.Text = step.instruction ?? "Нет инструкции";

            txtPlannedTemp.Text = step.planned_temp_c?.ToString() ?? "—";
            txtPlannedDuration.Text = step.planned_duration_min?.ToString() ?? "—";
            txtPlannedPressure.Text = step.planned_pressure_bar?.ToString() ?? "—";

            if (step.status == "in_progress")
            {
                borderActual.Visibility = Visibility.Visible;
                btnStartStep.Visibility = Visibility.Collapsed;
                btnCompleteStep.Visibility = Visibility.Visible;

                txtActualTemp.Text = step.actual_temp_c?.ToString() ?? "";
                txtActualDuration.Text = step.actual_duration_min?.ToString() ?? "";
                txtActualPressure.Text = step.actual_pressure_bar?.ToString() ?? "";
            }
            else if (step.status == "pending")
            {
                borderActual.Visibility = Visibility.Collapsed;
                btnStartStep.Visibility = Visibility.Visible;
                btnCompleteStep.Visibility = Visibility.Collapsed;
            }
            else if (step.status == "completed")
            {
                borderActual.Visibility = Visibility.Collapsed;
                btnStartStep.Visibility = Visibility.Collapsed;
                btnCompleteStep.Visibility = Visibility.Collapsed;
            }
        }

        private void StepItem_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var step = border?.Tag as BatchStep;
            if (step != null && step.status != "completed")
            {
                _currentStep = step;
                DisplayStepDetails(step);
            }
        }

        private async void BtnStartStep_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep == null) return;

            try
            {
                var result = await ApiClient.PostAsync<StartStepResponse>($"batchsteps/{_currentStep.id}/start", null);
                MessageBox.Show($"Шаг {_currentStep.step_name} начат!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadBatchProgram();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска шага: {ex.Message}");
            }
        }

        private async void BtnCompleteStep_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep == null) return;

            // Проверка параметров
            decimal? temp = null;
            int? duration = null;
            decimal? pressure = null;

            if (!string.IsNullOrWhiteSpace(txtActualTemp.Text))
            {
                if (!decimal.TryParse(txtActualTemp.Text, out var t))
                {
                    MessageBox.Show("Введите корректную температуру");
                    return;
                }
                temp = t;
            }

            if (!string.IsNullOrWhiteSpace(txtActualDuration.Text))
            {
                if (!int.TryParse(txtActualDuration.Text, out var d))
                {
                    MessageBox.Show("Введите корректную длительность");
                    return;
                }
                duration = d;
            }

            if (!string.IsNullOrWhiteSpace(txtActualPressure.Text))
            {
                if (!decimal.TryParse(txtActualPressure.Text, out var p))
                {
                    MessageBox.Show("Введите корректное давление");
                    return;
                }
                pressure = p;
            }

            // Проверка отклонений
            bool hasDeviation = false;
            string severity = "warning";

            if (temp.HasValue && _currentStep.planned_temp_c.HasValue)
            {
                var tolerance = _currentStep.temp_tolerance_max ?? 2;
                if (Math.Abs(temp.Value - _currentStep.planned_temp_c.Value) > tolerance)
                {
                    hasDeviation = true;
                }
            }

            if (pressure.HasValue && _currentStep.planned_pressure_bar.HasValue)
            {
                var tolerance = _currentStep.pressure_tolerance_max ?? 0.3m;
                if (Math.Abs(pressure.Value - _currentStep.planned_pressure_bar.Value) > tolerance)
                {
                    hasDeviation = true;
                }
            }

            if (hasDeviation && string.IsNullOrWhiteSpace(txtComment.Text))
            {
                MessageBox.Show("При отклонении необходимо указать комментарий!",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                borderComment.Visibility = Visibility.Visible;
                return;
            }

            if (hasDeviation && cmbSeverity.SelectedItem != null)
            {
                severity = (cmbSeverity.SelectedItem as ComboBoxItem)?.Content.ToString() == "Критическое"
                    ? "critical" : "warning";
            }

            try
            {
                var request = new CompleteStepRequest
                {
                    ActualTempC = temp,
                    ActualDurationMin = duration,
                    ActualPressureBar = pressure,
                    OperatorComment = txtComment.Text,
                    Severity = severity
                };

                var result = await ApiClient.PutAsync<CompleteStepResponse>($"batchsteps/{_currentStep.id}/complete", request);

                if (result.all_steps_completed)
                {
                    MessageBox.Show("Все шаги партии завершены! Партия направлена на контроль качества.",
                        "Поздравляем!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Шаг {_currentStep.step_name} завершен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadBatchProgram();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка завершения шага: {ex.Message}");
            }
        }
    }
}
