using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Logs
{
    public class LogsElement
    {
        public int processId = -2;
        public string log = string.Empty;
        public DateTime time = DateTime.Now;

        public LogsElement(int _processId, string _log)
        {
            processId = _processId;
            log = _log;
            time = DateTime.Now;
        }
    }
}
