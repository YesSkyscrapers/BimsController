using BimsController.Defines;
using BimsController.Managers;
using BimsController.Tools;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            _workingSessions.ForEach(_id =>
                LocksManager.getInstance().Lock(LocksManager.MainBotStartingProcess, _id));

            var array = _workingSessions.Select(_id => OpenAutoReconnectChecking(_id)).ToArray();
            if (array.Length > 0)
                await Task.WhenAll(array);

            array = _workingSessions.Select(_id => OpenWowSessionTask(_id)).ToArray();
            if (array.Length > 0)
                await Task.WhenAll(array);

            //await Task.WhenAll(_workingSessions.Select(_id => OpenBimsbotSessionTask(_id)).ToArray());
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
                List<Lock> _locks = new List<Lock>();

                if (Infos[_id].State.Equals(ProcessStates.WaitingToStart))
                {
                    _locks.Add(LocksManager.getInstance().Lock(LocksManager.InterruptingWaitingToStart, _id));
                }
                else if (Infos[_id].State.Equals(ProcessStates.OpeningAutoReconnectChecking))
                {
                    _locks.Add(LocksManager.getInstance().Lock(LocksManager.InterruptingOpeningAutoReconnectChecking, _id));
                }
                else if (Infos[_id].State.Equals(ProcessStates.FillingAutoReconnectCaptcha))
                {
                    _locks.Add(LocksManager.getInstance().Lock(LocksManager.InterruptingFillingAutoReconnectCaptcha, _id));
                }
                else if (Infos[_id].State.Equals(ProcessStates.WaitingOtherAutoReconnectProcesses))
                {
                    //nothing
                }
                else if (Infos[_id].State.Equals(ProcessStates.StartingWow))
                {
                    //nothing
                }
                else if (Infos[_id].State.Equals(ProcessStates.CheckingIsWowOpened))
                {
                    //nothing
                }
                else if (Infos[_id].State.Equals(ProcessStates.EnteringCredentials))
                {
                    //nothing
                }
                else if (Infos[_id].State.Equals(ProcessStates.EnteringToWorld))
                {
                    //nothing
                }
                else if (Infos[_id].State.Equals(ProcessStates.CheckingIsEnteredToWorld))
                {
                    //nothing
                }
                else if (Infos[_id].State.Equals(ProcessStates.PreloadStringSending))
                {
                    //nothing
                }
                else if (Infos[_id].State.Equals(ProcessStates.Running))
                {
                    _locks.Add(LocksManager.getInstance().Lock(LocksManager.InterruptingRunning, _id));
                }
                else
                {
                    Infos[_id].SetState(ProcessStates.Stopped);
                }

                if (Infos[_id].State.id >= ProcessStates.WaitingOtherAutoReconnectProcesses.id && Infos[_id].AutoReconnectEnabled)
                    _locks.Add(LocksManager.getInstance().Lock(LocksManager.InterruptingAutoReconnectLooping, _id));

                if (Infos[_id].State.id >= ProcessStates.StartingWow.id && Infos[_id].State.id <= ProcessStates.PreloadStringSending.id)
                    _locks.Add(LocksManager.getInstance().Lock(LocksManager.InterruptingOpeningWowClient, _id));

                if (_locks.Count > 0)
                    interruptingLocks.AddRange(_locks);
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
            string CharacterName = null;

            LocksManager.getInstance().Lock(LocksManager.AutoReconnectLooping, sessionId);

            Logic.Execute(logic =>
            {
                checkStatusDelay = logic.settings.appSettings.generalSettings.checkStatusDelay;
                CharacterName = logic.settings.appSettings.profilesSettings[sessionId].characterName;
            }, true);

            while (!LocksManager.getInstance().CheckLock(LocksManager.InterruptingAutoReconnectLooping, sessionId))
            {
                Infos[sessionId].WebDriver.Navigate().GoToUrl(AUTORECONNECT_CHARACTERS_LIST_URL);

                await Task.Delay(1000);

                SelectElement selectBox = new SelectElement(Infos[sessionId].WebDriver.FindElement(By.Name("realm")));
                selectBox.SelectByValue("67");

                await Task.Delay(1000);

                IWebElement table = Infos[sessionId].WebDriver.FindElement(By.CssSelector("tbody.char"));
                ((IJavaScriptExecutor)Infos[sessionId].WebDriver).ExecuteScript("arguments[0].scrollIntoView();", table);

                IWebElement characterLine = table.FindElements(By.TagName("tr")).Where(_element => _element.GetAttribute("textContent").Contains(CharacterName)).FirstOrDefault();

                if (characterLine == null)
                {
                    await Infos[sessionId].SetState(ProcessStates.NotFoundCharacter, 20);

                    LocksManager.getInstance().Unlock(LocksManager.InterruptingAutoReconnectLooping, sessionId);
                    return;
                }

                bool status = characterLine.GetAttribute("textContent").Contains("Онлайн");
                UpdateCharacterStatus(sessionId, status);

                await Task.Delay(checkStatusDelay);
            }

            LocksManager.getInstance().Unlock(LocksManager.InterruptingAutoReconnectLooping, sessionId);
            LocksManager.getInstance().Unlock(LocksManager.AutoReconnectLooping, sessionId);
            await CallToStopProcess(sessionId);
        }
        
        public async Task<bool> CheckIsCharacterEntered(int processId, int sessionId)
        {
            while (LocksManager.getInstance().CheckLock(LocksManager.OpeningWowClient))
            {
                await Task.Delay(1000);

                if (LocksManager.getInstance().CheckLock(LocksManager.InterruptingOpeningWowClient, sessionId))
                {

                    return false;
                }
            }

            LocksManager.getInstance().Lock(LocksManager.OpeningWowClient);


            ProcessInfo currentInfo = Infos[sessionId];
            string profilePath = null;
            bool usingTrial = true;
            string wowPath = null;

            Logic.Execute(logic =>
            {
                profilePath = logic.settings.appSettings.profilesSettings[sessionId].profilePath;
                usingTrial = logic.settings.appSettings.generalSettings.usingTrial;
                wowPath = logic.settings.appSettings.profilesSettings[sessionId].wowPath;
            }, true);

            Process additionalWowProcess = null;
            bool usingAdditionalWowProcess = false;

            if (Infos.Where(info => info.WowProcess != null).Count() <= 1)
            {
                usingAdditionalWowProcess = true;
                additionalWowProcess = new Process();
                additionalWowProcess.StartInfo.FileName = wowPath;
                additionalWowProcess.StartInfo.Arguments = "-noautolaunch64bi﻿﻿t";
                additionalWowProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                additionalWowProcess.Start();

                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    if (LocksManager.getInstance().CheckLock(LocksManager.InterruptingOpeningWowClient, sessionId))
                    {
                        LocksManager.getInstance().Unlock(LocksManager.OpeningWowClient);
                        if (usingAdditionalWowProcess)
                        {
                            additionalWowProcess.Kill();
                            additionalWowProcess = null;
                        }

                        return false;
                    }
                }
            }

            while (LocksManager.getInstance().CheckLock(LocksManager.OpeningBimsbot))
            {
                await Task.Delay(1000);

                if (LocksManager.getInstance().CheckLock(LocksManager.InterruptingOpeningWowClient, sessionId))
                {

                    LocksManager.getInstance().Unlock(LocksManager.OpeningWowClient);
                    if (usingAdditionalWowProcess)
                    {
                        additionalWowProcess.Kill();
                        additionalWowProcess = null;
                    }

                    return false;
                }
            }

            LocksManager.getInstance().Lock(LocksManager.OpeningBimsbot);

            currentInfo.BimsbotProcess = Process.Start(profilePath);
            await Task.Delay(3000);

            if (LocksManager.getInstance().CheckLock(LocksManager.InterruptingOpeningWowClient, sessionId))
            {
                currentInfo.BimsbotProcess.Kill();
                LocksManager.getInstance().Unlock(LocksManager.OpeningWowClient);
                if (usingAdditionalWowProcess)
                {
                    additionalWowProcess.Kill();
                    additionalWowProcess = null;
                }


                return false;
            }

            if (usingTrial)
                WinApi.CloseWindow(currentInfo.BimsbotProcess.MainWindowHandle);

            await Task.Delay(3000);

            if (LocksManager.getInstance().CheckLock(LocksManager.InterruptingOpeningWowClient, sessionId))
            {
                currentInfo.BimsbotProcess.Kill();
                LocksManager.getInstance().Unlock(LocksManager.OpeningWowClient);
                if (usingAdditionalWowProcess)
                {
                    additionalWowProcess.Kill();
                    additionalWowProcess = null;
                }


                return false;
            }

            string outBimsString = WinApi.GetPidsList().Where(pid => pid.Contains(processId.ToString())).FirstOrDefault();
            bool result = outBimsString != null ? outBimsString.Contains("Не в игре") : false;

            currentInfo.BimsbotProcess.Kill();


            LocksManager.getInstance().Unlock(LocksManager.OpeningBimsbot);

            if (usingAdditionalWowProcess)
            {
                additionalWowProcess.Kill();
                additionalWowProcess = null;
                usingAdditionalWowProcess = false;
            }
            LocksManager.getInstance().Unlock(LocksManager.OpeningWowClient);

            return !result;
        }
        public async Task<bool> CheckIsClientOpened(int processId, int sessionId)
        {
            while (LocksManager.getInstance().CheckLock(LocksManager.OpeningWowClient))
            {
                await Task.Delay(1000);

                if (LocksManager.getInstance().CheckLock(LocksManager.InterruptingOpeningWowClient, sessionId))
                {

                    return false;
                }
            }

            LocksManager.getInstance().Lock(LocksManager.OpeningWowClient);

            ProcessInfo currentInfo = Infos[sessionId];
            string profilePath = null;
            bool usingTrial = true;
            string wowPath = null;

            Logic.Execute(logic =>
            {
                profilePath = logic.settings.appSettings.profilesSettings[sessionId].profilePath;
                usingTrial = logic.settings.appSettings.generalSettings.usingTrial;
                wowPath = logic.settings.appSettings.profilesSettings[sessionId].wowPath;
            }, true);


            Process additionalWowProcess = null;
            bool usingAdditionalWowProcess = false;

            if (Infos.Where(info => info.WowProcess != null).Count() <= 1)
            {
                usingAdditionalWowProcess = true;
                additionalWowProcess = new Process();
                additionalWowProcess.StartInfo.FileName = wowPath;
                additionalWowProcess.StartInfo.Arguments = "-noautolaunch64bi﻿﻿t";
                additionalWowProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                additionalWowProcess.Start();

                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    if (LocksManager.getInstance().CheckLock(LocksManager.InterruptingOpeningWowClient, sessionId))
                    {
                        LocksManager.getInstance().Unlock(LocksManager.OpeningWowClient);
                        if (usingAdditionalWowProcess)
                        {
                            additionalWowProcess.Kill();
                            additionalWowProcess = null;
                        }

                        return false;
                    }
                }
            }

            while (LocksManager.getInstance().CheckLock(LocksManager.OpeningBimsbot))
            {
                await Task.Delay(1000);

                if (LocksManager.getInstance().CheckLock(LocksManager.InterruptingOpeningWowClient, sessionId))
                {

                    LocksManager.getInstance().Unlock(LocksManager.OpeningWowClient);
                    if (usingAdditionalWowProcess)
                    {
                        additionalWowProcess.Kill();
                        additionalWowProcess = null;
                    }

                    return false;
                }
            }

            LocksManager.getInstance().Lock(LocksManager.OpeningBimsbot);

            currentInfo.BimsbotProcess = Process.Start(profilePath);
            await Task.Delay(3000);

            if (LocksManager.getInstance().CheckLock(LocksManager.InterruptingOpeningWowClient, sessionId))
            {
                currentInfo.BimsbotProcess.Kill();
                LocksManager.getInstance().Unlock(LocksManager.OpeningWowClient);
                if (usingAdditionalWowProcess)
                {
                    additionalWowProcess.Kill();
                    additionalWowProcess = null;
                }


                return false;
            }

            if (usingTrial)
                WinApi.CloseWindow(currentInfo.BimsbotProcess.MainWindowHandle);

            await Task.Delay(3000);

            if (LocksManager.getInstance().CheckLock(LocksManager.InterruptingOpeningWowClient, sessionId))
            {
                currentInfo.BimsbotProcess.Kill();
                LocksManager.getInstance().Unlock(LocksManager.OpeningWowClient);
                if (usingAdditionalWowProcess)
                {
                    additionalWowProcess.Kill();
                    additionalWowProcess = null;
                }


                return false;
            }

            bool result = WinApi.GetPidsList().Where(pid => pid.Contains(processId.ToString())).Count() > 0;

            currentInfo.BimsbotProcess.Kill();

            LocksManager.getInstance().Unlock(LocksManager.OpeningBimsbot);

            if (usingAdditionalWowProcess)
            {
                additionalWowProcess.Kill();
                additionalWowProcess = null;
                usingAdditionalWowProcess = false;
            }

            LocksManager.getInstance().Unlock(LocksManager.OpeningWowClient);

            return result;
        }

        public async Task EnterCredentials(int sessionId)
        {
            string login = null;
            string password = null;

            Logic.Execute(logic => {
                login = logic.settings.appSettings.profilesSettings[sessionId].login;
                password = logic.settings.appSettings.profilesSettings[sessionId].password;
            }, true);

            await WinApi.EnterCredentials(Infos[sessionId].WowProcess.MainWindowHandle, login, password);
        }

        public async Task OpenWowClient(int sessionId)
        {
            string wowPath = null;
            int wowOpeningDelay = 20000;
            int enteringToWorldDelay = 20000;

            ProcessInfo currentInfo = Infos[sessionId];

            Logic.Execute(logic => {
                wowPath = logic.settings.appSettings.profilesSettings[sessionId].wowPath;
                wowOpeningDelay = logic.settings.appSettings.generalSettings.openingWowDelay;
                enteringToWorldDelay = logic.settings.appSettings.generalSettings.enteringToWorldDelay;
            }, true);


            await currentInfo.SetState(ProcessStates.StartingWow, 50);

            Infos[sessionId].WowProcess = new Process();
            Infos[sessionId].WowProcess.StartInfo.FileName = wowPath;
            Infos[sessionId].WowProcess.StartInfo.Arguments = "-noautolaunch64bi﻿﻿t";
            Infos[sessionId].WowProcess.Start();


            bool requireToCheckOpenedClient = true;
            do
            {
                await currentInfo.SetState(ProcessStates.StartingWow, 50);

                for(int _i=0; _i< wowOpeningDelay/1000; _i++)
                {

                    if (CheckInterruptingLock(LocksManager.InterruptingOpeningWowClient, sessionId))
                    {
                        Infos[sessionId].CloseWowProcess();
                        LocksManager.getInstance().Unlock(LocksManager.MainBotStartingProcess, sessionId);
                        await CallToStopProcess(sessionId);

                        return;
                    }
                    await Task.Delay(1000);
                }

                await currentInfo.SetState(ProcessStates.CheckingIsWowOpened, 50);

                

                requireToCheckOpenedClient = !(await CheckIsClientOpened(Infos[sessionId].WowProcess.Id, sessionId));

                if (CheckInterruptingLock(LocksManager.InterruptingOpeningWowClient, sessionId))
                {
                    Infos[sessionId].CloseWowProcess();
                    LocksManager.getInstance().Unlock(LocksManager.MainBotStartingProcess, sessionId);
                    await CallToStopProcess(sessionId);

                    return;
                }

            } while (requireToCheckOpenedClient);

            await currentInfo.SetState(ProcessStates.EnteringCredentials, 50);

            await EnterCredentials(sessionId);

            if (CheckInterruptingLock(LocksManager.InterruptingOpeningWowClient, sessionId))
            {
                Infos[sessionId].CloseWowProcess();
                LocksManager.getInstance().Unlock(LocksManager.MainBotStartingProcess, sessionId);
                await CallToStopProcess(sessionId);

                return;
            }

            bool requireToCheck = true;
            int attempt = 0;

            do
            {
                await currentInfo.SetState(ProcessStates.EnteringToWorld, 50);

                for(int _ii = 0; _ii < enteringToWorldDelay / 1000; _ii++)
                {
                    await Task.Delay(1000);

                    if (CheckInterruptingLock(LocksManager.InterruptingOpeningWowClient, sessionId))
                    {
                        Infos[sessionId].CloseWowProcess();
                        LocksManager.getInstance().Unlock(LocksManager.MainBotStartingProcess, sessionId);
                        await CallToStopProcess(sessionId);

                        return;
                    }
                }

                await currentInfo.SetState(ProcessStates.CheckingIsEnteredToWorld, 50);

                requireToCheck = !(await CheckIsCharacterEntered(Infos[sessionId].WowProcess.Id, sessionId));

                if (CheckInterruptingLock(LocksManager.InterruptingOpeningWowClient, sessionId))
                {
                    Infos[sessionId].CloseWowProcess();
                    LocksManager.getInstance().Unlock(LocksManager.MainBotStartingProcess, sessionId);
                    await CallToStopProcess(sessionId);

                    return;
                }

                attempt++;
                if (attempt >= 15)
                {
                    Infos[sessionId].WowProcess.Kill();
                    Infos[sessionId].WowProcess = null;
                    await OpenWowClient(sessionId);
                    return;
                }
            } while (requireToCheck);

            await currentInfo.SetState(ProcessStates.PreloadStringSending, 50);

            string keysToPressAfterEnteringToWorld = null;

            Logic.Execute(logic => keysToPressAfterEnteringToWorld = logic.settings.appSettings.profilesSettings[sessionId].keysToPressAfterEnteringToWorld, true);
            if (keysToPressAfterEnteringToWorld != null && keysToPressAfterEnteringToWorld.Length > 0)
                await WinApi.EnterString(Infos[sessionId].WowProcess.MainWindowHandle, keysToPressAfterEnteringToWorld);

            if (CheckInterruptingLock(LocksManager.InterruptingOpeningWowClient, sessionId))
            {
                Infos[sessionId].CloseWowProcess();
                LocksManager.getInstance().Unlock(LocksManager.MainBotStartingProcess, sessionId);
                await CallToStopProcess(sessionId);

                return;
            }

            LocksManager.getInstance().Unlock(LocksManager.MainBotStartingProcess, sessionId);
        }

        public async void WowBotLooping(int sessionId)
        {
            ProcessInfo currentInfo = Infos[sessionId];

            if (!currentInfo.State.Equals(ProcessStates.PreloadStringSending))
                return;

            await currentInfo.SetState(ProcessStates.Running, 50);

            LocksManager.getInstance().Lock(LocksManager.WowClientLooping, sessionId);

            currentInfo.isRunning = true;

            string profilePath = null;
            bool usingTrial = true;

            Logic.Execute(logic =>
            {
                profilePath = logic.settings.appSettings.profilesSettings[sessionId].profilePath;
                usingTrial = logic.settings.appSettings.generalSettings.usingTrial;
            }, true);

            while (currentInfo.isRunning)
            {
                LocksManager.getInstance().Lock(LocksManager.OpeningBimsbot);

                Infos[sessionId].BimsbotProcess = Process.Start(profilePath);

                await Task.Delay(3000);

                if (usingTrial)
                    WinApi.CloseWindow(currentInfo.BimsbotProcess.MainWindowHandle);

                await Task.Delay(3000);

                if (Infos.Where(info => info.WowProcess != null).Count() > 1)
                {
                    List<string> pids = WinApi.GetPidsList();
                    int ourLine = -1;

                    for (int i = 0; i < pids.Count(); i++)
                    {
                        if (pids[i].Contains(Infos[sessionId].WowProcess.Id.ToString()))
                            ourLine = i;
                    }

                    await WinApi.SelectLineInPidsList(ourLine);
                }

                await Task.Delay(1000);

                currentInfo.BimsbotHandler = await WinApi.FindBotWindow(Infos.Where(info => info.BimsbotHandler > 0).Select(info => info.BimsbotHandler).ToList());

                LocksManager.getInstance().Unlock(LocksManager.OpeningBimsbot);

                await WinApi.StartBot(currentInfo.BimsbotHandler);

                Random random = new Random();
                int delay = random.Next(300000, 450000);
                for (int i=0; i < delay / 1000; i++)
                {
                    await Task.Delay(1000);
                    if (CheckInterruptingLock(LocksManager.InterruptingRunning, sessionId))
                    {
                        currentInfo.isRunning = false;

                        currentInfo.CloseBimsbotProcess();
                        currentInfo.CloseWowProcess();

                        LocksManager.getInstance().Unlock(LocksManager.WowClientLooping, sessionId);
                        await CallToStopProcess(sessionId);

                        return;
                    }
                }

                currentInfo.CloseBimsbotProcess();
            }
        }

        public async Task OpenWowSessionTask(int sessionId)
        {
            ProcessInfo currentInfo = Infos[sessionId];

            if (!currentInfo.State.Equals(ProcessStates.WaitingOtherAutoReconnectProcesses))
                return;

            await OpenWowClient(sessionId);

            WowBotLooping(sessionId);
        }

        public async Task CallToStopProcess(int sessionId)
        {
            if (!LocksManager.getInstance().CheckLock(LocksManager.MainBotStartingProcess, sessionId) && !LocksManager.getInstance().CheckLock(LocksManager.AutoReconnectLooping, sessionId) && !LocksManager.getInstance().CheckLock(LocksManager.WowClientLooping, sessionId))
            {
                await Infos[sessionId].SetState(ProcessStates.Stopped, 50);
            }
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
                LocksManager.getInstance().Unlock(LocksManager.MainBotStartingProcess, sessionId);
                await CallToStopProcess(sessionId);

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
                LocksManager.getInstance().Unlock(LocksManager.MainBotStartingProcess, sessionId);
                await CallToStopProcess(sessionId);

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
                LocksManager.getInstance().Unlock(LocksManager.MainBotStartingProcess, sessionId);
                await CallToStopProcess(sessionId);

                return;
            }

            Infos[sessionId].WebDriver.Manage().Window.Position = new System.Drawing.Point(-32000, -32000);

            AutoReconnectLooping(sessionId);
            await currentInfo.SetState(ProcessStates.WaitingOtherAutoReconnectProcesses, 50);

            return;
        }
    }
}
