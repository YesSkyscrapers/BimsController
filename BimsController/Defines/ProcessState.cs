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

        public override bool Equals(object anotherStateObj)
        {
            if (anotherStateObj == null)
                return false;
            ProcessState anotherState = anotherStateObj as ProcessState; // возвращает null если объект нельзя привести к типу Money
            if (anotherStateObj as ProcessState == null)
                return false;

            return this.id == anotherState.id;
        }
    }
}
