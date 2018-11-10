using System;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Kinect;

namespace K4W.KinectDrone.Client.UserControls.Converters
{
    public class KinectStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(KinectStatus))
                return Brushes.Red;

            switch ((KinectStatus)value)
            {
                case KinectStatus.Connected:
                    return Brushes.Green;
                case KinectStatus.Disconnected:
                    return Brushes.Red;
                default:
                    return Brushes.OrangeRed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
