using BimsController.Defines;
using BimsController.Logics;
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

        public void UpdateWowSettings(string exePath, int sessionId)
        {
            string configPath = exePath.Substring(0, exePath.LastIndexOf("\\")) + "\\WTF\\UWow.wtf";
            string configBackupPath = exePath.Substring(0, exePath.LastIndexOf("\\")) + "\\WTF\\UWow_.wtf";
            try
            {
                if (!File.Exists(configBackupPath))
                    File.Move(configPath, configBackupPath);
            }
            catch (Exception ex)
            {
                Logic.Execute(logic => logic.logs.Log(sessionId, string.Format("Failed to save previous wow settings ({0})", ex.Message)));
                return;
            };
            try
            {
                if (File.Exists(configPath))
                    File.Delete(configPath);
                File.Copy("UWow.wtf", configPath);
            }
            catch (Exception ex)
            {
                Logic.Execute(logic => logic.logs.Log(sessionId, string.Format("Failed to update wow settings ({0})", ex.Message)));
                File.Move(configBackupPath, configPath);
            }
        }

        public void ReturnWowSettings(string exePath, int sessionId)
        {
            string configPath = exePath.Substring(0, exePath.LastIndexOf("\\")) + "\\WTF\\UWow.wtf";
            string configBackupPath = exePath.Substring(0, exePath.LastIndexOf("\\")) + "\\WTF\\UWow_.wtf";
            try
            {
                if (!File.Exists(configBackupPath))
                    return;

                File.Delete(configPath);
                File.Copy(configBackupPath, configPath);
            }
            catch (Exception ex) {
                Logic.Execute(logic => logic.logs.Log(sessionId, string.Format("Failed to return previous wow settings ({0})", ex.Message)));
            };
            
        }
    }
}
