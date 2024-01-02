using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyWorkflowEngine.Infrastructure
{
    public class WorkflowComment 
    {
        public int? Id { get; set; }
        public int WorkflowId { get; set; }
        public string CommentText { get; set; }
        public WorkflowUser CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
