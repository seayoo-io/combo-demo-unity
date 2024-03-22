using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Types
{
    [System.Serializable]
    public abstract class Serializable
    {
        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this);
            }
            catch (Exception _e)
            {
                throw new SerializationException(_e.Message, GetType().Name, "json");
            }
        }

        public static T FromJson<T>(string json)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };

                return JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (Exception _e)
            {
                throw new SerializationException(_e.Message, json, typeof(T).Name);
            }
        }

        public static T FromDict<T>(Dictionary<string, object> dict) where T : Serializable
        {
            try
            {
                string json = JsonConvert.SerializeObject(dict);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonSerializationException _e)
            {
                throw new SerializationException(_e.Message, dict.ToString(), typeof(T).Name);
            }
        }
    }

    public class SerializationException : JsonSerializationException
    {
        override public string Source { get; set; }
        public string Destination { get; }

        public SerializationException(string message, string source, string destination)
            : base(message)
        {
            Source = source;
            Destination = destination;
        }

        public SerializationException(string message, string source, string destination, Exception innerException)
            : base(message, innerException)
        {
            Source = source;
            Destination = destination;
        }

        // 如果需要其他构造函数，可以根据需要添加

        // 自定义ToString方法以提供更多信息
        public override string ToString()
        {
            return $"SerializationException: {Message}\nSource: {Source}\nDestination Type: {Destination}\n{StackTrace}";
        }
    }
}
