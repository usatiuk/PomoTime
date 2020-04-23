using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        public bool _on_rest;

        public bool OnRest
        {
            get { return _on_rest; }
            set
            {
                _on_rest = value;
                NotifyPropertyChanged("OnRest");
            }

        }
    }
}
