using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Settings
{
    public class GeneralSettings
    {
        public bool usingTrial = true;
        public int checkStatusDelay = 5000;
        public int openingWowDelay = 20000;
        public int enteringToWorldDelay = 120000;
        public int reconnectDelay = 15*60*1000; 
    }
}
