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
        public static ProcessState Running = new ProcessState(5, "Running");
        public static ProcessState NotFoundCharacter = new ProcessState(-10, "NotFoundCharacter");
    }
}
