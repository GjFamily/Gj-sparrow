using System;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class EventManage
    {
        public static EventManage single;
        private Dictionary<string, List<Event>> eventDic = new Dictionary<string, List<Event>>();

        private struct Event
        {
            public Action<float> action;
            public Action<string, float> cateAction;
            public string key;
            public bool once;
        }

        static EventManage()
        {
            single = new EventManage();
        }

        private string On(string key, Event e)
        {
            List<Event> eventList = GetEventList(key);
            if (eventList == null)
            {
                eventList = new List<Event>();
                eventDic.Add(key, eventList);
            }
            eventList.Add(e);
            return e.key;
        }

        public string On(string type, string category, Action<float> eventAction, bool once = false)
        {
            Event e = new Event
            {
                key = SimpleTools.GenerateStr(8),
                action = eventAction,
                once = once
            };
            return On(GetKey(type, category), e);
        }

        public string On(string type, Action<string, float> eventAction, bool once = false)
        {
            Event e = new Event
            {
                key = SimpleTools.GenerateStr(8),
                cateAction = eventAction,
                once = once
            };
            return On(type, e);
        }

        public string Once(string type, Action<string, float> eventAction)
        {
            return On(type, eventAction, true);
        }

        public string Once(string type, string category, Action<float> eventAction)
        {
            return On(type, category, eventAction, true);
        }

        public void Off(string type, string key)
        {
            List<Event> eventList = GetEventList(type);
            if (eventList != null)
            {
                foreach (Event e in eventList)
                {
                    if (key.Equals(e.key))
                    {
                        eventList.Remove(e);
                    }
                }
            }
        }

        public void Off(string type, string category, string key)
        {
            List<Event> eventList = GetEventList(GetKey(type, category));
            if (eventList != null)
            {
                foreach (Event e in eventList)
                {
                    if (key.Equals(e.key))
                    {
                        eventList.Remove(e);
                    }
                }
            }
        }

        private List<Event> GetEventList(string type)
        {
            if (eventDic.ContainsKey(type))
            {
                return eventDic[type];
            }
            else
            {
                return null;
            }
        }

        public void Emit(string type, string category, float value)
        {
            List<Event> eventList = GetEventList(type);
            if (eventList != null)
            {
                foreach (Event e in eventList)
                {
                    if (e.cateAction != null)
                    {
                        e.cateAction(category, value);
                        if (e.once)
                        {
                            eventList.Remove(e);
                        }
                    }
                    else
                    {
                        eventList.Remove(e);
                    }
                }
            }
            if (category != null) {
                eventList = GetEventList(GetKey(type, category));
                if (eventList != null)
                {
                    foreach (Event e in eventList)
                    {
                        if (e.action != null)
                        {
                            e.action(value);
                            if (e.once)
                            {
                                eventList.Remove(e);
                            }
                        }
                        else
                        {
                            eventList.Remove(e);
                        }
                    }
                }
            }
        }

        public void Emit(string type)
        {
            Emit(type, null, 0);
        }

        public void Emit(string type, string category)
        {
            Emit(type, category, 0);
        }

        private string GetKey(string type, string category)
        {
            return type + "-" + category;
        }
    }
}
