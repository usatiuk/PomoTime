﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using controls = Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Microsoft.Toolkit.Uwp.Notifications;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PomoTime
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public RunningState MainViewRunningState = new RunningState();
        private const int DefaultBreakMinutes = 5;
        private const int DefaultWorkMinutes = 25;
        private const int DefaultLongBreakMinutes = 15;
        ThreadPoolTimer Timer;

        private DateTime SuspendTime;
        private int _work_minutes;
        private int BreakMinutes { get; set; }
        private int WorkMinutes
        {
            get { return _work_minutes; }
            set
            {
                if (!MainViewRunningState.IsRunning && MainViewRunningState.CurrentPeriod == Period.Work)
                {
                    MainViewRunningState.MinutesLeft = value;
                }
                _work_minutes = value;
            }
        }
        private int LongBreakMinutes { get; set; }


        public MainPage()
        {
            this.InitializeComponent();

            // Some maigc numbers
            Size defaultSize = new Size(500, 250);

            ApplicationView.PreferredLaunchViewSize = defaultSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(defaultSize);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            Application.Current.Suspending += OnSuspending;
            Application.Current.Resuming += OnResuming;

            this.Loaded += MainPageLoaded;
            this.Unloaded += MainPageUnloaded;

            ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            Windows.Storage.ApplicationDataCompositeValue minutes = (ApplicationDataCompositeValue)roamingSettings.Values["Minutes"];
            if (minutes != null)
            {
                if (minutes["WorkMinutes"] != null)
                {
                    WorkMinutes = (int)minutes["WorkMinutes"];
                }
                else
                {
                    WorkMinutes = DefaultWorkMinutes;
                }
                if (minutes["BreakMinutes"] != null)
                {
                    BreakMinutes = (int)minutes["BreakMinutes"];
                }
                else
                {
                    BreakMinutes = DefaultBreakMinutes;
                }
                if (minutes["LongBreakMinutes"] != null)
                {
                    LongBreakMinutes = (int)minutes["LongBreakMinutes"];
                }
                else
                {
                    LongBreakMinutes = DefaultLongBreakMinutes;
                }
            }
            else
            {
                // Some maigc defualt numbers
                WorkMinutes = DefaultWorkMinutes;
                BreakMinutes = DefaultBreakMinutes;
                LongBreakMinutes = DefaultLongBreakMinutes;
            }
        }

        void TimerTick()
        {
            if (!MainViewRunningState.IsRunning)
            {
                return;
            }
            if (MainViewRunningState.SecondsLeft == 0)
            {
                MainViewRunningState.SecondsLeft = 59;
                if (MainViewRunningState.MinutesLeft == 0)
                {
                    switch (MainViewRunningState.CurrentPeriod)
                    {
                        case Period.Work:
                            if (MainViewRunningState.PreviousShortBreaks != 4)
                            {
                                MainViewRunningState.CurrentPeriod = Period.ShortBreak;
                                MainViewRunningState.MinutesLeft = BreakMinutes;
                            }
                            else
                            {
                                MainViewRunningState.CurrentPeriod = Period.LongBreak;
                                MainViewRunningState.MinutesLeft = LongBreakMinutes;
                            }
                            MainViewRunningState.SecondsLeft = 0;

                            break;
                        case Period.ShortBreak:
                            MainViewRunningState.CurrentPeriod = Period.Work;
                            MainViewRunningState.PreviousShortBreaks += 1;
                            MainViewRunningState.MinutesLeft = WorkMinutes;
                            MainViewRunningState.SecondsLeft = 0;
                            break;
                        case Period.LongBreak:
                            MainViewRunningState.CurrentPeriod = Period.Work;
                            MainViewRunningState.PreviousShortBreaks = 0;
                            MainViewRunningState.MinutesLeft = WorkMinutes;
                            MainViewRunningState.SecondsLeft = 0;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    SchedulePeriodOverNotification();
                }
                else
                {
                    MainViewRunningState.MinutesLeft--;
                }
            }
            else if (MainViewRunningState.SecondsLeft == 1 && MainViewRunningState.MinutesLeft == 0)
            {
                MainViewRunningState.IsRunning = false;
                MainViewRunningState.SecondsLeft--;
            }
            else
            {
                MainViewRunningState.SecondsLeft--;
            }
            SaveLocalState();
        }

        void SchedulePeriodOverNotification()
        {
            string header = $"{MainViewRunningState.CurrentPeriod.Name()} is over!";
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = header
                        },
                        new AdaptiveText()
                        {
                            Text = "Open app or the notification to continue"
                        },

                    },

                }
            };

            ToastActionsCustom actions = new ToastActionsCustom()
            {
                Buttons =
                {
                    new ToastButton("Continue", "action=continue")
                    {
                        ActivationType = ToastActivationType.Foreground
                    },

                    new ToastButton("Add 5 minutes", "action=5minutes")
                    {
                        ActivationType = ToastActivationType.Foreground
                    }
                }
            };

            ToastContent toastContent = new ToastContent()
            {
                Visual = visual,
                Actions = actions,
                Scenario = ToastScenario.Alarm,
            };

            TimeSpan WaitTime = new TimeSpan(0, MainViewRunningState.MinutesLeft, MainViewRunningState.SecondsLeft);
            var toast = new Windows.UI.Notifications.ScheduledToastNotification(toastContent.GetXml(), DateTime.Now + WaitTime);
            Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
        }

        private void ClearScheduledNotifications()
        {
            ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();
            IReadOnlyList<ScheduledToastNotification> scheduledToasts = notifier.GetScheduledToastNotifications();
            foreach (ScheduledToastNotification n in scheduledToasts)
            {
                notifier.RemoveFromSchedule(n);
            }
        }

        private void RescheduleNotification()
        {
            ClearScheduledNotifications();

            if(!MainViewRunningState.IsRunning)
            {
                return;
            }

            if (MainViewRunningState.MinutesLeft != 0 || MainViewRunningState.SecondsLeft != 0)
            {
                SchedulePeriodOverNotification();
            }
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            MainViewRunningState.IsRunning = true;
            MainViewRunningState.StartTime = DateTime.Now;
            SaveLocalState();

            if (MainViewRunningState.MinutesLeft != 0 || MainViewRunningState.SecondsLeft != 0)
            {
                SchedulePeriodOverNotification();
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            MainViewRunningState.IsRunning = false;

            ClearScheduledNotifications();
        }

        private void Reset()
        {
            MainViewRunningState.IsRunning = false;

            MainViewRunningState.CurrentPeriod = Period.Work;
            MainViewRunningState.MinutesLeft = WorkMinutes;
            MainViewRunningState.SecondsLeft = 0;

            ClearScheduledNotifications();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;

            Reset();
        }

        private void Plus1Button_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            MainViewRunningState.MinutesLeft += 1;

            RescheduleNotification();
        }

        private void Plus5Button_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            MainViewRunningState.MinutesLeft += 5;

            RescheduleNotification();
        }

        private void Plus10Button_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            MainViewRunningState.MinutesLeft += 10;

            RescheduleNotification();
        }
        private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            Windows.Storage.ApplicationDataCompositeValue minutes = new Windows.Storage.ApplicationDataCompositeValue();
            minutes["WorkMinutes"] = WorkMinutes;
            minutes["BreakMinutes"] = BreakMinutes;
            minutes["LongBreakMinutes"] = LongBreakMinutes;
            roamingSettings.Values["Minutes"] = minutes;

            SaveLocalState();

            deferral.Complete();
        }

        private void SaveLocalState()
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            SuspendTime = DateTime.Now;
            localSettings.Values["SuspendTime"] = SuspendTime.Ticks;
            localSettings.Values["StartTime"] = MainViewRunningState.StartTime.Ticks;
            localSettings.Values["MinutesLeft"] = MainViewRunningState.MinutesLeft;
            localSettings.Values["SecondsLeft"] = MainViewRunningState.SecondsLeft;
            localSettings.Values["IsRunning"] = MainViewRunningState.IsRunning;
            localSettings.Values["PreviousShortBreaks"] = MainViewRunningState.PreviousShortBreaks;
            localSettings.Values["CurrentPeriod"] = (int)MainViewRunningState.CurrentPeriod;
        }

        private void FastForwardTime(DateTime since)
        {
            TimeSpan TimeFromSuspend = DateTime.Now - since;
            if(TimeFromSuspend.TotalMilliseconds < 0)
            {
                return;
            }
            if (!MainViewRunningState.IsRunning)
            {
                return;
            }
            if (TimeFromSuspend.TotalSeconds >= MainViewRunningState.MinutesLeft * 60 + MainViewRunningState.SecondsLeft)
            {
                MainViewRunningState.IsRunning = false;
                MainViewRunningState.MinutesLeft = 0;
                MainViewRunningState.SecondsLeft = 0;
                return;
            }

            if (TimeFromSuspend.Seconds > MainViewRunningState.SecondsLeft)
            {
                MainViewRunningState.MinutesLeft -= TimeFromSuspend.Minutes + 1;
                MainViewRunningState.SecondsLeft = MainViewRunningState.SecondsLeft + 60 - TimeFromSuspend.Seconds;
                return;
            }

            MainViewRunningState.MinutesLeft -= TimeFromSuspend.Minutes;
            MainViewRunningState.SecondsLeft -= TimeFromSuspend.Seconds;

        }

        private void OnResuming(object sender, Object e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values["SuspendTime"] != null)
            {
                SuspendTime = new DateTime((long)localSettings.Values["SuspendTime"]);
                FastForwardTime(SuspendTime);
            }
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            if (Timer == null)
            {
                Timer = ThreadPoolTimer.CreatePeriodicTimer(async (t) =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High,
                        () =>
                             {
                                 TimerTick();
                             });
                }, TimeSpan.FromSeconds(1));
            }
        }

        private void MainPageUnloaded(object sender, RoutedEventArgs e)
        {
            Timer.Cancel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainViewRunningState = (RunningState)e.Parameter;

            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values["SuspendTime"] != null)
            {
                SuspendTime = new DateTime((long)localSettings.Values["SuspendTime"]);
                FastForwardTime(SuspendTime);
            }
            else
            {
                Reset();
                SaveLocalState();
            }

            RescheduleNotification();
        }

    }
}
