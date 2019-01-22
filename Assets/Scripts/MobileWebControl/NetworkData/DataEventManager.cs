using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
using MobileWebControl.NetworkData;

namespace MobileWebControl.NetworkData
{
    public class DataEventManager
    {
        //using a special unityevent inside manager.
        private class DataHolderEvent : UnityEvent<DataHolder>
        {
        }

        private Dictionary<NetworkEventType, DataHolderEvent> eventDictionary;

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
                eventDictionary = new Dictionary<NetworkEventType, DataHolderEvent>();
            }
        }

        public static void StartListening(NetworkEventType eventType, UnityAction<DataHolder> listener)
        {
            DataHolderEvent thisEvent = null;
            if (instance.eventDictionary.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new DataHolderEvent();
                thisEvent.AddListener(listener);
                instance.eventDictionary.Add(eventType, thisEvent);
            }
        }

        public static void StopListening(NetworkEventType eventType, UnityAction<DataHolder> listener)
        {
            if (eventManager == null) return;
            DataHolderEvent thisEvent = null;
            if (instance.eventDictionary.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public static void TriggerEvent(NetworkEventType eventType, DataHolder data)
        {
            DataHolderEvent thisEvent = null;
            if (instance.eventDictionary.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.Invoke(data);
            }
        }
    }
}