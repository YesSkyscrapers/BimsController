﻿using System;
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
        public int characterId = -1;
        public string characterName = null;
    }
}
