﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.CommunicationManagement
{
    public interface ICommunicationManager
    {
        void Initialize();
        //ServerResponse SendMessage(AgentMessage agentMessage);
    }
}