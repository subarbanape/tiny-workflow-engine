﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyWorkflowEngine.Infrastructure
{
    public interface IUserDataProvider
    {
        WorkflowUser GetUser(string userName);
    }
}
