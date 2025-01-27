﻿using launcher.BranchUtils;
using System.Windows;
using System.Windows.Controls;
using launcher.Game;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using Hardcodet.Wpf.TaskbarNotification;
using static launcher.Global.References;
using launcher.Utilities;
using launcher.Managers;

namespace launcher
{
    public partial class AskToQuitPopup : UserControl
    {
        public AskToQuitPopup()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Ini.Set(Ini.Vars.Enable_Quit_On_Close, "quit");
            Application.Current.Shutdown();
        }

        private void Tray_Click(object sender, RoutedEventArgs e)
        {
            Ini.Set(Ini.Vars.Enable_Quit_On_Close, "tray");
            AppManager.HideAskToQuit();
            AppManager.SendNotification("Launcher minimized to tray.", BalloonIcon.Info);
            Main_Window.OnClose();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            AppManager.HideAskToQuit();
        }
    }
}