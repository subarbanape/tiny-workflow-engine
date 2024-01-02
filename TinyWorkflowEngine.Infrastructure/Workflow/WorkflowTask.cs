using System;

namespace TinyWorkflowEngine.Infrastructure
{
    public class WorkflowTask
    {
        public int? Id { get; set; }
        public int? WorkflowId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public WorkflowUser CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public WorkflowUser AssignedTo { get; set; }
        public DateTime AssignedDate { get; set; }
        public WorkflowUser CompletedBy { get; set; }
        public DateTime? CompleteDate { get; set; }
        public WorkflowTaskStatus Status { get; set; }
        
    }
}
