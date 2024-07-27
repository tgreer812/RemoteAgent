using AgentCommon.AgentPluginCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlugins.DirectoryListPlugin
{
    internal class DirectoryListArguments
    {
        public string Path { get; set; }
        public DirectoryListArguments(PluginArguments args)
        {
            this.Path = args.GetArgument<string>("Path");
        }
    }
}
