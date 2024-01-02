using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using TinyWorkflowEngine.Infrastructure;

namespace TinyWorkflowEngine.Repository
{
    public class ProductCatalogWorkflowRepository
    {
        public string GetWorkflowData(int requestId)
        {
            return string.Empty;
        }

        public Workflow GetWorkflow(int requestId) =>
            new DefaultWorkflow { };

        public int AddWorkflowUser(WorkflowUser workflowUser)
        {
            return 1;
        }

        public WorkflowUser GetWorkflowUser(string userName) => new WorkflowUser { };

        public WorkflowUser GetWorkflowUser(int userId) => new WorkflowUser { };

        public WorkflowComment[] GetComments(int workflowId) => new WorkflowComment[] { };

        public int AddNewWorkflow(Workflow workflow)
        {
            return 1;
        }

        public int AddComment(WorkflowComment comment)
        {
            return 1;
        }

        public int AddTask(WorkflowTask task)
        {
            return 1;
        }

        public bool UpdateWorkflowData(int workflowId, string data, int updatedBy)
        {
            return true;
        }

        public bool CompleteTask(int taskId, int completedBy)
        {
            return true;
        }

        public bool CancelTask(int taskId, int cancelledBy)
        {
            return true;
        }

        public bool SetWorkflowStatus(int workflowId, WorkflowStatus workflowStatus, int updatedBy)
        {
            return true;
        }

        public WorkflowTask[] GetTasks(int requestId) => new WorkflowTask[] { };

        public WorkflowTask[] GetTasks(WorkflowTaskStatus status)
        {
            return new WorkflowTask[] { } ;
        }

        public WorkflowTask[] GetTasks(int requestId, WorkflowTaskStatus status) =>
             new WorkflowTask[] { };
            
        public WorkflowTask[] GetTasksByUserId(int userId, WorkflowTaskStatus status) =>
             new WorkflowTask[] { };

        public WorkflowTask[] GetActiveTasks(int requestId) => GetTasks(requestId, WorkflowTaskStatus.Assigned);
        public WorkflowTask[] GetCompleteTasks(int requestId) => GetTasks(requestId, WorkflowTaskStatus.Complete);
        public WorkflowTask[] GetCancelledTasks(int requestId) => GetTasks(requestId, WorkflowTaskStatus.Cancelled);

        public WorkflowTask[] GetTasks(int requestId, string taskName, WorkflowTaskStatus status) =>
             new WorkflowTask[] { };
        
        public WorkflowTask GetTask(int taskId) =>
             new WorkflowTask{ };

        public Workflow GetWorkflowByWorkflowDataKey(int workflowDataKey, WorkflowStatus status) =>
             new DefaultWorkflow { };

        public Workflow[] GetRequests(int userId, WorkflowStatus status) =>
             new DefaultWorkflow[] { };

        public Workflow[] GetRequests(WorkflowStatus status) =>
             new DefaultWorkflow[] { };
    }
}
