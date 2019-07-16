using BimsController.Defines;
using BimsController.Managers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Bot
{
    public class Bot
    {
        private string AUTORECONNECT_AUTH_URL = "https://cp.uwow.biz/auth/login";
        private string AUTORECONNECT_CHARACTERS_LIST_URL = "https://cp.uwow.biz/character/correct";

        public ProcessInfo[] Infos = new ProcessInfo[] { new ProcessInfo(), new ProcessInfo(), new ProcessInfo() };
        private List<int> _workingSessions = new List<int>();
        //List<Lock> _interruptingLocks = new List<Lock>();

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
                _workingSessions.Where(_id => logic.settings.appSettings.profilesSettings[_id].enabled).Where(_id => Infos[_id].State.Equals(ProcessStates.Stopped)).ToList().ForEach(_id => {
                    Infos[_id].AutoReconnectEnabled = logic.settings.appSettings.profilesSettings[_id].autoReconnect;
                    Infos[_id].SetState(ProcessStates.WaitingToStart);
                });
            });

            await (Task.Delay(50));

            await Task.WhenAll(_workingSessions.Select(_id => OpenAutoReconnectChecking(_id)).ToArray());

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
                else if (Infos[_id].State.Equals(ProcessStates.OpeningAutoReconnectChecking))
                {
                    _lock = LocksManager.getInstance().Lock(LocksManager.InterruptingOpeningAutoReconnectChecking, _id);
                }
                else if (Infos[_id].State.Equals(ProcessStates.FillingAutoReconnectCaptcha))
                {
                    _lock = LocksManager.getInstance().Lock(LocksManager.InterruptingFillingAutoReconnectCaptcha, _id);
                }
                else if (Infos[_id].State.Equals(ProcessStates.WaitingOtherAutoReconnectProcesses))
                {
                    if (Infos[_id].AutoReconnectEnabled)
                        _lock = LocksManager.getInstance().Lock(LocksManager.InterruptingAutoReconnectLooping, _id);
                    else
                        Infos[_id].SetState(ProcessStates.Stopped);
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

            workingIdsNeedsStopping.ForEach(_id =>
            {
                //additional safe
                if (Infos[_id].WebDriver != null)
                    Infos[_id].CloseWebDriver();
            });

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

        private void UpdateCharacterStatus(int sessionId, bool status)
        {
            if (Infos[sessionId].CharacterStatus != status)
            {
                Infos[sessionId].CharacterStatus = status;

                if (status)
                {
                    //enter to game
                }
                else
                {
                    //disconnect
                }
            }
        }

        private async void AutoReconnectLooping(int sessionId)
        {
            int checkStatusDelay = 5000;
            int CharacterId = 0;

            Logic.Execute(logic =>
            {
                checkStatusDelay = logic.settings.appSettings.generalSettings.checkStatusDelay;
                CharacterId = logic.settings.appSettings.profilesSettings[sessionId].characterId;
            }, true);

            while (!LocksManager.getInstance().CheckLock(LocksManager.InterruptingAutoReconnectLooping, sessionId))
            {
                Infos[sessionId].WebDriver.Navigate().GoToUrl(AUTORECONNECT_CHARACTERS_LIST_URL);

                await Task.Delay(1000);

                SelectElement selectBox = new SelectElement(Infos[sessionId].WebDriver.FindElement(By.Name("realm")));
                selectBox.SelectByValue("67");

                await Task.Delay(1000);

                IWebElement element = Infos[sessionId].WebDriver.FindElement(By.Id(CharacterId.ToString()));
                ((IJavaScriptExecutor)Infos[sessionId].WebDriver).ExecuteScript("arguments[0].scrollIntoView(true);", element);

                await Task.Delay(1000);

                string characterStatusLabelText = Infos[sessionId].WebDriver.FindElement(By.XPath("//*[@id='" + CharacterId.ToString() + "']/td[6]")).Text;


                bool status = characterStatusLabelText.Equals("Онлайн");
                UpdateCharacterStatus(sessionId, status);

                await Task.Delay(checkStatusDelay);
            }

            LocksManager.getInstance().Unlock(LocksManager.InterruptingAutoReconnectLooping, sessionId);
            Infos[sessionId].SetState(ProcessStates.Stopped);
        }

        public async Task OpenAutoReconnectChecking(int sessionId)
        {
            ProcessInfo currentInfo = Infos[sessionId];

            if (!currentInfo.State.Equals(ProcessStates.WaitingToStart))
                return;

            if (!Infos[sessionId].AutoReconnectEnabled)
            {
                await Infos[sessionId].SetState(ProcessStates.WaitingOtherAutoReconnectProcesses, 50);
                return;
            }

            while (LocksManager.getInstance().CheckLock(LocksManager.OpeningAutoReconnectChecking))
            {
                await Task.Delay(500);
            }

            if (CheckInterruptingLock(LocksManager.InterruptingWaitingToStart, sessionId))
            {
                Infos[sessionId].SetState(ProcessStates.Stopped);

                return;
            }

            LocksManager.getInstance().Lock(LocksManager.OpeningAutoReconnectChecking);
            await currentInfo.SetState(ProcessStates.OpeningAutoReconnectChecking, 50);

            //Init webdriver
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            Infos[sessionId].WebDriver = new ChromeDriver(service);
            Infos[sessionId].WebDriver.Navigate().GoToUrl(AUTORECONNECT_AUTH_URL);

            await Task.Delay(200);

            Logic.Execute(logic => {
                Infos[sessionId].WebDriver.FindElement(By.Name("username")).SendKeys(logic.settings.appSettings.profilesSettings[sessionId].login);
                Infos[sessionId].WebDriver.FindElement(By.Name("password")).SendKeys(logic.settings.appSettings.profilesSettings[sessionId].password);
                Infos[sessionId].WebDriver.FindElement(By.ClassName("lbl")).Click();
                Infos[sessionId].WebDriver.FindElement(By.Name("captcha")).Click();
            }, true);
            
            if (CheckInterruptingLock(LocksManager.InterruptingOpeningAutoReconnectChecking, sessionId))
            {
                Infos[sessionId].CloseWebDriver();
                LocksManager.getInstance().Unlock(LocksManager.OpeningAutoReconnectChecking);
                Infos[sessionId].SetState(ProcessStates.Stopped);

                return;
            }

            LocksManager.getInstance().Unlock(LocksManager.OpeningAutoReconnectChecking);
            await currentInfo.SetState(ProcessStates.FillingAutoReconnectCaptcha, 50);

            while (Infos[sessionId].WebDriver.Url == AUTORECONNECT_AUTH_URL && !LocksManager.getInstance().CheckLock(LocksManager.InterruptingFillingAutoReconnectCaptcha, sessionId))
            {
                await Task.Delay(500);
            }

            if (CheckInterruptingLock(LocksManager.InterruptingFillingAutoReconnectCaptcha, sessionId))
            {
                Infos[sessionId].CloseWebDriver();
                Infos[sessionId].SetState(ProcessStates.Stopped);

                return;
            }

            Infos[sessionId].WebDriver.Manage().Window.Position = new System.Drawing.Point(-32000, -32000);

            AutoReconnectLooping(sessionId);
            await currentInfo.SetState(ProcessStates.WaitingOtherAutoReconnectProcesses, 50);

            return;
        }
    }
}
