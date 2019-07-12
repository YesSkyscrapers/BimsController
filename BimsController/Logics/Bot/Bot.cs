using BimsController.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Bot
{
    public class Bot
    {
        public ProcessInfo[] Infos = new ProcessInfo[] { new ProcessInfo(), new ProcessInfo(), new ProcessInfo() };

        public bool IsRunning()
        {
            return Infos.Select(info => info.State).Where(state => state.Equals(ProcessStates.Stopped)).Count() < 3;
        }

        public void SwitchBot()
        {
            /*Logic.Execute(logic => {
                if (logic.settings.windowSettings.isOpenSettingsWindow && !isRunning)
                    logic.settings.appSettings.CloseAppSettingsWindow();
                logic.settings.windowSettings.isLockSettingsWindow = !isRunning;
            });
            isRunning = !isRunning;*/
        }
    }
}
