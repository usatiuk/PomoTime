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


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Pomotimer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer dispatcherTimer;
        public RunningState runningState = new RunningState();

        private int RestMinutes { get; set; }
        private int WorkMinutes { get; set; }

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
                WorkMinutes = (int)minutes["WorkMinutes"];
                RestMinutes = (int)minutes["RestMinutes"];
            }
            else
            {
                // Some maigc defualt numbers
                WorkMinutes = 40;
                RestMinutes = 5;
            }
            runningState.MinutesLeft = WorkMinutes;
            runningState.SecondsLeft = 0;

        }
        public void DispatcherTimerSetup()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        }

        void dispatcherTimer_Tick(object sender, object e)
        {
            if (runningState.SecondsLeft == 0)
            {
                runningState.SecondsLeft = 59;
                if (runningState.MinutesLeft == 0)
                {
                    if (runningState.OnRest)
                    {
                        runningState.OnRest = false;
                        runningState.MinutesLeft = WorkMinutes;
                        runningState.SecondsLeft = 0;
                    }
                    else
                    {
                        runningState.OnRest = true;
                        runningState.MinutesLeft = RestMinutes;
                        runningState.SecondsLeft = 0;
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
            dispatcherTimer.Start();
            runningState.IsRunning = true;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            dispatcherTimer.Stop();
            runningState.IsRunning = false;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton b = sender as AppBarButton;
            dispatcherTimer.Stop();
            runningState.IsRunning = false;

            runningState.OnRest = false;
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
            minutes["RestMinutes"] = RestMinutes;
            roamingSettings.Values["Minutes"] = minutes;
            deferral.Complete();
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimerSetup();
        }
    }
}
