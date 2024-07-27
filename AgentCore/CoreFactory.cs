using AgentCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore
{
    public static class CoreFactory
    {
        static CoreFactory()
        {
        }

        public static Core CreateCore()
        {
            Core.CoreBuilder builder = new Core.CoreBuilder();
            Core core = builder.Build();
            core.Run();

            return core;
        }
    }
}
