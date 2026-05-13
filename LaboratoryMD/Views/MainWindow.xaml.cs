using LaboratoryMD.Helpers;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            txtUserName.Text = AppSettings.CurrentUserName;
            txtUserRole.Text = "Лаборатория";

            // Загрузка страницы с сырьем по умолчанию
            MainFrame.Navigate(new RawMaterialsPage());
        }

        private void BtnRawMaterials_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RawMaterialsPage());
        }

        private void BtnFinishedProducts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new FinishedProductsPage());
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TestHistoryPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.Token = "";
            AppSettings.CurrentUserId = 0;

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
