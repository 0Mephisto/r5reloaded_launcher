﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace launcher
{
    /// <summary>
    /// Interaction logic for AccessibilitySettings.xaml
    /// </summary>
    public partial class AccessibilitySettings : UserControl
    {
        public AccessibilitySettings()
        {
            InitializeComponent();
        }

        public void SetupAccessibilitySettings()
        {
            // Set the initial state of the toggle switches
            DisableTransitionsBtn.IsChecked = Ini.Get(Ini.Vars.Disable_Transitions, false);
            DisableAnimationsBtn.IsChecked = Ini.Get(Ini.Vars.Disable_Animations, false);
            DisableBackgroundVideoBtn.IsChecked = Ini.Get(Ini.Vars.Disable_Background_Video, false);
        }

        private void DisableBackgroundVideoBtn_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool value = DisableBackgroundVideoBtn.IsChecked.Value;
            Ini.Set(Ini.Vars.Disable_Background_Video, value);
            Utilities.ToggleBackgroundVideo(DisableBackgroundVideoBtn.IsChecked.Value);
        }

        private void DisableAnimationsBtn_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool value = DisableAnimationsBtn.IsChecked.Value;
            Ini.Set(Ini.Vars.Disable_Animations, value);
        }

        private void DisableTransitionsBtn_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool value = DisableTransitionsBtn.IsChecked.Value;
            Ini.Set(Ini.Vars.Disable_Transitions, value);
        }
    }
}