using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PomoTime
{
    public static class PeriodExtensions
    {
        public static string Name(this Period period)
        {
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

            return output;
        }
    }

    public enum Period
    {
        Work, ShortBreak, LongBreak
    }
}
