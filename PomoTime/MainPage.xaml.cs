using System;
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
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PomoTime
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public RunningState runningState = new RunningState();
        private const int DefaultBreakMinutes = 5;
        private const int DefaultWorkMinutes = 25;
        private const int DefaultLongBreakMinutes = 15;


        private int BreakMinutes { get; set; }
        private int WorkMinutes { get; set; }
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
            this.Loaded += MainPageLoaded;

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
            runningState.MinutesLeft = WorkMinutes;
            runningState.SecondsLeft = 0;

        }

        void timer_Tick()
        {
            if (!runningState.IsRunning)
            {
                return;
            }
            if (runningState.SecondsLeft == 0)
            {
                runningState.SecondsLeft = 59;
                if (runningState.MinutesLeft == 0)
                {
                    switch (runningState.CurrentPeriod)
                    {
                        case Period.Work:
                            if (runningState.PreviousShortBreaks != 4)
                            {
                                runningState.CurrentPeriod = Period.ShortBreak;
                                runningState.MinutesLeft = BreakMinutes;
                            }
                            else
                            {
                                runningState.CurrentPeriod = Period.LongBreak;
                                runningState.MinutesLeft = LongBreakMinutes;
                            }
                            runningState.SecondsLeft = 0;

                            break;
                        case Period.ShortBreak:
                            runningState.CurrentPeriod = Period.Work;
                            runningState.PreviousShortBreaks += 1;
                            runningState.MinutesLeft = WorkMinutes;
                            runningState.SecondsLeft = 0;
                            break;
                        case Period.LongBreak:
                            runningState.CurrentPeriod = Period.Work;
                            runningState.PreviousShortBreaks = 0;
                            runningState.MinutesLeft = WorkMinutes;
                            runningState.SecondsLeft = 0;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    runningState.MinutesLeft--;
                }
            }
            else
            {
                runningState.SecondsLeft--;
            }
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            runningState.IsRunning = true;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            runningState.IsRunning = false;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            runningState.IsRunning = false;

            runningState.CurrentPeriod = Period.Work;
            runningState.MinutesLeft = WorkMinutes;
            runningState.SecondsLeft = 0;
        }

        private void Plus1Button_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            runningState.MinutesLeft += 1;
        }

        private void Plus5Button_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            runningState.MinutesLeft += 5;
        }

        private void Plus10Button_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            runningState.MinutesLeft += 10;
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
            deferral.Complete();
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            ThreadPoolTimer timer = ThreadPoolTimer.CreatePeriodicTimer(async (t) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    timer_Tick();
                });
            }, TimeSpan.FromSeconds(1));
        }
    }
}
