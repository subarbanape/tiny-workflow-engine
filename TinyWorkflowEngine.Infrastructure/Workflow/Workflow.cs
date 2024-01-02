using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyWorkflowEngine.Infrastructure
{
    public abstract class Workflow
    {
        public int? Id { get; set; }
        public int WorkflowDataKey { get; set; }
        public WorkflowTask[] Tasks { get; set; }
        public WorkflowComment[] Comments { get; set; }
        public WorkflowStatus Status { get; set; }
        public abstract string Data { get; }
        public WorkflowUser CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public WorkflowUser UpdatedBy { get; set; }
        public DateTime ? UpdatedDate { get; set; }
        public WorkflowUser[] InitiateRequestNotifyRecipients { get; set; }
        public WorkflowUser[] CompleteRequestNotifyRecipients { get; set; }
    }
}
