using AgentCommon.AgentPluginCommon;

namespace CorePlugins.ExecuteCommandPlugin
{
    internal class ExecuteCommandArguments
    {
        public string Command { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public int TimeoutSeconds { get; set; }
        public bool CaptureOutput { get; set; }
        public bool UseShell { get; set; }

        public ExecuteCommandArguments(PluginArguments args)
        {
            this.Command = args.GetArgument<string>("Command");
            this.Arguments = args.GetArgument<string>("Arguments") ?? "";
            this.WorkingDirectory = args.GetArgument<string>("WorkingDirectory") ?? "";
            this.TimeoutSeconds = args.GetArgument<int>("TimeoutSeconds");
            if (this.TimeoutSeconds <= 0) this.TimeoutSeconds = 30; // Default 30 seconds
            this.CaptureOutput = args.GetArgument<bool>("CaptureOutput", true); // Default true
            this.UseShell = args.GetArgument<bool>("UseShell", false); // Default false for security
        }
    }
}
