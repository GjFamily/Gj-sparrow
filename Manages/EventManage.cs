using System;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class EventManage
    {
        public static EventManage single;
        private Dictionary<string, List<Event>> eventDic = new Dictionary<string, List<Event>>();

        private struct Event {
            public Action<String[]> action;
            public string key;
            public bool once;
        }

        static EventManage()
        {
            single = new EventManage();
        }

        public string On(string eventName, Action<string[]> eventAction, bool once = false)
        {
            List<Event> eventList = GetEventList(eventName);
            if (eventList == null)
            {
                eventList = new List<Event>();
                eventDic.Add(eventName, eventList);
            }
            Event e = new Event
            {
                key = SimpleTools.GenerateStr(8),
                action = eventAction,
                once = once
            };
            eventList.Add(e);
            return e.key;
        }

        public string Once(string eventName, Action<string[]> eventAction) {
            return On(eventName, eventAction, true);
        }

        public void Off (string eventName, string key) {
            List<Event> eventList = GetEventList(eventName);
            if (eventList != null) {
                foreach (Event e in eventList) {
                    if (key.Equals(e.key)) {
                        eventList.Remove(e);
                    }
                }
            }
        }

        private List<Event> GetEventList(string eventName)
        {
            if (eventDic.ContainsKey(eventName))
            {
                return eventDic[eventName];
            }
            else
            {
                return null;
            }
        }

        public void Emit(string eventName, string[] param)
        {
            List<Event> eventList = GetEventList(eventName);
            if (eventList != null)
            {
                foreach (Event e in eventList)
                {
                    if (e.action != null)
                    {
                        e.action(param);
                        if (e.once)
                        {
                            eventList.Remove(e);
                        }
                    } else {
                        eventList.Remove(e);
                    }
                }
            }
        }

        public void Emit(string eventName)
        {
            Emit(eventName, new string[] { });
        }

        public void Emit(string eventName, string param1)
        {
            Emit(eventName, new string[] { param1 });
        }

        public void Emit(string eventName, string param1, string param2)
        {
            Emit(eventName, new string[] { param1, param2 });
        }

        public void Emit(string eventName, string param1, string param2, string param3)
        {
            Emit(eventName, new string[] { param1, param2, param3 });
        }
    }
}
