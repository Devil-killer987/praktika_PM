using OperatorMD.Helpers;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            txtUserName.Text = AppSettings.CurrentUserName;
            txtShiftLine.Text = $"{AppSettings.CurrentShift} | {AppSettings.CurrentLine}";

            MainFrame.Navigate(new ActiveBatchesPage());
        }

        private void BtnActiveBatches_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ActiveBatchesPage());
        }

        private void BtnProgram_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BatchProgramPage());
        }

        private void BtnExtruder_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ExtruderMonitorPage());
        }

        private void BtnReportProblem_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new ReportProblemWindow();
            reportWindow.Owner = this;
            reportWindow.ShowDialog();
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
