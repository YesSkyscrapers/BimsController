using BimsController.Defines;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Bot
{
    public class ProcessInfo
    {
        public ProcessState State = ProcessStates.Stopped;
        public bool AutoReconnectEnabled = false;
        public IWebDriver WebDriver = null;
        public bool CharacterStatus = false;
        public bool AutoReconnectProcessStarted = false;
        public Process WowProcess = null;
        public Process BimsbotProcess = null;
        public bool isRunning = false;
        public Int32 BimsbotHandler = 0;

        public void SetState(ProcessState state)
        {
            Logic.Execute(logic => this.State = state);
        }

        public async Task SetState(ProcessState state, int delay)
        {
            Logic.Execute(logic => this.State = state);
            await Task.Delay(delay);
        }

        public void CloseWebDriver()
        {
            if (WebDriver != null)
            {
                WebDriver.Quit();
                WebDriver = null;
            }
        }

        public void CloseWowProcess()
        {
            if (WowProcess != null)
            {
                WowProcess.Kill();
                WowProcess = null;
            }
        }

        public void CloseBimsbotProcess()
        {
            if (BimsbotProcess != null)
            {
                BimsbotProcess.Kill();
                BimsbotProcess = null;
                if (BimsbotHandler > 0)
                    BimsbotHandler = 0;
            }
        }
    }
}
