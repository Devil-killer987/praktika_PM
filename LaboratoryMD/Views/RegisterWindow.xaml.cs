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
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtError.Text = "Заполните все обязательные поля";
                return;
            }

            try
            {
                btnRegister.IsEnabled = false;
                btnRegister.Content = "Регистрация...";

                var request = new
                {
                    Username = txtUsername.Text,
                    Password = txtPassword.Password,
                    FullName = txtFullName.Text,
                    Email = txtEmail.Text,
                    Phone = txtPhone.Text,
                    Role = "laboratory",
                    Department = txtDepartment.Text
                };

                await ApiClient.PostAsync<dynamic>("auth/register", request);

                MessageBox.Show("Регистрация успешна!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                txtError.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                btnRegister.IsEnabled = true;
                btnRegister.Content = "Зарегистрироваться";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
