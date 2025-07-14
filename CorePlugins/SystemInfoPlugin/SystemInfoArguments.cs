using AgentCommon.AgentPluginCommon;

namespace CorePlugins.SystemInfoPlugin
{
    internal class SystemInfoArguments
    {
        public bool IncludeEnvironmentVariables { get; set; }
        public bool IncludeNetworkInfo { get; set; }

        public SystemInfoArguments(PluginArguments args)
        {
            this.IncludeEnvironmentVariables = args.GetArgument<bool>("IncludeEnvironmentVariables");
            this.IncludeNetworkInfo = args.GetArgument<bool>("IncludeNetworkInfo");
        }
    }
}
