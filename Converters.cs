using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AeroFleetManagerPro.Models;

namespace AeroFleetManagerPro.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }

    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isInverted = parameter?.ToString() == "Invert";
            var isVisible = value != null;
            
            if (isInverted)
                isVisible = !isVisible;
            
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AircraftStatus status)
            {
                return status switch
                {
                    AircraftStatus.Active => new SolidColorBrush(Colors.Green),
                    AircraftStatus.Maintenance => new SolidColorBrush(Colors.Orange),
                    AircraftStatus.Grounded => new SolidColorBrush(Colors.Red),
                    AircraftStatus.Reserved => new SolidColorBrush(Colors.Blue),
                    AircraftStatus.Retired => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FlightStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlightStatus status)
            {
                return status switch
                {
                    FlightStatus.Scheduled => new SolidColorBrush(Colors.Blue),
                    FlightStatus.Boarding => new SolidColorBrush(Colors.Orange),
                    FlightStatus.Departed => new SolidColorBrush(Colors.Green),
                    FlightStatus.InFlight => new SolidColorBrush(Colors.LimeGreen),
                    FlightStatus.Arrived => new SolidColorBrush(Colors.DarkGreen),
                    FlightStatus.Cancelled => new SolidColorBrush(Colors.Red),
                    FlightStatus.Delayed => new SolidColorBrush(Colors.Orange),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MaintenanceStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MaintenanceStatus status)
            {
                return status switch
                {
                    MaintenanceStatus.Scheduled => new SolidColorBrush(Colors.Blue),
                    MaintenanceStatus.InProgress => new SolidColorBrush(Colors.Orange),
                    MaintenanceStatus.Completed => new SolidColorBrush(Colors.Green),
                    MaintenanceStatus.Cancelled => new SolidColorBrush(Colors.Gray),
                    MaintenanceStatus.Overdue => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DaysUntilConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is DateTime targetDate && values[1] is DateTime currentDate)
            {
                var days = (targetDate - currentDate).Days;
                return days.ToString();
            }
            return "N/A";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}