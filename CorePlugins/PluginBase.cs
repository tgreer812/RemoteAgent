using AgentCommon;
using AgentCommon.AgentPluginCommon;
using System.Threading;
using System.Threading.Tasks;

namespace CorePlugins
{
    public abstract class PluginBase : IPlugin
    {
        public ILogger Logger { get; set; }
        public PluginContext Context { get; set; }

        public PluginBase(PluginContext context)
        {
            // Pull out the logger for convenience
            Logger = context.Logger;
            Context = context;
        }

        public virtual Task<bool> LoadAsync(PluginArguments agentPluginArguments = null)
        {
            // Return a completed task with false to indicate not implemented, but allow override.
            return Task.FromResult(false);
        }

        public virtual Task<bool> UnloadAsync(PluginArguments agentPluginArguments = null)
        {
            // Return a completed task with false to indicate not implemented, but allow override.
            return Task.FromResult(false);
        }

        public virtual Task<PluginResult> StartAsync(PluginArguments agentPluginArguments = null, CancellationToken cancellationToken = default)
        {
            // Return a completed task with a default PluginResult (or you can customize it)
            // to indicate not implemented, but allow override.
            return Task.FromResult(default(PluginResult));
        }

        public virtual Task<bool> StopAsync(PluginArguments agentPluginArguments = null)
        {
            // Return a completed task with false to indicate not implemented, but allow override.
            return Task.FromResult(false);
        }
    }
}