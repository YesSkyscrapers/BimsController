using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Defines
{
    public static class ProcessStates
    {
        public static ProcessState Stopped = new ProcessState(0, "Stopped");
        public static ProcessState WaitingToStart = new ProcessState(1, "WaitingToStart");
        public static ProcessState OpeningAutoReconnectChecking = new ProcessState(2, "OpeningAutoReconnectChecking");
        public static ProcessState FillingAutoReconnectCaptcha = new ProcessState(3, "FillingAutoReconnectCaptcha");
        public static ProcessState WaitingOtherAutoReconnectProcesses = new ProcessState(4, "WaitingOtherAutoReconnectProcesses");
        public static ProcessState StartingWow = new ProcessState(5, "StartingWow");
        public static ProcessState CheckingIsWowOpened = new ProcessState(6, "CheckingIsWowOpened");
        public static ProcessState EnteringCredentials = new ProcessState(7, "EnteringCredentials");
        public static ProcessState EnteringToWorld = new ProcessState(8, "EnteringToWorld");
        public static ProcessState CheckingIsEnteredToWorld = new ProcessState(9, "CheckingIsEnteredToWorld");
        public static ProcessState PreloadStringSending = new ProcessState(10, "PreloadStringSending");
        public static ProcessState Running = new ProcessState(11, "Running");
        public static ProcessState NotFoundCharacter = new ProcessState(-10, "NotFoundCharacter");
    }
}
