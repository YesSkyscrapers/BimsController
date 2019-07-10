using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Defines
{
    public class ProcessState
    {
        public int id = -1;
        public string description = null;
        public ProcessState(int id, string description)
        {
            this.id = id;
            this.description = description;
        }

        public bool Equals(ProcessState anotherState)
        {
            return anotherState.id == this.id;
        }
    }
}
