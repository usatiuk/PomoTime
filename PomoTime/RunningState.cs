using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class RunningState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _is_running;
        public bool IsRunning
        {
            get { return _is_running; }
            set
            {
                _is_running = value;
                NotifyPropertyChanged("IsRunning");
            }

        }

        public int _minutes_left;
        public int MinutesLeft
        {
            get { return _minutes_left; }
            set
            {
                _minutes_left = value;
                NotifyPropertyChanged("MinutesLeft");
            }

        }

        public int _seconds_left;
        public int SecondsLeft
        {
            get { return _seconds_left; }
            set
            {
                _seconds_left = value;
                NotifyPropertyChanged("SecondsLeft");
            }

        }

        public Period _period;

        public Period CurrentPeriod
        {
            get { return _period; }
            set
            {
                _period = value;
                NotifyPropertyChanged("CurrentPeriod");
            }

        }

        public DateTime _start_time;

        public DateTime StartTime
        {
            get { return _start_time; }
            set
            {
                _start_time = value;
                NotifyPropertyChanged("StartTime");
            }
        }

        public int _previous_short_breaks;

        public int PreviousShortBreaks
        {
            get { return _previous_short_breaks; }
            set
            {
                _previous_short_breaks = value;
                NotifyPropertyChanged("PreviousShortBreaks");
            }
        }

        public string CurrentPeriodName()
        {
            return CurrentPeriod.Name();
        }
    }


}
