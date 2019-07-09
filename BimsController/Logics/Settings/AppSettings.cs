using BimsController.Defines;
using BimsController.Managers;
using BimsController.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BimsController.Logics.Settings
{
    public class AppSettings
    {
        private SettingsWindow settingsWindow;

        public GeneralSettings generalSettings = new GeneralSettings();
        public ProfileSettings[] profilesSettings = new ProfileSettings[] { new ProfileSettings(), new ProfileSettings(), new ProfileSettings() };


        public void LoadAppSettings()
        {
            AppSettings loadedSettings;

            try
            {
                loadedSettings = FileManager.getInstance().LoadAppSettings();
            }catch(Error error)
            {
                MessageBox.Show("Failed to load previous settings. Using default options.");
                loadedSettings = new AppSettings();
            }

            generalSettings = loadedSettings.generalSettings;
            profilesSettings = loadedSettings.profilesSettings;
        }

        public bool SaveAppSettings()
        {
            try
            {
                FileManager.getInstance().SaveAppSettings(this);
            }
            catch (Error error)
            {
                MessageBox.Show("Failed to save settings.");
                return false;
            }

            return true;
        }

        public void OpenAppSettingsWindow()
        {
            if (settingsWindow != null)
            {
                settingsWindow.Activate();
            }
            else
            {
                settingsWindow = new SettingsWindow();
                settingsWindow.Show();
                Logic.Execute(logic => logic.settings.windowSettings.isOpenSettingsWindow = true, true);
            }
        }

        public void CloseAppSettingsWindow(bool skipClosing = false)
        {
            if (!skipClosing)
                settingsWindow.Close();
            settingsWindow = null;
            Logic.Execute(logic => logic.settings.windowSettings.isOpenSettingsWindow = false, true);
        }

        public AppSettings Clone()
        {
            AppSettings clone = JsonConvert.DeserializeObject<AppSettings>(JsonConvert.SerializeObject(this));
            clone.settingsWindow = this.settingsWindow;
            return clone;
        }
    }
}
