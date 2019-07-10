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
    }
}
