using BimsController.Defines;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Bot
{
    public class SessionInfo
    {
        public ProcessState State;
        public IWebDriver WebDriver;
        public bool AutoreconnectCheckingEnabled = false;
        public bool AutoreconnectLoopingClosed = false;

        public SessionInfo()
        {
            State = ProcessStates.Stopped;
        }

        public void CloseWebDriver()
        {
            if (AutoreconnectCheckingEnabled)
                AutoreconnectCheckingEnabled = false;
            else
            {
                WebDriver.Quit();
                WebDriver = null;
            }
        }
    }
}
