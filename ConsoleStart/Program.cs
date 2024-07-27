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
            // Use CoreFactory to create an instance of Core
            CoreFactory.CreateCore();

        }
    }
}