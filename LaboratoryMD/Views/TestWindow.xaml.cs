using LaboratoryMD.Models;
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
using System.Windows.Shapes;

namespace LaboratoryMD.Views
{
    /// <summary>
    /// Логика взаимодействия для TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        private dynamic _target;
        private string _sampleType;
        private int _testId;
        private List<TestResult> _results;

        public TestWindow(dynamic target, string sampleType)
        {
            InitializeComponent();
            _target = target;
            _sampleType = sampleType;

            LoadTest();
        }

        private async void LoadTest()
        {
            try
            {
                // Создаем испытание
                var request = new CreateTestRequest
                {
                    BatchId = _sampleType == "finished_product" ? (int?)_target.id : null,
                    MaterialId = _sampleType == "raw_material" ? (int?)_target.id : null,
                    SampleType = _sampleType
                };

                var result = await ApiClient.PostAsync<dynamic>("qualitytests/create", request);
                _testId = result.id;

                // Устанавливаем заголовок
                if (_sampleType == "finished_product")
                {
                    txtTitle.Text = $"Испытание готовой продукции - партия {_target.batch_number}";
                    txtObjectInfo.Text = $"Продукт: {_target.product_name}";
                    txtDetails.Text = $"Номер партии: {_target.batch_number} | Количество: {_target.quantity} кг";

                    // Стандартные параметры для готовой продукции
                    _results = new List<TestResult>
                    {
                        new TestResult { parameter_name = "Концентрация", standard_value = "97", unit = "%", result = "pending", measured_value = "" },
                        new TestResult { parameter_name = "pH", standard_value = "6.5-7.0", unit = "", result = "pending", measured_value = "" },
                        new TestResult { parameter_name = "Плотность", standard_value = "1.40-1.42", unit = "g/cm³", result = "pending", measured_value = "" }
                    };
                }
                else
                {
                    txtTitle.Text = $"Испытание сырья - {_target.name}";
                    txtObjectInfo.Text = $"Материал: {_target.name} ({_target.code})";
                    txtDetails.Text = $"Поставщик: {_target.supplier} | Тип: {_target.material_type}";

                    // Стандартные параметры для сырья
                    _results = new List<TestResult>
                    {
                        new TestResult { parameter_name = "Чистота", standard_value = "98", unit = "%", result = "pending", measured_value = "" },
                        new TestResult { parameter_name = "Влажность", standard_value = "0.5", unit = "%", result = "pending", measured_value = "" },
                        new TestResult { parameter_name = "Примеси", standard_value = "0.1", unit = "%", result = "pending", measured_value = "" }
                    };
                }

                dgParameters.ItemsSource = _results;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания испытания: {ex.Message}");
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var result = textBox?.Tag as TestResult;

            if (result != null && !string.IsNullOrEmpty(result.measured_value))
            {
                CheckParameter(result);
            }

            CheckAllResults();
        }

        private void CheckParameter(TestResult result)
        {
            if (!decimal.TryParse(result.measured_value, out decimal measured))
            {
                result.result = "pending";
                return;
            }

            if (result.standard_value.Contains("-"))
            {
                // Диапазон
                var parts = result.standard_value.Split('-');
                if (decimal.TryParse(parts[0], out decimal min) && decimal.TryParse(parts[1], out decimal max))
                {
                    result.result = (measured >= min && measured <= max) ? "pass" : "fail";
                }
            }
            else
            {
                // Одиночное значение (допуск ±5%)
                if (decimal.TryParse(result.standard_value, out decimal standard))
                {
                    decimal tolerance = standard * 0.05m;
                    result.result = (measured >= standard - tolerance && measured <= standard + tolerance) ? "pass" : "fail";
                }
            }

            // Обновляем отображение
            dgParameters.Items.Refresh();
        }

        private void CheckAllResults()
        {
            bool allCompleted = _results.All(r => !string.IsNullOrEmpty(r.measured_value));
            if (allCompleted)
            {
                cmbDecision.IsEnabled = true;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, что все результаты введены
                if (_results.Any(r => string.IsNullOrEmpty(r.measured_value)))
                {
                    MessageBox.Show("Заполните все параметры анализа!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Собираем результаты
                var resultsList = new List<TestResultItem>();
                foreach (var result in _results)
                {
                    resultsList.Add(new TestResultItem
                    {
                        ParameterName = result.parameter_name,
                        MeasuredValue = result.measured_value,
                        StandardValue = result.standard_value,
                        Unit = result.unit,
                        Result = result.result
                    });
                }

                // Сохраняем результаты
                var input = new TestResultInput
                {
                    TestId = _testId,
                    Comment = txtComment.Text,
                    Results = resultsList
                };

                await ApiClient.PostAsync<dynamic>("testresults/batch", input);

                // Принимаем решение
                string decision = (cmbDecision.SelectedItem as ComboBoxItem)?.Content.ToString() == "Одобрить" ? "approved" : "blocked";

                if (decision == "blocked" && string.IsNullOrWhiteSpace(txtComment.Text))
                {
                    MessageBox.Show("При блокировке партии необходимо указать причину!",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var decisionRequest = new DecisionRequest
                {
                    Decision = decision,
                    Comment = txtComment.Text
                };

                await ApiClient.PostAsync<dynamic>($"qualitytests/{_testId}/decision", decisionRequest);

                MessageBox.Show($"Испытание завершено. Решение: {(decision == "approved" ? "Одобрено" : "Забраковано")}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }
    }
}
