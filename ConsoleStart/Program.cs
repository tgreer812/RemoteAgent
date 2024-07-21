﻿using System;
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
            AgentCore.Core.Run(logger);
        }
    }
}
