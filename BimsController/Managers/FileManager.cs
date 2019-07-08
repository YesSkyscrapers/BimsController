using BimsController.Defines;
using BimsController.Logics.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Managers
{
    public class FileManager
    {
        private static FileManager _instance;

        public static FileManager getInstance()
        {
            if (_instance == null)
                _instance = new FileManager();

            return _instance;
        }

        private string APP_SETTINGS_FILE = "settings.json";

        public AppSettings LoadAppSettings()
        {
            try
            {
                return JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(APP_SETTINGS_FILE, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                throw Errors.FailedLoadAppSettings;
            }
        }

        public void SaveAppSettings(AppSettings appSettings)
        {
            try
            {
                File.WriteAllText(APP_SETTINGS_FILE, JsonConvert.SerializeObject(appSettings), Encoding.UTF8);
                JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(APP_SETTINGS_FILE));
            }
            catch (Exception ex)
            {
                throw Errors.FailedSaveAppSettings;
            }
        }
    }
}
