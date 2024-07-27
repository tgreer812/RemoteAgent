using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class LoadPriorityAttribute : Attribute
    {
        public int LoadPriority { get; }

        public LoadPriorityAttribute(int priority)
        {
            LoadPriority = priority;
        }
    }
}
