using AgentCommon;
using AgentCore.CommunicationManagement;
using AgentCore.EventManagement;
using AgentCore.JobManagement;
using AgentCore.PluginManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore
{
    public partial class Core
    {
        public class CoreBuilder
        {
            private ILogger _logger;
            private IPluginManager _pluginManager;
            private IJobManager _jobManager;
            private IEventDispatcher _eventManager;
            private ICommunicationManager _communicationManager;

            public CoreBuilder SetLogger(ILogger logger)
            {
                _logger = logger;
                return this;
            }

            public CoreBuilder SetPluginManager(IPluginManager pluginManager)
            {
                _pluginManager = pluginManager;
                return this;
            }

            public CoreBuilder SetJobManager(IJobManager jobManager)
            {
                _jobManager = jobManager;
                return this;
            }

            public CoreBuilder SetEventManager(IEventDispatcher eventManager)
            {
                _eventManager = eventManager;
                return this;
            }

            public CoreBuilder SetCommunicationManager(ICommunicationManager communicationManager)
            {
                _communicationManager = communicationManager;
                return this;
            }

            public Core Build()
            {
                if (Core.Instance != null)
                {
                    throw new InvalidOperationException("Core instance already exists!");
                }

                Core.Instance = new Core
                {
                    Logger = _logger ?? new ConsoleLogger(),
                    PluginManager = _pluginManager ?? new PluginManager(_logger),
                    JobManager = _jobManager ?? new JobManager(_logger, null),
                    EventManager = _eventManager ?? new EventDispatcher(_logger),
                    CommunicationManager = _communicationManager ?? new CommunicationManager()
                };

                return Core.Instance;
            }
        }
    }

}
