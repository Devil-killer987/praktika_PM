using LaboratoryMD.Helpers;
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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                txtError.Text = "Заполните логин и пароль";
                return;
            }

            try
            {
                btnLogin.IsEnabled = false;
                btnLogin.Content = "Вход...";

                var request = new LoginRequest
                {
                    Username = txtUsername.Text,
                    Password = txtPassword.Password
                };

                var response = await ApiClient.PostAsync<LoginResponse>("auth/login", request);

                if (response != null && !string.IsNullOrEmpty(response.token))
                {
                    AppSettings.Token = response.token;
                    AppSettings.CurrentUserId = response.id;
                    AppSettings.CurrentUserName = response.full_name;
                    AppSettings.CurrentUserRole = response.role;
                    ApiClient.SetAuthToken(response.token);

                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    txtError.Text = "Неверный логин или пароль";
                }
            }
            catch (Exception ex)
            {
                txtError.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                btnLogin.IsEnabled = true;
                btnLogin.Content = "Войти";
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
    }
}
