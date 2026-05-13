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
    
        public partial class TechCardDialog : Window
        {
        private TechCard _techCard;

            public TechCardDialog(TechCard techCard = null)
            {
                InitializeComponent();
                _techCard = techCard;
                LoadProducts();

                if (techCard != null)
                {
                    Title = "Редактирование техкарты";
                    cmbProduct.SelectedValue = techCard.product_id;
                    cmbProduct.IsEnabled = false;
                }
            }

            private async void LoadProducts()
            {
                try
                {
                    var products = await ApiClient.GetAsync<List<Product>>("products/active");
                    cmbProduct.ItemsSource = products;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}");
                }
            }

            private async void BtnSave_Click(object sender, RoutedEventArgs e)
            {
                if (cmbProduct.SelectedValue == null)
                {
                    MessageBox.Show("Выберите продукт", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var request = new
                    {
                        product_id = (int)cmbProduct.SelectedValue
                    };

                    var result = await ApiClient.PostAsync<dynamic>("techcards", request);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка создания: {ex.Message}");
                }
            }

            private void BtnCancel_Click(object sender, RoutedEventArgs e)
            {
                DialogResult = false;
                Close();
            }
        }
    }

