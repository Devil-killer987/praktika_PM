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
using TecnologApp.Services;

namespace TecnologApp.Views
{
    /// <summary>
    /// Логика взаимодействия для ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();

            // Устанавливаем даты после инициализации компонентов
            dpFromDate.SelectedDate = DateTime.Now.AddMonths(-1);
            dpToDate.SelectedDate = DateTime.Now;

            // Загружаем отчет
            LoadReport();
        }

        private void CmbReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadReport();
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            LoadReport();
        }

        private async void LoadReport()
        {
            try
            {
                // Проверка, что элементы управления существуют
                if (btnGenerate == null || cmbReportType == null) return;

                btnGenerate.IsEnabled = false;
                btnGenerate.Content = "Загрузка...";

                string reportType = (cmbReportType.SelectedItem as ComboBoxItem)?.Content?.ToString();
                string fromDate = dpFromDate.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
                string toDate = dpToDate.SelectedDate?.ToString("yyyy-MM-dd") ?? "";

                string endpoint = "";

                switch (reportType)
                {
                    case "По партиям":
                        endpoint = $"reports/batches?fromDate={fromDate}&toDate={toDate}";
                        break;
                    case "По отклонениям":
                        endpoint = $"reports/deviations?fromDate={fromDate}&toDate={toDate}";
                        break;
                    case "По качеству":
                        endpoint = $"reports/quality?fromDate={fromDate}&toDate={toDate}";
                        break;
                    case "По использованию рецептур":
                        endpoint = "reports/recipes-usage";
                        break;
                    default:
                        endpoint = $"reports/batches?fromDate={fromDate}&toDate={toDate}";
                        break;
                }

                var result = await ApiClient.GetAsync<List<object>>(endpoint);

                if (result != null && result.Count > 0)
                {
                    dgReport.ItemsSource = result;
                    txtInfo.Text = $"Найдено записей: {result.Count}";
                    txtInfo.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    dgReport.ItemsSource = null;
                    txtInfo.Text = "Нет данных за выбранный период";
                    txtInfo.Foreground = System.Windows.Media.Brushes.Orange;
                }
            }
            catch (Exception ex)
            {
                if (txtInfo != null)
                {
                    txtInfo.Text = $"Ошибка загрузки: {ex.Message}";
                    txtInfo.Foreground = System.Windows.Media.Brushes.Red;
                }
                dgReport.ItemsSource = null;
            }
            finally
            {
                if (btnGenerate != null)
                {
                    btnGenerate.IsEnabled = true;
                    btnGenerate.Content = "Сформировать";
                }
            }
        }
    }
}
