using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCommon.AgentPluginCommon
{
    public class AgentPluginAttribute : Attribute
    {
        public string Name { get; set; }
        public AgentPluginAttribute(string name) 
        {
            Name = name;
        }
    }
}
