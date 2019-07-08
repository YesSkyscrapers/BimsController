using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Logics
{
    public class Logic
    {
        public delegate void LogicAction(Logic logic);

        private static Logic _logic;
        private static List<LogicAction> subscribers = new List<LogicAction>();

        public Settings.Settings settings = new Settings.Settings();
        public Logs.Logs logs = new Logs.Logs();

        public static Logic Execute(LogicAction action)
        {
            if (_logic == null)
                _logic = new Logic();

            action(_logic);

            subscribers.ForEach(subscriber =>
            {
                subscriber(_logic);
            });

            return _logic;
        }

        public static void Subscribe(LogicAction action)
        {
            if (_logic == null)
                _logic = new Logic();
            subscribers.Add(action);
        }

        public static void Unsubscribe(LogicAction action)
        {
            subscribers.Remove(action);
        }
    }
}
