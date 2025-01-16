﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using static launcher.ControlReferences;

namespace launcher
{
    /// <summary>
    /// Interaction logic for SettingsPopup.xaml
    /// </summary>
    public partial class SettingsPopup : UserControl
    {
        public Button Repair_Button = null;

        public SettingsPopup()
        {
            InitializeComponent();
        }

        private void btnRepair_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => GameRepair.Start());
        }

        private void AdvancedOptions_Click(object sender, RoutedEventArgs e)
        {
            if (!AppState.InAdvancedMenu)
            {
                GameSettings_Popup.IsOpen = false;
                Utilities.ShowAdvancedControl();
            }
        }
    }
}