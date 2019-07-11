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
        public bool isRunning = false;
        public ProcessState[] States = new ProcessState[] { ProcessStates.Stopped, ProcessStates.Stopped, ProcessStates.Stopped };

        public void SwitchBot()
        {
            if (isRunning)
                Stop();
            else
                Start();
            isRunning = !isRunning;
        }

        public async void Start(int id = -1)
        {
            await Logic.ExecuteAsync(async logic => {
                if (logic.settings.windowSettings.isOpenSettingsWindow)
                    logic.settings.appSettings.CloseAppSettingsWindow();
                logic.settings.windowSettings.isLockSettingsWindow = true;

                for (int i = 0; i < 3; i++)
                {
                    if (logic.settings.appSettings.profilesSettings[i].enabled && (id == i || id == -1))
                        StartSession(i);
                }
            });
        }

        private async Task StartSession(int sessionId)
        {
            SetState(sessionId, ProcessStates.WaitingRunning);
            await Logic.ExecuteAsync(async (logic) =>
            {
                await Task.Delay(1000 * sessionId);
                if (States[sessionId].Equals(ProcessStates.Canceled))
                    return;
                SetState(sessionId, ProcessStates.Running);
            });
        }

        private async Task StopSession(int sessionId)
        {
            SetState(sessionId, ProcessStates.Canceled);
            await Logic.ExecuteAsync(async (logic) =>
            {
                await Task.Delay(1000 * sessionId);
                SetState(sessionId, ProcessStates.Stopped);
            });
        }

        public void SetState(int sessionId, ProcessState state)
        {
            States[sessionId] = state;
        }

        public async void Stop(int id = -1)
        {
            Logic.Execute(logic => logic.settings.windowSettings.isStartButtonLocked = true);
            Logic.ExecuteAsync(async logic =>
            {
                //antiplaying delay - idk why...just safety
                await Task.Delay(3000);
                logic.settings.windowSettings.isStartButtonLocked = false;
            });
            await Logic.ExecuteAsync(async logic => {
                
                await Task.WhenAll((new List<int>() { 0, 1, 2 }).Where(i => !States[i].Equals(ProcessStates.Stopped) && (id == i || id == -1)).Select(i => 
                StopSession(i)).ToArray());

                //Check for locking or unlocking settings window
                bool isAllProcessesStopped = true;

                for (int i = 0; i < 3; i++)
                {
                    if (i == id)
                        continue;
                    if (!States[i].Equals(ProcessStates.Stopped))
                        isAllProcessesStopped = false;
                }

                if (isAllProcessesStopped)
                    logic.settings.windowSettings.isLockSettingsWindow = false;
            });
        }
    }
}
