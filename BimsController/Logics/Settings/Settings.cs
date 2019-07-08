using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Settings
{
    public class Settings
    {
        public WindowSettings windowSettings;
        public AppSettings appSettings;

        public Settings()
        {
            windowSettings = new WindowSettings();
            appSettings = new AppSettings();
            appSettings.LoadAppSettings();
        }
    }
}
