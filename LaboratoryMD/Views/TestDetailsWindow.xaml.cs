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
    /// Логика взаимодействия для TestDetailsWindow.xaml
    /// </summary>
    public partial class TestDetailsWindow : Window
    {
        private dynamic _test;

        public TestDetailsWindow(dynamic test)
        {
            InitializeComponent();
            _test = test;
            LoadTestDetails();
        }

        private async void LoadTestDetails()
        {
            try
            {
                int testId = _test.id;
                var testDetails = await ApiClient.GetAsync<dynamic>($"qualitytests/{testId}");

                // Заполняем информацию
                txtSampleType.Text = testDetails.sample_type == "finished_product" ? "Готовая продукция" : "Сырье";
                txtObjectName.Text = testDetails.batch_number?.ToString() ?? testDetails.material_name?.ToString() ?? "-";
                txtStatus.Text = testDetails.status == "completed" ? "Завершено" : "В работе";
                txtDecision.Text = testDetails.decision == "approved" ? "Одобрено" :
                                  (testDetails.decision == "blocked" ? "Забраковано" : "Не принято");
                txtAnalysisDate.Text = testDetails.analysis_date?.ToString() ?? "-";
                txtComment.Text = testDetails.analyst_comment?.ToString() ?? "Нет комментария";

                // Загружаем результаты
                var results = await ApiClient.GetAsync<List<TestResult>>($"testresults/bytest/{testId}");
                dgResults.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей: {ex.Message}");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
