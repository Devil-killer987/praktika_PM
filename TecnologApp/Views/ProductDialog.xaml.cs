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
using TecnologApp.Models;
using TecnologApp.Services;

namespace TecnologApp.Views
{
    /// <summary>
    /// Логика взаимодействия для ProductDialog.xaml
    /// </summary>
    public partial class ProductDialog : Window
    {
        private Product _product;

        public ProductDialog(Product product = null)
        {
            InitializeComponent();
            _product = product;

            if (product != null)
            {
                txtCode.Text = product.code;
                txtName.Text = product.name;
                cmbType.Text = product.product_type;
                cmbForm.Text = product.release_form;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_product == null)
                {
                    // Создание нового продукта
                    var newProduct = new
                    {
                        code = txtCode.Text,
                        name = txtName.Text,
                        product_type = (cmbType.SelectedItem as ComboBoxItem)?.Content.ToString(),
                        release_form = (cmbForm.SelectedItem as ComboBoxItem)?.Content.ToString()
                    };
                    await ApiClient.PostAsync<dynamic>("products", newProduct);
                }
                else
                {
                    // Обновление существующего
                    _product.code = txtCode.Text;
                    _product.name = txtName.Text;
                    _product.product_type = (cmbType.SelectedItem as ComboBoxItem)?.Content.ToString();
                    _product.release_form = (cmbForm.SelectedItem as ComboBoxItem)?.Content.ToString();
                    await ApiClient.PutAsync<dynamic>($"products/{_product.id}", _product);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
