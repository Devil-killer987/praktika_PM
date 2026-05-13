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
    /// Логика взаимодействия для TestHistoryWindow.xaml
    /// </summary>
    public partial class TestHistoryPage : Page
    {
        public TestHistoryPage()
        {
            InitializeComponent();
            LoadHistory();
        }

        private async void LoadHistory()
        {
            try
            {
                var history = await ApiClient.GetAsync<List<dynamic>>("qualitytests");
                dgHistory.ItemsSource = history;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}");
            }
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            var test = (sender as Button).Tag;
            var detailsWindow = new TestDetailsWindow(test);
            detailsWindow.Owner = Window.GetWindow(this);
            detailsWindow.ShowDialog();
        }

        private void DgHistory_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var test = dgHistory.SelectedItem;
            if (test != null)
            {
                var detailsWindow = new TestDetailsWindow(test);
                detailsWindow.Owner = Window.GetWindow(this);
                detailsWindow.ShowDialog();
            }
        }
    }
}
