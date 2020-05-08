﻿using Microsoft.QueryStringDotNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PomoTime
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public RunningState RunningState = new RunningState();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        private void OnLaunchedOrActivated(IActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated
                    || e.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
                {
                    ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    if (localSettings.Values["MinutesLeft"] != null)
                    {
                        RunningState.MinutesLeft = (int)localSettings.Values["MinutesLeft"];
                        RunningState.SecondsLeft = (int)localSettings.Values["SecondsLeft"];
                        RunningState.IsRunning = (bool)localSettings.Values["IsRunning"];
                        RunningState.PreviousShortBreaks = (int)localSettings.Values["PreviousShortBreaks"];
                        RunningState.CurrentPeriod = (Period)localSettings.Values["CurrentPeriod"];
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e is LaunchActivatedEventArgs && ((LaunchActivatedEventArgs)e).PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), RunningState);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
            if (e is ToastNotificationActivatedEventArgs)
            {
                var toastActivationArgs = e as ToastNotificationActivatedEventArgs;

                ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values["MinutesLeft"] != null)
                {
                    RunningState.MinutesLeft = (int)localSettings.Values["MinutesLeft"];
                    RunningState.SecondsLeft = (int)localSettings.Values["SecondsLeft"];
                    RunningState.IsRunning = (bool)localSettings.Values["IsRunning"];
                    RunningState.PreviousShortBreaks = (int)localSettings.Values["PreviousShortBreaks"];
                    RunningState.CurrentPeriod = (Period)localSettings.Values["CurrentPeriod"];
                }

                QueryString args = QueryString.Parse(toastActivationArgs.Argument);
                if (args.Contains("action"))
                {
                    localSettings.Values["SuspendTime"] = (DateTime.Now + new TimeSpan(1, 0, 0)).Ticks;
                    switch (args["action"])
                    {
                        case "continue":
                            RunningState.IsRunning = true;
                            RunningState.MinutesLeft = 0;
                            RunningState.SecondsLeft = 0;
                            break;
                        case "5minutes":
                            RunningState.MinutesLeft += 5;
                            RunningState.IsRunning = true;
                            break;
                    }
                }

                rootFrame.Navigate(typeof(MainPage), RunningState);
                Window.Current.Activate();
            }
        }
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            OnLaunchedOrActivated(e);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
