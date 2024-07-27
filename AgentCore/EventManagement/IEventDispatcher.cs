using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.EventManagement
{
    public interface IEventDispatcher
    {
        void Subscribe(string eventName, EventHandler handler);

        void Unsubscribe(string eventName, EventHandler handler);

        void Publish(string eventName, object sender, EventArgs e);
    }
}
