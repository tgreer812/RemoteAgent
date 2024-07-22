using AgentCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.CommunicationManagement
{
    public class CommunicationManager : ICommunicationManager
    {
        internal bool IsRunning { get; private set; }
        private ILogger Logger { get; set; }
        internal CommunicationManager(ILogger logger)
        {
            Logger = logger;
        }

        public async Task Start()
        {
            if (IsRunning)
            {
                Logger.LogWarning("Communication manager is already running");
                return;
            }

            Logger.LogInfo("Communication manager is starting...");
            IsRunning = true;
            while (IsRunning)
            {
                Logger.LogDebug("Communication manager is running");
                await Task.Delay(1000);
            }
        }

        public async Task<bool> Stop()
        {
            IsRunning = false;
            Logger.LogInfo("Communication manager is stopping...");
            return await Task.FromResult(true);
        }
    }
}
