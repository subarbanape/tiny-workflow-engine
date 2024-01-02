using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyWorkflowEngine.Infrastructure
{
    public enum WorkflowStatus
    {
        Complete = 0,
        Active = 1,
        Cancelled = 2,
    }
}
