
using TinyWorkflowEngine.Infrastructure;

namespace TinyWorkflowEngine
{
    [Serializable]
    public class InvalidWorkflowException : Exception
    {
        public InvalidWorkflowException():base("Workflow doesn't have Id.") { }

    }

    [Serializable]
    public class TaskAlreadyExistException : Exception
    {
        public TaskAlreadyExistException(int taskId) : base($"Task {taskId} already exist.") { }
    }

    [Serializable]
    public class TaskAlreadyCompleteOrCancelledException : Exception
    {
        public TaskAlreadyCompleteOrCancelledException(string name) : base($"Task {name} already complete/cancelled.") { }
    }

    [Serializable]
    public class WorkflowNotActiveException : Exception
    {
        public WorkflowNotActiveException(WorkflowStatus status) : base($"Workflow is not active. Status: ${status}") { }
    }

    [Serializable]
    public class WorkflowAlreadyExistException : Exception
    {
        public WorkflowAlreadyExistException(int workflowId) : base($"Workflow {workflowId} already exist.") { }
    }

    [Serializable]
    public class NotifyTemplateInputEmptyException : Exception
    {
        public NotifyTemplateInputEmptyException() : base($"Notify template input is not supplied.") { }
    }
}
