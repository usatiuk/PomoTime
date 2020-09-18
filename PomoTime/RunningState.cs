using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PomoTime
{
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
    }
}
