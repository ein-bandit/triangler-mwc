using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
using MobileWebControl.NetworkData;
using UnityToolbag;

namespace MobileWebControl.NetworkData
{
    public class NetworkEventDispatcher
    {
        //Runs on the Network Thread and collects events from 
        //using Dispatcher.cs to wait for Unity Main Thread to execute Update. 
        //using a special unityevent inside manager.
        private class AsyncRetrievedEvent : UnityEvent<DataHolder> { }

        private Dictionary<NetworkEventType, AsyncRetrievedEvent> eventDictionary;

        private static NetworkEventDispatcher eventManager;

        public static NetworkEventDispatcher instance
        {
            get
            {
                if (eventManager == null)
                {
                    eventManager = new NetworkEventDispatcher();

                    eventManager.Init();
                }

                return eventManager;
            }
        }

        void Init()
        {
            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<NetworkEventType, AsyncRetrievedEvent>();
            }
        }

        public static void StartListening(NetworkEventType eventType, UnityAction<DataHolder> listener)
        {
            AsyncRetrievedEvent thisEvent = null;
            if (instance.eventDictionary.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new AsyncRetrievedEvent();
                thisEvent.AddListener(listener);
                instance.eventDictionary.Add(eventType, thisEvent);
            }
        }

        public static void StopListening(NetworkEventType eventType, UnityAction<DataHolder> listener)
        {
            AsyncRetrievedEvent thisEvent = null;
            if (instance.eventDictionary.TryGetValue(eventType, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        //trigger event is called from another thread when data is received.
        //with the dispatcher triggerEvent waits for unity to be ready and sends event immediately.
        public static void TriggerEvent(NetworkEventType eventType, DataHolder data)
        {
            AsyncRetrievedEvent thisEvent = null;
            if (instance.eventDictionary.TryGetValue(eventType, out thisEvent))
            {
                int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                Dispatcher.InvokeAsync(() =>
                {
                    Debug.Log($"calling event on unity thread, from {threadId}");
                    thisEvent.Invoke(data);
                });
            }
        }
    }
}