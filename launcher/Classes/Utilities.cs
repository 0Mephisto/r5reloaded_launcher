﻿using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;

namespace launcher
{
    /// <summary>
    /// The Utilities class provides various utility methods for setting up the application,
    /// managing the game launch process, handling UI updates, and performing version checks.
    /// It includes methods for initializing the main window components, launching the game,
    /// checking for new versions, updating UI elements, and controlling the visibility of
    /// progress bars and settings controls.
    /// </summary>
    public static class Utilities
    {
        public static void SetupApp(MainWindow mainWindow)
        {
            Console.WriteLine("Setting up launcher");

            ControlReferences.App = mainWindow;
            ControlReferences.dispatcher = mainWindow.Dispatcher;
            ControlReferences.progressBar = mainWindow.progressBar;
            ControlReferences.lblStatus = mainWindow.lblStatus;
            ControlReferences.lblFilesLeft = mainWindow.lblFilesLeft;
            ControlReferences.cmbBranch = mainWindow.cmbBranch;
            ControlReferences.btnPlay = mainWindow.btnPlay;
            ControlReferences.settingsControl = mainWindow.SettingsControl;
            ControlReferences.subMenuControl = mainWindow.subMenuControl;
            ControlReferences.TransitionRect = mainWindow.TransitionRect;
            ControlReferences.SubMenuPopup = mainWindow.SubMenuPopup;
            ControlReferences.downloadsPopupControl = mainWindow.DownloadsPopupControl;

            ControlReferences.App.launcherVersionlbl.Text = Global.launcherVersion;

            ShowProgressBar(false);

            Global.launcherPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            Console.WriteLine($"Launcher path: {Global.launcherPath}");

            Global.serverConfig = DataFetcher.FetchServerConfig();
            Console.WriteLine($"Server config fetched");

            Global.launcherConfig = FileManager.GetLauncherConfig();
            Console.WriteLine($"Launcher config found");

            Global.isInstalled = Global.launcherConfig != null;
            Console.WriteLine($"Is game installed: {Global.isInstalled}");

            ControlReferences.cmbBranch.ItemsSource = SetupGameBranches();
            ControlReferences.cmbBranch.SelectedIndex = 0;
            Console.WriteLine("Game branches set up");
        }

        public static List<ComboBranch> SetupGameBranches()
        {
            return Global.serverConfig.branches
                .Select(branch => new ComboBranch
                {
                    title = branch.branch,
                    subtext = branch.enabled ? branch.currentVersion : "branch disabled"
                })
                .ToList();
        }

        public static void LaunchGame()
        {
            Console.WriteLine("Launching Game");

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start \"\" \"{Global.launcherPath}\\r5apex.exe\""
            };

            // Start the new process via cmd
            Process.Start(startInfo);
        }

        public static void SetInstallState(bool installing, string buttonText = "PLAY")
        {
            Console.WriteLine($"Set install state to: {installing} | {buttonText}");

            ControlReferences.dispatcher.Invoke(() =>
            {
                Global.isInstalling = installing;

                ControlReferences.btnPlay.Content = buttonText;
                ControlReferences.cmbBranch.IsEnabled = !installing;
                ControlReferences.btnPlay.IsEnabled = !installing;
                ControlReferences.lblStatus.Text = "";
                ControlReferences.lblFilesLeft.Text = "";
            });

            ShowProgressBar(installing);
        }

        public static void UpdateStatusLabel(string statusText)
        {
            Console.WriteLine($"Updating status label: {statusText}");
            ControlReferences.dispatcher.Invoke(() =>
            {
                ControlReferences.lblStatus.Text = statusText;
            });
        }

        private static void ShowProgressBar(bool isVisible)
        {
            ControlReferences.dispatcher.Invoke(() =>
            {
                ControlReferences.progressBar.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
                ControlReferences.lblStatus.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
                ControlReferences.lblFilesLeft.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            });
        }

        public static int GetCmbBranchIndex()
        {
            int cmbSelectedIndex = 0;

            //TODO: FOR BRANCHES LATER, ONLY ALLOWING MAIN BRANCH FOR NOW
            //mainWindow.Dispatcher.Invoke(() =>
            //{
            //    cmbSelectedIndex = mainWindow.cmbBranch.SelectedIndex;
            //});

            return cmbSelectedIndex;
        }

        public static void ShowSettingsControl()
        {
            var transitionInStoryboard = CreateTransitionStoryboard(-2400, 0, 0.25);
            transitionInStoryboard.Completed += (s, e) =>
            {
                ControlReferences.settingsControl.Visibility = Visibility.Visible;
                var fadeInStoryboard = CreateFadeStoryboard(0, 1, 0.2);
                fadeInStoryboard.Completed += (s, e) =>
                {
                    var transitionOutStoryboard = CreateTransitionStoryboard(0, 2400, 0.25);
                    transitionOutStoryboard.Begin();
                };
                fadeInStoryboard.Begin();
            };
            transitionInStoryboard.Begin();
            ControlReferences.subMenuControl.Settings.IsEnabled = false;
        }

        public static void HideSettingsControl()
        {
            var transitionInStoryboard = CreateTransitionStoryboard(2400, 0, 0.5);
            transitionInStoryboard.Completed += (s, e) =>
            {
                var fadeOutStoryboard = CreateFadeStoryboard(1, 0, 0.2);
                fadeOutStoryboard.Completed += (s, e) =>
                {
                    ControlReferences.settingsControl.Visibility = Visibility.Hidden;
                    var transitionOutStoryboard = CreateTransitionStoryboard(0, -2400, 0.25);
                    transitionOutStoryboard.Begin();
                };
                fadeOutStoryboard.Begin();
            };
            transitionInStoryboard.Begin();
            ControlReferences.subMenuControl.Settings.IsEnabled = true;
        }

        private static Storyboard CreateTransitionStoryboard(double from, double to, double duration)
        {
            var storyboard = new Storyboard();
            var doubleAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(duration))
            };
            Storyboard.SetTarget(doubleAnimation, ControlReferences.TransitionRect);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("RenderTransform.Children[0].X"));
            storyboard.Children.Add(doubleAnimation);
            return storyboard;
        }

        private static Storyboard CreateFadeStoryboard(double from, double to, double duration)
        {
            var storyboard = new Storyboard();
            var doubleAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(duration))
            };
            Storyboard.SetTarget(doubleAnimation, ControlReferences.settingsControl);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(doubleAnimation);
            return storyboard;
        }
    }
}