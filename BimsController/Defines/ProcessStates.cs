using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Defines
{
    public static class ProcessStates
    {
        public static ProcessState Canceled = new ProcessState(-1, "Canceled");
        public static ProcessState Stopped = new ProcessState(0, "Stopped");
        public static ProcessState Running = new ProcessState(1, "Running");
        public static ProcessState WaitingRunning = new ProcessState(2, "WaitingRunning");
        public static ProcessState OpeningOnlineChecker = new ProcessState(3, "Opening Online Checker");
        public static ProcessState EnteringCredentialsToOnlineChecker = new ProcessState(4, "Entering Credentials To Online Checker");
        public static ProcessState WaitingCaptchaFilling = new ProcessState(5, "Waiting Captcha Filling");
        public static ProcessState Checking = new ProcessState(6, "Checking");
    }
}
