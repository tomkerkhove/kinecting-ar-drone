using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using K4W.KinectDrone.Core.Enums;

namespace K4W.KinectDrone.Client.UserControls.Converters
{
    public class DroneStatusToBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(DroneConnection))
                return Brushes.Red;

            switch ((DroneConnection)value)
            {
                case DroneConnection.Connected:
                    return Brushes.Green;
                case DroneConnection.NotConnected:
                    return Brushes.Red;
                case DroneConnection.Unknown:
                    return Brushes.OrangeRed;
            }

            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
