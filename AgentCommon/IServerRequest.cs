﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace AgentCommon 
{ 
    public interface IServerRequest
    {
        string Endpoint { get; }
        string Method { get; }
        object Payload { get; }
    }

}