using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AR.Drone.Data.Navigation;

namespace K4W.KinectDrone.Client.UserControls.Converters
{
    class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Enum myEnum = (Enum)value;

            string description = GetEnumDescription(myEnum);

            return description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }


        private string GetEnumDescription(Enum enumObj)
        {
            // Get the Type
            Type t = enumObj.GetType();

            // Get fields
            var fieldInfo = t.GetFields();

            if (fieldInfo.Count() > 1)
            {
                if (t != typeof(NavigationState))
                {
                    return enumObj.ToString();
                }
                else
                {
                    NavigationState state = (NavigationState)enumObj;
                    state &= ~NavigationState.Watchdog;
                    state &= ~NavigationState.Command;
                    state &= ~NavigationState.Bootstrap;
                    state &= ~NavigationState.Control;
                    return state.ToString();
                }
            }
            else
            {
                object[] attribArray = fieldInfo[0].GetCustomAttributes(false);

                if (attribArray.Length == 0)
                {
                    return enumObj.ToString();
                }
                else
                {
                    DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
                    return attrib.Description;
                }
            }
        }

        private string GetFieldInfoAttribute(FieldInfo info, Enum enumObj)
        {
            object[] attribArray = info.GetCustomAttributes(false);

            if (attribArray.Length == 0)
            {
                return enumObj.ToString();
            }
            else
            {
                DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
                return attrib.Description;
            }
        }
    }
}
