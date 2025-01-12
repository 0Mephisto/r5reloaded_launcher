﻿using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static launcher.Global;

namespace launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int lastSelectedIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Utilities.CreateLauncherConfig();

            Utilities.SetupApp(this);

            if (!Utilities.GetIniSetting(Utilities.IniSettings.Disable_Animations, false))
            {
                OnOpen();
            }
            else
            {
                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                double windowWidth = this.Width;
                double windowHeight = this.Height;
                this.Left = (screenWidth / 2) - (windowWidth / 2);
                this.Top = (screenHeight / 2) - (windowHeight / 2);
            }

            btnPlay.Content = IS_INSTALLED ? "PLAY" : "INSTALL";
            if (!Utilities.GetIniSetting(Utilities.IniSettings.Installed, false) && File.Exists(Path.Combine(LAUNCHER_PATH, "r5apex.exe")))
                btnPlay.Content = "REPAIR";

            if (IS_INSTALLED)
            {
                if (IS_ONLINE)
                {
                    cmbBranch.SelectedItem = SERVER_CONFIG.branches.FirstOrDefault(b => b.branch == Utilities.GetIniSetting(Utilities.IniSettings.Current_Branch, ""));
                    Task.Run(() => UpdateChecker.Start());
                }
                else
                {
                    cmbBranch.IsEnabled = false;
                    cmbBranch.SelectedIndex = 0;
                }
            }

            bool useStaticImage = Utilities.GetIniSetting(Utilities.IniSettings.Disable_Background_Video, false);
            mediaImage.Visibility = useStaticImage ? Visibility.Visible : Visibility.Hidden;
            mediaElement.Visibility = useStaticImage ? Visibility.Hidden : Visibility.Visible;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            //Calculate half of the offset to move the form

            if (sizeInfo.HeightChanged)
                this.Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2;

            if (sizeInfo.WidthChanged)
                this.Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2;
        }

        private async Task OnOpen()
        {
            // Set initial off-screen state
            this.Opacity = 0;
            MainUI.Opacity = 0;
            this.Width = 0;

            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);

            // Create a storyboard for simultaneous animations
            var storyboard = new Storyboard();

            // Animate Width
            var widthAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1340,
                Duration = new Duration(TimeSpan.FromSeconds(0.75)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(widthAnimation, this);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath("Width"));

            // Animate Opacity
            var opacityAnimationWindow = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0.75)), // Match the duration of the width animation
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(opacityAnimationWindow, this);
            Storyboard.SetTargetProperty(opacityAnimationWindow, new PropertyPath("Opacity"));

            // Add animations to the storyboard
            storyboard.Children.Add(widthAnimation);
            storyboard.Children.Add(opacityAnimationWindow);

            // Begin the storyboard and await its completion
            TaskCompletionSource<bool> tcs = new();
            storyboard.Completed += (s, e) => tcs.SetResult(true);
            storyboard.Begin();

            await tcs.Task;

            // Fade in MainUI opacity after width animation completes
            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(opacityAnimation, MainUI);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            var opacityStoryboard = new Storyboard();
            opacityStoryboard.Children.Add(opacityAnimation);
            opacityStoryboard.Begin();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (IS_INSTALLED)
            {
                if (UPDATE_REQUIRED)
                {
                    Task.Run(() => GameUpdate.Start());
                }
                else
                {
                    Utilities.LaunchGame();
                }
            }
            else if (!IS_INSTALLING)
            {
                if (!Utilities.GetIniSetting(Utilities.IniSettings.Installed, false) && File.Exists(Path.Combine(LAUNCHER_PATH, "r5apex.exe")))
                {
                    Task.Run(() => GameRepair.Start());
                }
                else
                {
                    Task.Run(() => GameInstall.Start());
                }
            }
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = new TimeSpan(0, 0, 1);
            mediaElement.Play();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void cmbBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IS_ONLINE)
                return;

            if (sender is not ComboBox comboBox) return;

            var selectedBranch = comboBox.SelectedIndex;

            if (!SERVER_CONFIG.branches[selectedBranch].enabled)
            {
                comboBox.SelectedIndex = lastSelectedIndex;
                return;
            }

            lastSelectedIndex = selectedBranch;

            if (IS_INSTALLED)
            {
                if (string.IsNullOrEmpty(Utilities.GetIniSetting(Utilities.IniSettings.Current_Branch, "")))
                    Utilities.SetIniSetting(Utilities.IniSettings.Current_Branch, SERVER_CONFIG.branches[0].branch);

                if (string.IsNullOrEmpty(Utilities.GetIniSetting(Utilities.IniSettings.Current_Version, "")))
                    Utilities.SetIniSetting(Utilities.IniSettings.Current_Version, SERVER_CONFIG.branches[0].currentVersion);

                if (SERVER_CONFIG.branches[0].branch == Utilities.GetIniSetting(Utilities.IniSettings.Current_Branch, ""))
                {
                    UPDATE_REQUIRED = false;
                    btnPlay.Content = "Play";
                }
                else
                {
                    btnPlay.Content = "Update";
                    UPDATE_REQUIRED = true;
                }
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPopup.IsOpen = true;
        }

        private void StatusBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusPopup.IsOpen = true;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (StatusPopup.IsOpen)
            {
                var offset = StatusPopup.HorizontalOffset;
                StatusPopup.HorizontalOffset = offset + 1;
                StatusPopup.HorizontalOffset = offset;
            }
        }

        private void StatusPopup_Unloaded(object sender, EventArgs e)
        {
            StatusBtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        private void StatusPopup_Loaded(object sender, EventArgs e)
        {
            StatusBtn.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        }

        private void SubMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            SubMenuPopup.IsOpen = true;
        }

        private void DownloadsBtn_Click(object sender, RoutedEventArgs e)
        {
            DownloadsPopup.IsOpen = true;
        }

        private void DownloadsPopup_Unloaded(object sender, EventArgs e)
        {
            DownloadsBtn.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        private void DownloadsPopup_Loaded(object sender, EventArgs e)
        {
            DownloadsBtn.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}