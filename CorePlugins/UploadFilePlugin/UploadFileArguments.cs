using AgentCommon.AgentPluginCommon;

namespace CorePlugins.UploadFilePlugin
{
    internal class UploadFileArguments
    {
        public string FilePath { get; set; }
        public string Content { get; set; }
        public string Encoding { get; set; }
        public bool CreateDirectories { get; set; }

        public UploadFileArguments(PluginArguments args)
        {
            this.FilePath = args.GetArgument<string>("FilePath");
            this.Content = args.GetArgument<string>("Content");
            this.Encoding = args.GetArgument<string>("Encoding") ?? "base64";
            this.CreateDirectories = args.GetArgument<bool>("CreateDirectories");
        }
    }
}
