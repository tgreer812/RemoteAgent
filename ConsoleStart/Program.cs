using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentCore;
using AgentCommon;

namespace CoreTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConsoleLogger logger = new ConsoleLogger();
            var core = new AgentCore.Core.CoreBuilder()
                .SetLogger(logger)
                .Build();

            core.Run();
        }
    }
}
