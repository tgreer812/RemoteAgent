﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentCore.JobManagement
{
    internal interface IJobConverter
    {
        Job Convert(object jobData);
    }
}
