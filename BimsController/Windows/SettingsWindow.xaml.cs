using BimsController.Logics;
using BimsController.Logics.Settings;
using Newtonsoft.Json;
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
            CharacterNameTextBox.Text = _appSettings.profilesSettings[_selectedProfileForEdit].characterName;
            EnteredWorldKeys.Text = _appSettings.profilesSettings[_selectedProfileForEdit].keysToPressAfterEnteringToWorld;
            RestartServerTimeTextBox.Text = _appSettings.profilesSettings[_selectedProfileForEdit].serverRestartTime.ToString("HH:mm");
            RestartServerTimeTextBox.IsEnabled = _appSettings.profilesSettings[_selectedProfileForEdit].avoidServerRestart;
            AvoidRestartServerCheckBox.IsChecked = _appSettings.profilesSettings[_selectedProfileForEdit].avoidServerRestart;

            BimsbotProfilePathTextBox.IsEnabled = profileEnabled;
            BimsbotProfilePathButton.IsEnabled = profileEnabled;
            WowPathTextBox.IsEnabled = profileEnabled;
            WowPathButton.IsEnabled = profileEnabled;
            AutoreconnectCheckBox.IsEnabled = profileEnabled;
            LoginTextBox.IsEnabled = profileEnabled && autoreconnectEnabled;
            PasswordTextBox.IsEnabled = profileEnabled && autoreconnectEnabled;
            CharacterNameTextBox.IsEnabled = profileEnabled && autoreconnectEnabled;

            UsingTrialCheckBox.IsChecked = _appSettings.generalSettings.usingTrial;
            CheckStatusDelayTextBox.Text = _appSettings.generalSettings.checkStatusDelay.ToString();
            OpeningWowDelayTextBox.Text = _appSettings.generalSettings.openingWowDelay.ToString();
            EnteringToWorldDelayTextBox.Text = _appSettings.generalSettings.enteringToWorldDelay.ToString();
            AutoReconnectDelayTextBox.Text = _appSettings.generalSettings.reconnectDelay.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic => {

                logic.settings.appSettings = _appSettings;

                if (logic.settings.appSettings.SaveAppSettings())
                    logic.settings.appSettings.CloseAppSettingsWindow();
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
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

        private void CharacterNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.profilesSettings[_selectedProfileForEdit].characterName = CharacterNameTextBox.Text;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Logic.Execute(logic =>
            {
                _appSettings = logic.settings.appSettings.Clone();

                UpdateSettingsWindow();
            });
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Logic.Execute(logic => {
                logic.settings.appSettings.CloseAppSettingsWindow(true);
            });
        }

        private void NumberAndPositiveValidation(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text) && int.Parse(e.Text) > 0;
            }catch(Exception ex)
            {
                e.Handled = false;
            }
        }

        private void CheckStatusDelayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.generalSettings.checkStatusDelay = int.Parse(CheckStatusDelayTextBox.Text);
        }

        private void OpeningWowDelayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.generalSettings.openingWowDelay = int.Parse(OpeningWowDelayTextBox.Text);
        }

        private void EnteringToWorldDelayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.generalSettings.enteringToWorldDelay = int.Parse(EnteringToWorldDelayTextBox.Text);
        }

        private void EnteredWorldKeys_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.profilesSettings[_selectedProfileForEdit].keysToPressAfterEnteringToWorld = EnteredWorldKeys.Text;
        }

        private void AutoReconnectDelayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
                _appSettings.generalSettings.reconnectDelay = int.Parse(AutoReconnectDelayTextBox.Text);
        }

        private void AvoidRestartServerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            
            if (_appSettings != null)
            {
                _appSettings.profilesSettings[_selectedProfileForEdit].avoidServerRestart = (bool)AvoidRestartServerCheckBox.IsChecked;
                UpdateSettingsWindow();
            }
        }

        private void RestartServerTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_appSettings != null)
            {

                try
                {
                    _appSettings.profilesSettings[_selectedProfileForEdit].serverRestartTime = DateTime.Parse(RestartServerTimeTextBox.Text);
                    RestartServerTimeTextBox.Foreground = Brushes.Black;
                }
                catch(Exception ex)
                {
                    RestartServerTimeTextBox.Foreground = Brushes.Red;
                }
            }
        }
    }
}
