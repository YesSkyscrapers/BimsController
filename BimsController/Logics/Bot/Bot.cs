using BimsController.Defines;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBox = System.Windows.MessageBox;

namespace BimsController.Logics.Bot
{
    public class Bot
    {
        private string AUTORECONNECT_AUTH_URL = "https://cp.uwow.biz/auth/login";

        public bool isRunning = false;
        public SessionInfo[] Infos = new SessionInfo[] { new SessionInfo(), new SessionInfo(), new SessionInfo() };

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

                int profilesCountWithEnabledAutoreconnectOption = logic.settings.appSettings.profilesSettings.Select(profile => profile.autoReconnect).Where(enabled => enabled).Count();
                if (profilesCountWithEnabledAutoreconnectOption > 0)
                    MessageBox.Show("Please, fill captcha field in " + profilesCountWithEnabledAutoreconnectOption + " chrome windows");


                //parallel run
                await Task.WhenAll((new List<int>() { 0, 1, 2 }).Where(i => (id == i || id == -1)).Select(i =>
                    StartSession(i)).ToArray());

                //posled run
                /*
                for (int i = 0; i < 3; i++)
                {
                    if (logic.settings.appSettings.profilesSettings[i].enabled && (id == i || id == -1))
                        StartSession(i);
                }
                */
            });
        }

        private async Task SetStateWithDelay(int sessionId, ProcessState state)
        {
            Logic.Execute(_ => SetState(sessionId, state));
            await Task.Delay(100);
        }

        private async Task StartSession(int sessionId)
        {
            SetState(sessionId, ProcessStates.WaitingRunning);
            await Logic.ExecuteAsync(async (logic) =>
            {
                if (logic.settings.appSettings.profilesSettings[sessionId].autoReconnect)
                {
                    await SetStateWithDelay(sessionId, ProcessStates.OpeningOnlineChecker);

                    Infos[sessionId].AutoreconnectLoopingClosed = false;

                    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;

                    Infos[sessionId].WebDriver = new ChromeDriver(service);
                    Infos[sessionId].WebDriver.Navigate().GoToUrl(AUTORECONNECT_AUTH_URL);

                    await SetStateWithDelay(sessionId, ProcessStates.EnteringCredentialsToOnlineChecker);

                    Infos[sessionId].WebDriver.FindElement(By.Name("username")).SendKeys(logic.settings.appSettings.profilesSettings[sessionId].login);
                    Infos[sessionId].WebDriver.FindElement(By.Name("password")).SendKeys(logic.settings.appSettings.profilesSettings[sessionId].password);
                    Infos[sessionId].WebDriver.FindElement(By.ClassName("lbl")).Click();
                    Infos[sessionId].WebDriver.FindElement(By.Name("captcha")).Click();

                    await SetStateWithDelay(sessionId, ProcessStates.WaitingCaptchaFilling);

                    while (Infos[sessionId].WebDriver.Url == AUTORECONNECT_AUTH_URL)
                    {
                        await Task.Delay(1000);
                        if (Infos[sessionId].State.Equals(ProcessStates.Canceled))
                        {

                            Infos[sessionId].AutoreconnectLoopingClosed = true;
                            return;
                        }
                    }

                    Infos[sessionId].WebDriver.Manage().Window.Position = new System.Drawing.Point(-32000, -32000);
                }
            });

            if (Infos[sessionId].State.Equals(ProcessStates.Canceled))
            {

                Infos[sessionId].AutoreconnectLoopingClosed = true;
                return;
            }

            Logic.Execute(logic => SetState(sessionId, ProcessStates.Running));

            if (Infos[sessionId].WebDriver != null)
            {
                _ = Logic.ExecuteAsync(async logic =>
                {
                    Infos[sessionId].AutoreconnectCheckingEnabled = true;

                    while (Infos[sessionId].AutoreconnectCheckingEnabled)
                    {
                        //actions
                        await Task.Delay(100);
                    }

                    Infos[sessionId].CloseWebDriver();
                    Infos[sessionId].AutoreconnectLoopingClosed = true;
                });
            }
        }

        

        private async Task StopSession(int sessionId)
        {
            SetState(sessionId, ProcessStates.Canceled);
            
            if (Infos[sessionId].WebDriver != null)
            {
                Infos[sessionId].CloseWebDriver();
            }

            await Logic.ExecuteAsync(async (logic) =>
            {
                if (logic.settings.appSettings.profilesSettings[sessionId].autoReconnect)
                    while (!Infos[sessionId].AutoreconnectLoopingClosed)
                        await Task.Delay(200);
                SetState(sessionId, ProcessStates.Stopped);
            });
        }

        public void SetState(int sessionId, ProcessState state)
        {
            Infos[sessionId].State = state;
        }

        public async void Stop(int id = -1)
        {
            Logic.Execute(logic => logic.settings.windowSettings.isStartButtonLocked = true);
            _ = Logic.ExecuteAsync(async logic =>
              {
                //antiplaying delay - idk why...just safety
                await Task.Delay(3000);
                  logic.settings.windowSettings.isStartButtonLocked = false;
              });
            await Logic.ExecuteAsync(async logic => {
                
                await Task.WhenAll((new List<int>() { 0, 1, 2 }).Where(i => !Infos[i].State.Equals(ProcessStates.Stopped) && (id == i || id == -1)).Select(i => 
                StopSession(i)).ToArray());

                //Check for locking or unlocking settings window
                bool isAllProcessesStopped = true;

                for (int i = 0; i < 3; i++)
                {
                    if (i == id)
                        continue;
                    if (!Infos[i].State.Equals(ProcessStates.Stopped))
                        isAllProcessesStopped = false;
                }

                if (isAllProcessesStopped)
                    logic.settings.windowSettings.isLockSettingsWindow = false;
            });
        }
    }
}
