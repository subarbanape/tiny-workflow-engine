using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyWorkflowEngine.Infrastructure
{
    public class WorkflowNotificationRequest
    {
        public WorkflowUser From { get; set; }
        public WorkflowUser[] ToAddresses { get; set; }
        public WorkflowUser[] CCAddresses { get; set; }
        public string Subject;
        public string Content;
       
    }
}
