using System;
using System.Collections;
using System.Collections.Generic;

public class EventManage
{
    public static EventManage single;
    private Dictionary<string, List<Action<string[]>>> eventDic = new Dictionary<string, List<Action<string[]>>>();


    static EventManage()
    {
        single = new EventManage();
    }

    public void On(string eventName, Action<string[]> eventAction)
    {
        List<Action<string[]>> eventList = GetEventList(eventName);
        if (eventList == null)
        {
            eventList = new List<Action<string[]>>();
            eventDic.Add(eventName, eventList);
        }
        eventList.Add(eventAction);
    }

    private List<Action<string[]>> GetEventList(string eventName)
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
        List<Action<string[]>> eventList = GetEventList(eventName);
        if (eventList != null)
        {
            foreach (Action<string[]> action in eventList)
            {
                if (action != null)
                {
                    action(param);
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

    public void Clean(string eventName, Action<string[]> eventAction)
    {
        List<Action<string[]>> eventList = GetEventList(eventName);
        if (eventList != null)
        {
            eventList.Remove(eventAction);
        }
    }
}
