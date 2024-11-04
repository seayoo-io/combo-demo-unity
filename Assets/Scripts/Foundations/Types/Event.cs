using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ListenerInfo {
    public WeakRef<object> instance;
    public MethodInfo method;
    public Type eventType;

    public override int GetHashCode()
    {
        return instance.GetHashCode();
    }
}

public static class EventSystem
{
    private static readonly Dictionary<Type, HashSet<ListenerInfo>> eventListeners = 
        new Dictionary<Type, HashSet<ListenerInfo>>();

    public class BindEventAttribute : Attribute { }

    public static void Register(object listener)
    {
        var listenerType = listener.GetType();
        var methods = listenerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute(typeof(BindEventAttribute), false);
            if (attribute == null)
            {
                continue;
            }

            var parameter = method.GetParameters().FirstOrDefault();
            if (parameter == null) {
                Log.E($"Failed to register event for {listenerType}.{method.Name}, cannot find proper parameter");
                continue;
            }

            if (!typeof(Event).IsAssignableFrom(parameter.ParameterType)) {
                Log.E($"Failed to register event for {listenerType}.{method.Name}, The first parameter is not of derived from Event");
                continue;
            }

            var eventType = parameter.ParameterType;

            var lisenerInfo = new ListenerInfo {
                instance = new WeakRef<object>(listener),
                method = method,
                eventType = eventType,
            };

            if (!eventListeners.ContainsKey(eventType)) {
                eventListeners[eventType] = new HashSet<ListenerInfo>();
            }

            eventListeners[eventType].Add(lisenerInfo);
        }
        var a = eventListeners;
    }

    public static void UnRegister(object listener)
    {
        foreach (var listenerSet in eventListeners.Values)
        {
            listenerSet.RemoveWhere(info => info.instance.Target == listener);
        }
    }

    public static HashSet<ListenerInfo> GetListeners(Type eventType) {
        eventListeners.TryGetValue(eventType, out var result);
        return result;
    }
}

public abstract class Event<T> : Event where T : new() {
    public static void Invoke() {
        Invoke(new T());
    }

    public static void Invoke(T e)
    {
        var eventType = typeof(T);

        var listeners = EventSystem.GetListeners(eventType);
        if(listeners == null) {
            Log.W($"Event {eventType} has no subscriber");
            return;
        }

        foreach(var lisenerInfo in listeners) {
            if (!lisenerInfo.instance.IsAlive) {
                EventSystem.UnRegister(null);
                continue;
            }
            try {
                lisenerInfo.method.Invoke(lisenerInfo.instance.Target, new object[] { e });
            } catch (Exception ex) {
                Log.E(ex);
            }
            
        }
    }
}

public abstract class Event {
}


