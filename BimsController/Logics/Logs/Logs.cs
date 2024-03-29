﻿using BimsController.Managers;
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
        private int maxLogsCount = 200;
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
            if (_logs.Where(elem => elem.processId == processId).Count() > maxLogsCount)
            {
                var oldLogs = _logs.Where(elem => elem.processId == processId).Take(10);
                oldLogs.ToList().ForEach(oldLog => _logs.Remove(oldLog));
            }

            LogsElement logElement = new LogsElement(processId, log);

            _logs.Add(logElement);

            FileManager.getInstance().WriteLogs(processId, string.Format("[{0}]: {1}", logElement.time.ToString("HH:mm:ss"), logElement.log));

            _subscribers.ForEach(subscriber => subscriber());
        }

        public string ReadLogs(int processId)
        {
            return string.Join("\r\n", _logs.Where(logElement => logElement.processId == processId).Select(logElement => string.Format("[{0}]: {1}", logElement.time.ToString("HH:mm:ss"), logElement.log)));
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
