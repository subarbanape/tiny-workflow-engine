using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyWorkflowEngine.Infrastructure
{
    public class DefaultWorkflow : Workflow
    {
        public string DefaultWorkflowData { get; set; }
        public override string Data => DefaultWorkflowData;
    }
}
