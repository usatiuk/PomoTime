﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PomoTime
{
    public class PeriodToStringConverter : IValueConverter
    {

        #region IValueConverter Members

        // Define the Convert method to change a DateTime object to 
        // a month string.
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            // The value parameter is the data from the source object.
            Period period = (Period)value;
            string output;
            switch (period)
            {
                case Period.LongBreak:
                    output = "Long break";
                    break;
                case Period.ShortBreak:
                    output = "Short break";
                    break;
                case Period.Work:
                    output = "Work";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Return the month value to pass to the target.
            return output;
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
