using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Managers
{
    public class Lock
    {
        public bool isLocked = false;
    }

    public class LocksManager
    {
        private static LocksManager _instance;

        public static string BotStartLock = "BotStartLock";
        public static string OpeningAutoReconnectChecking = "OpeningAutoReconnectChecking";
        public static string OpeningWowClient = "OpeningWowClient";
        public static string OpeningBimsbot = "OpeningBimsbot"; 

        public static string InterruptingWaitingToStart = "InterruptingWaitingToStart";
        public static string InterruptingOpeningAutoReconnectChecking = "InterruptingOpeningAutoReconnectChecking";
        public static string InterruptingFillingAutoReconnectCaptcha = "InterruptingFillingAutoReconnectCaptcha";
        public static string InterruptingAutoReconnectLooping = "InterruptingAutoReconnectLooping";
        public static string InterruptingOpeningWowClient = "InterruptingOpeningWowClient";
        public static string InterruptingRunning = "InterruptingRunning";
        public static string InterruptingAutoReconnect = "InterruptingAutoReconnect";

        public static string SettingsWindowLock = "SettingsWindowLock";

        public static string AutoReconnectLooping = "AutoReconnectLooping";
        public static string WowClientLooping = "WowClientLooping";
        public static string MainBotStartingProcess = "MainBotStartingProcess";


        public static LocksManager getInstance()
        {
            if (_instance == null)
                _instance = new LocksManager();

            return _instance;
        }

        private List<Dictionary<string, Lock>> _storage = new List<Dictionary<string, Lock>>() { new Dictionary<string, Lock>(), new Dictionary<string, Lock>(), new Dictionary<string, Lock>(), new Dictionary<string, Lock>() };

        public Lock Lock(string key, int sessionId = 3)
        {
            Lock _lock;

            if (_storage[sessionId].TryGetValue(key, out _lock))
            {
                _lock.isLocked = true;
            }
            else
            {
                _lock = new Lock();
                _lock.isLocked = true;
                _storage[sessionId][key] = _lock;
            }

            return _lock;
        }

        public void Unlock(Lock _lock)
        {
            _lock.isLocked = false;
        }

        public void Unlock(string key, int sessionId = 3)
        {
            _storage[sessionId][key].isLocked = false;
        }

        public bool CheckLock(Lock _lock)
        {
            return _lock.isLocked;
        }

        public bool CheckLock(string key, int sessionId = 3)
        {
            Lock _lock;
            if (_storage[sessionId].TryGetValue(key, out _lock))
            {
                return _lock.isLocked;
            }
            else
            {
                _lock = new Lock();
                _storage[sessionId][key] = _lock;
                return _lock.isLocked;
            }
        }
    }
}
