using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using K4W.KinectDrone.Core;
using K4W.KinectDrone.Core.Enums;

namespace K4W.KinectDrone.Client.UserControls.Converters
{
    public class DroneConnectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(DroneConnection))
                return Visibility.Collapsed;

            return ((DroneConnection)value == DroneConnection.Connected) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
