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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LaboratoryMD.Views
{
    /// <summary>
    /// Логика взаимодействия для RawMaterialsPage.xaml
    /// </summary>
    public partial class RawMaterialsPage : Page
    {
        private List<dynamic> _rawMaterials;

        public RawMaterialsPage()
        {
            InitializeComponent();
            LoadRawMaterials();
        }

        private async void LoadRawMaterials()
        {
            try
            {
                // Получаем партии, требующие контроля сырья
                var materials = await ApiClient.GetAsync<List<dynamic>>("qualitytests/pending?type=raw_material");
                _rawMaterials = materials;
                dgRawMaterials.ItemsSource = _rawMaterials;
                txtTotalCount.Text = _rawMaterials?.Count.ToString() ?? "0";

                // Применяем фильтр
                ApplyFilter();
            }
            catch (Exception ex)
            {
                txtInfo.Text = $"Ошибка загрузки: {ex.Message}";
                txtInfo.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void ApplyFilter()
        {
            if (_rawMaterials == null) return;

            var status = (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
            var filtered = _rawMaterials.AsEnumerable();

            switch (status)
            {
                case "Ожидает контроля":
                    filtered = filtered.Where(m => m.test_status?.ToString() == "pending");
                    break;
                case "В работе":
                    filtered = filtered.Where(m => m.test_status?.ToString() == "in_progress");
                    break;
                case "Разрешено":
                    filtered = filtered.Where(m => m.test_status?.ToString() == "approved");
                    break;
                case "Заблокировано":
                    filtered = filtered.Where(m => m.test_status?.ToString() == "blocked");
                    break;
            }

            dgRawMaterials.ItemsSource = filtered.ToList();
            txtTotalCount.Text = filtered.Count().ToString();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRawMaterials();
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            var material = (sender as Button).Tag;
            var testWindow = new TestWindow(material, "raw_material");
            testWindow.Owner = Window.GetWindow(this);
            testWindow.ShowDialog();
            LoadRawMaterials(); // Обновляем после завершения
        }

        private void DgRawMaterials_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var material = dgRawMaterials.SelectedItem;
            if (material != null)
            {
                var testWindow = new TestWindow(material, "raw_material");
                testWindow.Owner = Window.GetWindow(this);
                testWindow.ShowDialog();
                LoadRawMaterials();
            }
        }
    }
}
