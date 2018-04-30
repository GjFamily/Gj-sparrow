using System;
using System.Collections;
using System.Collections.Generic;

namespace Gj
{
    public class EventService
    {
        public static EventService single;
        private Dictionary<string, List<Event>> eventDic = new Dictionary<string, List<Event>>();

        private struct Event
        {
            public Action<float> action;
            public Action<byte, float> cateAction;
            public string key;
            public bool once;
        }

        static EventService()
        {
            single = new EventService();
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

        public string On(byte type, byte category, Action<float> eventAction, bool once = false)
        {
            Event e = new Event
            {
                key = SimpleTools.GenerateStr(8),
                action = eventAction,
                once = once
            };
            return On(GetKey(type, category), e);
        }

        public string On(byte type, Action<byte, float> eventAction, bool once = false)
        {
            Event e = new Event
            {
                key = SimpleTools.GenerateStr(8),
                cateAction = eventAction,
                once = once
            };
            return On(GetKey(type), e);
        }

        public string Once(byte type, Action<byte, float> eventAction)
        {
            return On(type, eventAction, true);
        }

        public string Once(byte type, byte category, Action<float> eventAction)
        {
            return On(type, category, eventAction, true);
        }

        public void Off(byte type, string key)
        {
            List<Event> eventList = GetEventList(GetKey(type));
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

        public void Off(byte type, byte category, string key)
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

        private List<Event> GetEventList(string key)
        {
            if (eventDic.ContainsKey(key))
            {
                return eventDic[key];
            }
            else
            {
                return null;
            }
        }

        public void Emit(byte type, byte category, float value)
        {
            List<Event> eventList = GetEventList(GetKey(type));
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
            if (category != 0) {
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

        public void Emit(byte type)
        {
            Emit(type, 0, 0);
        }

        public void Emit(byte type, byte category)
        {
            Emit(type, category, 0);
        }

        private string GetKey(byte type, byte category)
        {
            return type + "-" + category;
        }

        private string GetKey(byte type)
        {
            return type + "*";
        }
    }
}
