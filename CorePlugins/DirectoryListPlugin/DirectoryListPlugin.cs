using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentCommon;
using AgentCommon.AgentPluginCommon;

namespace CorePlugins.DirectoryListPlugin
{
    [AgentPlugin("DirectoryListPlugin")]
    internal class DirectoryListPlugin : PluginBase
    {
        
        public DirectoryListPlugin(PluginContext context) : base(context)
        {
        }   

        public override bool Load(PluginArguments agentPluginArguments = null)
        {
            Logger.LogInfo("DirectoryListPlugin loaded");
            return true;
        }

        public override bool Start(PluginArguments args = null)
        {
            DirectoryListArguments arguments = new DirectoryListArguments(args);

            if (arguments.Path == null)
            {
                Logger.LogError("Path argument is required");
                return false;
            }

            if (!Directory.Exists(arguments.Path))
            {
                Logger.LogError("Path does not exist");
                return false;
            }

            try
            {
                string[] files = Directory.GetFiles(arguments.Path);
                string output = $@"Directory listing for {arguments.Path}:"
                    + Environment.NewLine
                    + string.Join(Environment.NewLine, files);

                // TODO: Properly send this output back to the server
                Console.WriteLine(output);
            }
            catch (Exception e)
            {
                Logger.LogError("Error listing directory: " + e.Message);
                return false;
            }

            return true;
            
        }
    }
}
