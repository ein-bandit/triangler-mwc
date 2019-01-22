using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

public class DataEvent : UnityEvent<object>
{
}

public class DataEventManager
{
    private Dictionary<DataEventType, DataEvent> eventDictionary;

    private static DataEventManager eventManager;

    public static DataEventManager instance
    {
        get
        {
            if (eventManager == null)
            {
                eventManager = new DataEventManager();

                eventManager.Init();
            }

            return eventManager;
        }
    }

    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<DataEventType, DataEvent>();
        }
    }

    public static void StartListening(DataEventType eventType, UnityAction<object> listener)
    {
        DataEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new DataEvent();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventType, thisEvent);
        }
    }

    public static void StopListening(DataEventType eventType, UnityAction<object> listener)
    {
        if (eventManager == null) return;
        DataEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(DataEventType eventType, object data)
    {
        DataEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventType, out thisEvent))
        {
            thisEvent.Invoke(data);
        }
    }
}