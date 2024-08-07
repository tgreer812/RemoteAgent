using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AgentCommon.AgentPluginCommon
{
    public class PluginArguments
    {
        private readonly JObject _jObject;

        public PluginArguments()
        {
            _jObject = new JObject();
        }

        public PluginArguments(JObject jObject)
        {
            _jObject = new JObject(jObject);
        }

        public T GetArgument<T>(string key, T defaultValue = default)
        {
            if (_jObject.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var value))
            {
                try
                {
                    return value.ToObject<T>();
                }
                catch (Exception)
                {
                    // Log or handle the exception as needed
                }
            }

            return defaultValue;
        }

        public void AddArgument(string key, JToken value)
        {
            _jObject[key] = value;
        }

        public bool TryGetValue(string key, out JToken value)
        {
            return _jObject.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out value);
        }
    }
}
