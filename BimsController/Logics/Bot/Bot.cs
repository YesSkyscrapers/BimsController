using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Bot
{
    public class Bot
    {
        public bool isRunning = false;

        public void SwitchBot()
        {
            Logic.Execute(logic => {
                if (logic.settings.windowSettings.isOpenSettingsWindow && !isRunning)
                    logic.settings.appSettings.CloseAppSettingsWindow();
                logic.settings.windowSettings.isLockSettingsWindow = !isRunning;
            });
            isRunning = !isRunning;
        }
    }
}
