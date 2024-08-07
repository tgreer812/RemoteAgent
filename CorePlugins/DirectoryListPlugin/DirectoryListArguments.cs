using AgentCommon.AgentPluginCommon;

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
