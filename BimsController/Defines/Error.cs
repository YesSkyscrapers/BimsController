using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Defines
{
    public class Error : Exception
    {
        public int id = -1;
        public string description = null;
        public Error(int id, string description)
        {
            this.id = id;
            this.description = description;
        }
    }
}
