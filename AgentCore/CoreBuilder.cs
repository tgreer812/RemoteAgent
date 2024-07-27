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
        internal class CoreBuilder
        {
            private ILogger _logger;
            private IPluginManager _pluginManager;
            private IJobManager _jobManager;
            private IEventDispatcher _eventManager;
            private ICommunicationManager _communicationManager;

            internal CoreBuilder SetLogger(ILogger logger)
            {
                _logger = logger;
                return this;
            }

            internal CoreBuilder SetPluginManager(IPluginManager pluginManager)
            {
                _pluginManager = pluginManager;
                return this;
            }

            internal CoreBuilder SetJobManager(IJobManager jobManager)
            {
                _jobManager = jobManager;
                return this;
            }

            internal CoreBuilder SetEventManager(IEventDispatcher eventManager)
            {
                _eventManager = eventManager;
                return this;
            }

            internal CoreBuilder SetCommunicationManager(ICommunicationManager communicationManager)
            {
                _communicationManager = communicationManager;
                return this;
            }

            internal Core Build()
            {
                if (Core.Instance != null)
                {
                    throw new InvalidOperationException("Core instance already exists!");
                }

                if ( _logger == null)
                {
                    _logger = new ConsoleLogger();
                }

                Core.Instance = new Core
                {
                    Logger = _logger,
                    PluginManager = _pluginManager ?? new PluginManager(_logger),
                    JobManager = _jobManager ?? new JobManager(_logger),
                    EventManager = _eventManager ?? new EventDispatcher(_logger),
                    CommunicationManager = _communicationManager ?? new CommunicationManager(_logger)
                };

                return Core.Instance;
            }
        }
    }

}
