using Newtonsoft.Json;
using System;
using System.Diagnostics;
using UnityEngine;

public static class Log
{
    public static void I(object message)
    {
        UnityEngine.Debug.Log($"[Demo] {Tag}: " + ParseMessage(message));
    }

    public static void D(object message)
    {
        UnityEngine.Debug.Log($"[Demo] {Tag}: " + ParseMessage(message));
    }

    public static void E(object message)
    {
        UnityEngine.Debug.LogError($"[Demo] {Tag}: " + ParseMessage(message));
    }

    public static void W(object message)
    {
        UnityEngine.Debug.LogWarning($"[Demo] {Tag}: " + ParseMessage(message));
    }

    static string Tag {
        get {
            var type = new StackTrace()?.GetFrame(2)?.GetMethod()?.ReflectedType;
            while ((type?.Name?.Contains("__") ?? false) || (type?.Name?.Contains("`") ?? false) || (type?.Name?.Contains("<") ?? false) || (type?.Name?.Contains(">") ?? false))
                type = type?.ReflectedType ?? null;
            return type?.Name ?? "";
        }
    }

    static string ParseMessage(object message)
    {
        if (message.GetType().IsPrimitive || message is string)
        {
            return message.ToString();
        }
        else
        {
            try
            {
                return JsonConvert.SerializeObject(message);
            } catch (Exception)
            {
                return message.ToString();
            }
        }
    }
}