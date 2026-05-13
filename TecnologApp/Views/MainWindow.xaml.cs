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
using TecnologApp.Helpers;
using TecnologApp.Models;
using TecnologApp.Services;

namespace TecnologApp.Views
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
            txtUserRole.Text = GetRoleName(AppSettings.CurrentUserRole);

            MainFrame.Navigate(new DashboardPage());
        }

        private string GetRoleName(string role)
        {
            if (role == "technologist")
                return "Технолог";
            else if (role == "admin")
                return "Администратор";
            else if (role == "manager")
                return "Руководитель";
            else
                return role;
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }

        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProductsPage());
        }

        private void BtnRecipes_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RecipesPage());
        }

        private void BtnTechCards_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TechCardsPage());
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrdersPage());
        }

        private void BtnBatches_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BatchesPage());
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReportsPage());
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
