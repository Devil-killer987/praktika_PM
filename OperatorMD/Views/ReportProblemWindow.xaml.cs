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
using System.Windows.Shapes;

namespace OperatorMD.Views
{
    /// <summary>
    /// Логика взаимодействия для ReportProblemWindow.xaml
    /// </summary>
    public partial class ReportProblemWindow : Window
    {
        private List<ActiveBatch> _batches;

        public ReportProblemWindow()
        {
            InitializeComponent();
            LoadBatches();
        }

        private async void LoadBatches()
        {
            try
            {
                _batches = await ApiClient.GetAsync<List<ActiveBatch>>("batches/active");
                cmbBatch.ItemsSource = _batches;
                cmbBatch.DisplayMemberPath = "batch_number";
                cmbBatch.SelectedValuePath = "id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партий: {ex.Message}");
            }
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBatch.SelectedValue == null)
            {
                MessageBox.Show("Выберите партию", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Введите описание проблемы", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var request = new
                {
                    batch_id = (int)cmbBatch.SelectedValue,
                    deviation_type = (cmbProblemType.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    description = txtDescription.Text,
                    severity = "critical"
                };

                await ApiClient.PostAsync<dynamic>("deviations/report", request);

                MessageBox.Show("Сообщение о проблеме отправлено технологу!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
