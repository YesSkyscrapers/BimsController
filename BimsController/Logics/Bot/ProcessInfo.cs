using BimsController.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Bot
{
    public class ProcessInfo
    {
        public ProcessState State = ProcessStates.Stopped;

        public void SetState(ProcessState state)
        {
            Logic.Execute(logic => this.State = state);
        }
    }
}
