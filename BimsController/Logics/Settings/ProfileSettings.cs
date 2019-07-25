using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Settings
{
    public class ProfileSettings
    {
        public bool enabled = false;
        public string profilePath = null;
        public string wowPath = null;
        public bool autoReconnect = false;
        public string login = null;
        public string password = null;
        public string characterName = null;
        public string keysToPressAfterEnteringToWorld = null;
        public bool avoidServerRestart = false;
        public DateTime serverRestartTime = DateTime.Parse("7:00");
        public bool useWowLowSettings = true;
    }
}
