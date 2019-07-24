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
        public static ProcessState WaitingToStart = new ProcessState(1, "Waiting to start");
        public static ProcessState OpeningAutoReconnectChecking = new ProcessState(2, "Opening AutoReconnect client");
        public static ProcessState FillingAutoReconnectCaptcha = new ProcessState(3, "Filling AutoReconnect captcha");
        public static ProcessState WaitingOtherAutoReconnectProcesses = new ProcessState(4, "Waiting other AutoReconnect processes");
        public static ProcessState StartingWow = new ProcessState(5, "Starting Wow");
        public static ProcessState CheckingIsWowOpened = new ProcessState(6, "Checking is Wow opened");
        public static ProcessState EnteringCredentials = new ProcessState(7, "Entering credentials");
        public static ProcessState EnteringToWorld = new ProcessState(8, "Entering to world");
        public static ProcessState CheckingIsEnteredToWorld = new ProcessState(9, "Checking is entered to world");
        public static ProcessState PreloadStringSending = new ProcessState(10, "Preload string sending");
        public static ProcessState Running = new ProcessState(11, "Running");
        public static ProcessState AutoReconnect = new ProcessState(-1, "AutoReconnect");
        public static ProcessState NotFoundCharacter = new ProcessState(-10, "Error: Not found character");
    }
}
