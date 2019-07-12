using BimsController.Defines;
using BimsController.Managers;
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
        private List<int> _workingSessions = new List<int>();
        List<Lock> _interruptingLocks = new List<Lock>();

        public bool IsRunning()
        {
            return !Infos.Select(info => info.State).All(state => state.Equals(ProcessStates.Stopped));
        }

        public void SwitchBot()
        {
            if (IsRunning())
            {
                StopBot();
            } 
            else
            {
                StartBot();
            }
        }

        public async void StartBot(int id = -1)
        {
            if (LocksManager.getInstance().CheckLock(LocksManager.BotStartLock))
                return;

            LocksManager.getInstance().Lock(LocksManager.BotStartLock);

            _workingSessions.AddRange((new List<int>() { 0, 1, 2 }).Where(_id => id == _id || id == -1).Where(_id => !_workingSessions.Contains(_id)));

            Logic.Execute(logic => {
                if (logic.settings.windowSettings.isOpenSettingsWindow)
                    logic.settings.appSettings.CloseAppSettingsWindow();
                LocksManager.getInstance().Lock(LocksManager.SettingsWindowLock);
                _workingSessions.Select(_id => Infos[_id]).Where(info => info.State.Equals(ProcessStates.Stopped)).ToList().ForEach(info => info.SetState(ProcessStates.WaitingToStart));
            });

            await Task.WhenAll(_workingSessions.Select(_id => OpenAutoReconnectWindow(_id)).ToArray());

            Logic.Execute(logic => LocksManager.getInstance().Unlock(LocksManager.BotStartLock));
        }

        public async void StopBot(int id = -1)
        {
            List<int> stoppingIds = (new List<int>() { 0, 1, 2 }).Where(_id => id == _id || id == -1).ToList();
            List<int> workingIdsNeedsStopping = _workingSessions.Intersect(stoppingIds).ToList();
            _workingSessions = _workingSessions.Except(workingIdsNeedsStopping).ToList();
            List<Lock> interruptingLocks = new List<Lock>();
            workingIdsNeedsStopping.ForEach(_id =>
            {
                Lock _lock = null;
                if (Infos[_id].State.Equals(ProcessStates.WaitingToStart))
                {
                    _lock = LocksManager.getInstance().Lock(LocksManager.InterruptingWaitingToStart, _id);
                }
                else if (Infos[_id].State.Equals(ProcessStates.OpeningAutoReconnectWindow))
                {
                    _lock = LocksManager.getInstance().Lock(LocksManager.InterruptingOpeningAutoReconnectWindow, _id);
                }
                else
                {
                    Infos[_id].SetState(ProcessStates.Stopped);
                }
                if (_lock != null)
                    interruptingLocks.Add(_lock);
            });

            while (interruptingLocks.Select(__lock => __lock.isLocked).Any(isLocked => isLocked))
            {
                await Task.Delay(100);
            }

            if (Infos.Select(info => info.State).All(state => state.Equals(ProcessStates.Stopped)))
                Logic.Execute(logic => {
                    LocksManager.getInstance().Unlock(LocksManager.SettingsWindowLock);
                });
        }

        private bool CheckInterruptingLock(string key, int sessionId = 3)
        {
            if (LocksManager.getInstance().CheckLock(key, sessionId))
            {
                LocksManager.getInstance().Unlock(key, sessionId);

                return true;
            }
            return false;
        }

        public async Task OpenAutoReconnectWindow(int sessionId)
        {
            ProcessInfo currentInfo = Infos[sessionId];

            if (!currentInfo.State.Equals(ProcessStates.WaitingToStart))
                return;

            while (LocksManager.getInstance().CheckLock(LocksManager.OpeningAutoReconnectWindow))
            {
                await Task.Delay(500);
            }

            if (CheckInterruptingLock(LocksManager.InterruptingWaitingToStart, sessionId))
            {
                Infos[sessionId].SetState(ProcessStates.Stopped);

                return;
            }

            LocksManager.getInstance().Lock(LocksManager.OpeningAutoReconnectWindow);
            currentInfo.SetState(ProcessStates.OpeningAutoReconnectWindow);

            await Task.Delay(1000);

            if (CheckInterruptingLock(LocksManager.InterruptingOpeningAutoReconnectWindow, sessionId))
            {
                LocksManager.getInstance().Unlock(LocksManager.OpeningAutoReconnectWindow);
                Infos[sessionId].SetState(ProcessStates.Stopped);

                return;
            }

            currentInfo.SetState(ProcessStates.FillingAutoReconnectCaptcha);
            LocksManager.getInstance().Unlock(LocksManager.OpeningAutoReconnectWindow);
            return;
        }
    }
}
