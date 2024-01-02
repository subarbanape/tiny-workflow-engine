using TinyWorkflowEngine.Repository;
using TinyWorkflowEngine.Infrastructure;

namespace TinyWorkflowEngine
{
    public static class WorkflowExtensions
    {
        static WorkflowUser GetUser(string user)
        {
            if (string.IsNullOrEmpty(user)) return null;

            return user.Contains('@') ?
                new WorkflowUser { Email = user } :
                new WorkflowUser { UserName = user };
        }

        public static WorkflowUser ToContractType(this WorkflowUser item)
        {
            return new WorkflowUser
            {
                Id = item.Id,
                UserName = item.UserName,
                FullName = item.FullName,
                Email = item.Email
            };
        }

        public static WorkflowComment ToContractType(this WorkflowComment item, ProductCatalogWorkflowRepository workflowDataProvider)
        {
            return new WorkflowComment
            {
                Id = item.Id,
                WorkflowId = (int)item.WorkflowId,
                CommentText = item.CommentText,
                CreatedBy = workflowDataProvider.GetWorkflowUser((int)item.CreatedBy.Id),
                CreatedDate = item.CreatedDate,
            };
        }

        public static WorkflowTask ToContractType(this WorkflowTask item, ProductCatalogWorkflowRepository workflowDataProvider)
        {
            return new WorkflowTask {
                Id = item.Id,
                WorkflowId = item.WorkflowId,
                Name = item.Name,
                DisplayName = item.DisplayName,
                CreatedBy = workflowDataProvider.GetWorkflowUser(item.CreatedBy.UserName),
                CreatedDate = item.CreatedDate,
                AssignedTo = workflowDataProvider.GetWorkflowUser(item.AssignedTo.UserName),
                AssignedDate = item.AssignedDate,
                CompletedBy = item.CompletedBy == null ? null : workflowDataProvider.GetWorkflowUser((int)item.CompletedBy.Id),
                CompleteDate = item.CompleteDate,
                Status = (WorkflowTaskStatus) item.Status,
            };
        }

        public static Workflow ToContractType(this Workflow item, ProductCatalogWorkflowRepository workflowDataProvider)
        {
            return new DefaultWorkflow
            {
                Id = item.Id,
                DefaultWorkflowData = item.Data,
                WorkflowDataKey = item.WorkflowDataKey,
                Status = (WorkflowStatus)item.Status,
                CreatedBy = workflowDataProvider.GetWorkflowUser(item.CreatedBy.UserName),
                CreatedDate = item.CreatedDate,
                UpdatedBy = item.UpdatedBy == null ? null : workflowDataProvider.GetWorkflowUser(item.UpdatedBy.UserName),
                UpdatedDate = item.UpdatedDate == null ? DateTime.MaxValue : (DateTime) item.UpdatedDate,
            };
        }
    }
}
