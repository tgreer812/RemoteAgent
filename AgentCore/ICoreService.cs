﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore
{
    public interface ICoreService
    {
        Task Start();
        Task<bool> Stop();
    }
}