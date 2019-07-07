using BimsController.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics.Logs
{
    public delegate void ReadLogsDelegate();

    public class Logs
    {
        private List<LogsElement> _logs;
        private List<ReadLogsDelegate> _subscribers;
        private LogsWindow[] windows = new LogsWindow[3];

        public Logs()
        {
            _subscribers = new List<ReadLogsDelegate>();
            _logs = new List<LogsElement>();
        }

        public void Subscribe(ReadLogsDelegate subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void Unsubscribe(ReadLogsDelegate subscriber)
        {
            _subscribers.Remove(subscriber);
        }


        public void Log(int processId, string log)
        {
            _logs.Add(new LogsElement(processId, log));

            _subscribers.ForEach(subscriber => subscriber());
        }

        public string ReadLogs(int processId)
        {
            return string.Join("\r\n", _logs.Where(logElement => logElement.processId == processId).Select(logElement => logElement.log));
        }

        public void OpenLogsWindow(int processId)
        {
            if (windows[processId] != null)
            {
                windows[processId].Activate();
            }
            else
            {
                windows[processId] = new LogsWindow();
                windows[processId]._processId = processId;
                windows[processId].Show();
            }
        }

        public void CloseLogsWindow(int processId)
        {
            windows[processId] = null;
        }
    }
}
