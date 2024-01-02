using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyWorkflowEngine.Infrastructure
{
    public enum WorkflowTaskStatus
    {
        Complete = 0,
        Assigned = 1,
        Cancelled = 2,
    }
}
