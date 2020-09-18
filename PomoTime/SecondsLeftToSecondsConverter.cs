using System;
using Windows.UI.Xaml.Data;

namespace PomoTime
{
    public class SecondsLeftToSecondsConverter : IValueConverter
    {

        #region IValueConverter Members

        // Define the Convert method to change a DateTime object to 
        // a month string.
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            // The value parameter is the data from the source object.
            int secondsLeft = (int)value;
            return String.Format("{0:00}", secondsLeft % 60);
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
