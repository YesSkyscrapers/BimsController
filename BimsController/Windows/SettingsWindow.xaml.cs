﻿using BimsController.Logics;
using BimsController.Logics.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BimsController.Windows
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private AppSettings _appSettings;
        private int _selectedProfileForEdit = 0;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void UpdateSettingsWindow()
        {
            SelectProfileComboBox.SelectedIndex = _selectedProfileForEdit;

            bool profileEnabled = _appSettings.profilesSettings[_selectedProfileForEdit].enabled;
            bool autoreconnectEnabled = _appSettings.profilesSettings[_selectedProfileForEdit].autoReconnect;

            ProfileEnabledCheckBox.IsChecked = profileEnabled;
            BimsbotProfilePathTextBox.Text = _appSettings.profilesSettings[_selectedProfileForEdit].profilePath;
            WowPathTextBox.Text = _appSettings.profilesSettings[_selectedProfileForEdit].wowPath;
            AutoreconnectCheckBox.IsChecked = _appSettings.profilesSettings[_selectedProfileForEdit].autoReconnect;
            LoginTextBox.Text = _appSettings.profilesSettings[_selectedProfileForEdit].login;
            PasswordTextBox.Text = _appSettings.profilesSettings[_selectedProfileForEdit].password;
            CharacterIdTextBox.Text = _appSettings.profilesSettings[_selectedProfileForEdit].characterId.ToString();
            CharacterNameTextBox.Text = _appSettings.profilesSettings[_selectedProfileForEdit].characterName;

            BimsbotProfilePathTextBox.IsEnabled = profileEnabled;
            BimsbotProfilePathButton.IsEnabled = profileEnabled;
            WowPathTextBox.IsEnabled = profileEnabled;
            WowPathButton.IsEnabled = profileEnabled;
            AutoreconnectCheckBox.IsEnabled = profileEnabled;
            LoginTextBox.IsEnabled = profileEnabled && autoreconnectEnabled;
            PasswordTextBox.IsEnabled = profileEnabled && autoreconnectEnabled;
            CharacterIdTextBox.IsEnabled = profileEnabled && autoreconnectEnabled;
            CharacterNameTextBox.IsEnabled = profileEnabled && autoreconnectEnabled;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccessful = false;

            Logic.Execute(logic => {

                logic.settings.appSettings = _appSettings;

                isSuccessful = logic.settings.appSettings.SaveAppSettings();
            });
            if (isSuccessful)
                this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Logic.Execute(logic => {
                logic.settings.appSettings.CloseAppSettingsWindow();
            });
        }

        private void BimsbotProfilePathButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".bimsp";
            dlg.Filter = "BIMSP Files (*.bimsp)|*.bimsp";

            if (dlg.ShowDialog() == true)
            {
                _appSettings.profilesSettings[_selectedProfileForEdit].profilePath = dlg.FileName;
                BimsbotProfilePathTextBox.Text = dlg.FileName;
            }
        }

        private void WowPathButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".exe";
            dlg.Filter = "WoW Files (*wow.exe)|*wow.exe";

            if (dlg.ShowDialog() == true)
            {
                _appSettings.profilesSettings[_selectedProfileForEdit].wowPath = dlg.FileName;
                WowPathTextBox.Text = dlg.FileName;
            }
        }

        private void UsingTrialCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.generalSettings.usingTrial = (bool)UsingTrialCheckBox.IsChecked;
        }

        private void SelectProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProfileForEdit = SelectProfileComboBox.SelectedIndex;
            if (_appSettings != null)
                UpdateSettingsWindow();
        }

        private void ProfileEnabledCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.profilesSettings[_selectedProfileForEdit].enabled = (bool)ProfileEnabledCheckBox.IsChecked;
                UpdateSettingsWindow();
            }
        }

        private void AutoreconnectCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_appSettings != null)
            {
                _appSettings.profilesSettings[_selectedProfileForEdit].autoReconnect = (bool)AutoreconnectCheckBox.IsChecked;
                UpdateSettingsWindow();
            }
        }

        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.profilesSettings[_selectedProfileForEdit].login = LoginTextBox.Text;
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.profilesSettings[_selectedProfileForEdit].password = PasswordTextBox.Text;
        }

        private void CharacterIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.profilesSettings[_selectedProfileForEdit].characterId = int.Parse(CharacterIdTextBox.Text);
        }

        private void CharacterNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.profilesSettings[_selectedProfileForEdit].characterName = CharacterNameTextBox.Text;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic =>
            {
                _appSettings = logic.settings.appSettings;

                UpdateSettingsWindow();
            });

            UsingTrialCheckBox.IsChecked = _appSettings.generalSettings.usingTrial;
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
