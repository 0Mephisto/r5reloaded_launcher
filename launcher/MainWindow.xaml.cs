﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
            Utilities.SetupApp(this);
            UpdateChecker updateChecker = new UpdateChecker(Dispatcher);
            btnPlay.Content = Global.isInstalled ? "PLAY" : "INSTALL";
            if (Global.isInstalled)
            {
                cmbBranch.SelectedItem = Global.serverConfig.branches.FirstOrDefault(b => b.branch == Global.launcherConfig.currentUpdateBranch);
                Task.Run(() => updateChecker.Start());
            }
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
            if (Global.isInstalled)
            {
                if (Global.updateRequired)
                {
                    Task.Run(() => ControlReferences.gameUpdate.Start());
                }
                else
                {
                    Utilities.LaunchGame();
                }
            }
            else if (!Global.isInstalling)
            {
                Task.Run(() => ControlReferences.gameInstall.Start());
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
            if (sender is not ComboBox comboBox) return;

            var selectedBranch = comboBox.SelectedIndex;

            if (!Global.serverConfig.branches[selectedBranch].enabled)
            {
                comboBox.SelectedIndex = lastSelectedIndex;
                return;
            }

            lastSelectedIndex = selectedBranch;

            if (Global.isInstalled)
            {
                if (Global.serverConfig.branches[0].branch == Global.launcherConfig.currentUpdateBranch)
                {
                    Global.updateRequired = false;
                    btnPlay.Content = "Play";
                }
                else
                {
                    btnPlay.Content = "Update";
                    Global.updateRequired = true;
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
    }
}