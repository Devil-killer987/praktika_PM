using OperatorMD.Helpers;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OperatorMD.Views
{
    /// <summary>
    /// Логика взаимодействия для ExtruderMonitorPage.xaml
    /// </summary>
    public partial class ExtruderMonitorPage : Page
    {
        private DispatcherTimer _refreshTimer;
        private Random _random = new Random();

        public ExtruderMonitorPage()
        {
            InitializeComponent();

            txtLine.Text = AppSettings.CurrentLine;
            LoadTelemetry();

            // Обновление каждые 2 секунды (симуляция реального времени)
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(1);
            _refreshTimer.Tick += (s, e) => LoadTelemetry();
            _refreshTimer.Start();
        }

        private async void LoadTelemetry()
        {
            try
            {
                // Пытаемся получить реальные данные с API
                var telemetry = await ApiClient.GetAsync<TelemetryData>("equipment/extruder/telemetry");

                UpdateDisplay(telemetry);
            }
            catch
            {
                // Если API не готов, используем симуляцию
                SimulateTelemetry();
            }
        }

        private void SimulateTelemetry()
        {
            // Симуляция случайных данных для демонстрации
            var telemetry = new TelemetryData
            {
                current_temperature = 70 + (decimal)_random.NextDouble() * 20,
                current_pressure = 2.5m + (decimal)_random.NextDouble() * 1.5m,
                current_rpm = 1500 + _random.Next(500),
                equipment_status = "Работает",
                last_update = DateTime.Now.ToString("HH:mm:ss")
            };

            UpdateDisplay(telemetry);
        }

        private void UpdateDisplay(TelemetryData telemetry)
        {
            // Температура
            txtTemperature.Text = telemetry.current_temperature.ToString("F1");
            tempProgress.Value = (double)telemetry.current_temperature;

            if (telemetry.current_temperature > 85)
            {
                txtTempStatus.Text = "Превышение!";
                txtTempStatus.Foreground = new SolidColorBrush(Colors.Red);
                tempProgress.Background = new SolidColorBrush(Colors.Red);
            }
            else if (telemetry.current_temperature < 65)
            {
                txtTempStatus.Text = "Низкая!";
                txtTempStatus.Foreground = new SolidColorBrush(Colors.Orange);
                tempProgress.Background = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                txtTempStatus.Text = "Норма";
                txtTempStatus.Foreground = new SolidColorBrush(Colors.Green);
                tempProgress.Background = new SolidColorBrush(Colors.LightGray);
            }

            // Давление
            txtPressure.Text = telemetry.current_pressure.ToString("F1");
            pressureProgress.Value = (double)telemetry.current_pressure;

            if (telemetry.current_pressure > 3.5m)
            {
                txtPressureStatus.Text = "Критическое!";
                txtPressureStatus.Foreground = new SolidColorBrush(Colors.Red);
                pressureProgress.Background = new SolidColorBrush(Colors.Red);
            }
            else if (telemetry.current_pressure > 3.0m)
            {
                txtPressureStatus.Text = "Повышенное";
                txtPressureStatus.Foreground = new SolidColorBrush(Colors.Orange);
                pressureProgress.Background = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                txtPressureStatus.Text = "Норма";
                txtPressureStatus.Foreground = new SolidColorBrush(Colors.Green);
                pressureProgress.Background = new SolidColorBrush(Colors.LightGray);
            }

            // Обороты
            txtRPM.Text = telemetry.current_rpm.ToString();
            rpmProgress.Value = telemetry.current_rpm;

            if (telemetry.current_rpm > 2500)
            {
                txtRPMStatus.Text = "Перегрузка";
                txtRPMStatus.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                txtRPMStatus.Text = "Норма";
                txtRPMStatus.Foreground = new SolidColorBrush(Colors.Green);
            }

            // Статус оборудования
            txtEquipmentStatus.Text = telemetry.equipment_status;
            txtLastUpdate.Text = $"Последнее обновление: {telemetry.last_update ?? DateTime.Now.ToString("HH:mm:ss")}";

            // Цвет статуса
            if (telemetry.equipment_status == "Работает")
                txtEquipmentStatus.Foreground = new SolidColorBrush(Colors.Green);
            else if (telemetry.equipment_status == "Предупреждение")
                txtEquipmentStatus.Foreground = new SolidColorBrush(Colors.Orange);
            else
                txtEquipmentStatus.Foreground = new SolidColorBrush(Colors.Red);
        }
    }
}
