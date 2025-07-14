using AgentCommon.AgentPluginCommon;

namespace CorePlugins.DownloadFilePlugin
{
    internal class DownloadFileArguments
    {
        public string FilePath { get; set; }

        public DownloadFileArguments(PluginArguments args)
        {
            this.FilePath = args.GetArgument<string>("FilePath");
        }
    }
}
