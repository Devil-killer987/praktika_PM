using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LaboratoryMD.Helpers
{
    public class StatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value?.ToString() ?? "";
            return (status == "pending" || status == "in_progress" ||
                    status == "Ожидает контроля" || status == "В работе")
                    ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Visibility.Visible;
        }
    }

    public class ResultToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = value?.ToString() ?? "";

            if (result == "pass")
                return new SolidColorBrush(Colors.Green);
            else if (result == "fail")
                return new SolidColorBrush(Colors.Red);
            else if (result == "pending")
                return new SolidColorBrush(Colors.Orange);
            else
                return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
