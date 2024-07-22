﻿using System;
using System.Collections.Generic;

namespace AgentCommon.AgentPluginCommon
{
    public class PluginArguments : Dictionary<string, object>
    {
        public PluginArguments() : base()
        {
        }

        public PluginArguments(IDictionary<string, object> dictionary) : base(dictionary)
        {
        }

        public T GetArgument<T>(string key, T defaultValue = default)
        {
            if (TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return defaultValue;
        }
    }
}