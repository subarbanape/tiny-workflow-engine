using System.Linq;
using TinyWorkflowEngine.Infrastructure;
using TinyWorkflowEngine.Repository;

namespace TinyWorkflowEngine
{
    public class WorkflowEngine
    {
        ProductCatalogWorkflowRepository workflowDataProvider;
        INotifyWorkflowTemplate notifyWorkflowTemplate;
        INotificationProvider[] notificationProviders;
        WorkflowUser userContext;
       
        public WorkflowEngine(INotifyWorkflowTemplate notifyWorkflowTemplate,
            string[] emailWhitelist,
            WorkflowUser userContext)
        {
            this.notifyWorkflowTemplate = notifyWorkflowTemplate;
            notificationProviders = new INotificationProvider[] { new EmailNotifier(emailWhitelist) };
            workflowDataProvider = new ProductCatalogWorkflowRepository();
            var workflowUser = workflowDataProvider.GetWorkflowUser(userContext.UserName);
            int? id = workflowUser?.Id;
            if (workflowUser == null)
            {
                id = workflowDataProvider.AddWorkflowUser(userContext);
            }

            if (id != -1) this.userContext = workflowDataProvider.GetWorkflowUser((int)id);
        }

        void SetUserId(ref WorkflowComment workflowComment)
        {
            if (workflowComment.CreatedBy == null) workflowComment.CreatedBy = userContext;

            if (workflowComment.CreatedBy.Id != null) return;
            var id = workflowDataProvider
                .GetWorkflowUser(workflowComment.CreatedBy.UserName)?.Id;

            if (id == null) id = workflowDataProvider.AddWorkflowUser(workflowComment.CreatedBy);
            workflowComment.CreatedBy.Id = id;
        }

        void SetUserId(ref WorkflowTask workflowTask)
        {
            int? id = null;

            if (workflowTask.CreatedBy != null && workflowTask.CreatedBy.Id == null)
            {
                id = workflowDataProvider.GetWorkflowUser(workflowTask.CreatedBy.UserName)?.Id;
                if (id == null) id = workflowDataProvider.AddWorkflowUser(workflowTask.CreatedBy);
                workflowTask.CreatedBy.Id = id;
            }

            if (workflowTask.AssignedTo != null && workflowTask.AssignedTo.Id == null)
            {
                id = workflowDataProvider.GetWorkflowUser(workflowTask.AssignedTo.UserName)?.Id;
                if (id == null) id = workflowDataProvider.AddWorkflowUser(workflowTask.AssignedTo);
                workflowTask.AssignedTo.Id = id;
            }

            if (workflowTask.CompletedBy != null && workflowTask.CompletedBy.Id == null)
            {
                id = workflowDataProvider.GetWorkflowUser(workflowTask.CompletedBy.UserName)?.Id;
                if (id == null) id = workflowDataProvider.AddWorkflowUser(workflowTask.CompletedBy);
                workflowTask.CompletedBy.Id = id;
            }
        }

        public bool UpdateWorkflowData(int workflowId, string data)
        {
            var workflow = GetRequest(workflowId);
            if (workflow.Id == null) throw new InvalidWorkflowException();
            if (workflow.Status != WorkflowStatus.Active) throw new WorkflowNotActiveException(workflow.Status);
            workflowDataProvider.UpdateWorkflowData(workflowId, data, (int)userContext.Id);
            return true;
        }

        public int UpdateRequest(Workflow workflow)
        {
            if (workflow.Id == null) throw new InvalidWorkflowException();
            if (workflow.Status != WorkflowStatus.Active) throw new WorkflowNotActiveException(workflow.Status);

            // push new comments only
            workflow.Comments?.ToList().ForEach(comment => {
                if (comment.Id == null)
                {
                    comment.WorkflowId = (int)workflow.Id;
                    // we cant trust the caller if he has passed the id or not. 
                    // so, set the id if not available for the passied in user.
                    SetUserId(ref comment);
                    if (!string.IsNullOrEmpty(comment.CommentText))
                        workflowDataProvider.AddComment(comment);
                }
            });

            // push new tasks only
            var newTasks = new List<WorkflowTask>();
            workflow.Tasks?.ToList().ForEach(task => {
                if (task.Id == null)
                {
                    task.WorkflowId = (int)workflow.Id;
                    task.CreatedBy = userContext;
                    SetUserId(ref task);
                    var taskId = workflowDataProvider.AddTask(task);
                    task.Id = taskId;
                    newTasks.Add(task);
                }
            });

            UpdateWorkflowData((int)workflow.Id, workflow.Data);

            // notify tasks
            newTasks?.ToList().ForEach(task => {
                var taskAssignTemplate = notifyWorkflowTemplate.GetNewTaskAssignTemplate(workflow, task);
                notificationProviders?.ToList().ForEach(provider => provider.Notify(taskAssignTemplate));
            });

            return (int)workflow.Id;
        }

        public int InitNewRequest(Workflow workflow)
        {
            workflow.CreatedBy = userContext;
            var workflowId = workflowDataProvider.AddNewWorkflow(workflow);

            // push comments
            workflow.Comments?.ToList().ForEach(comment => {
                if (!string.IsNullOrEmpty(comment.CommentText))
                {
                    comment.WorkflowId = workflowId;
                    // we cant trust the caller if he has passed the id or not. 
                    // so, set the id if not available for the passied in user.
                    SetUserId(ref comment);
                    workflowDataProvider.AddComment(comment);
                }
            });

            // push tasks
            workflow.Tasks?.ToList().ForEach(task => {
                task.WorkflowId = workflowId;
                task.CreatedBy = userContext;
                SetUserId(ref task);
                var taskId = workflowDataProvider.AddTask(task);
                task.Id = taskId;
            });


            // notify tasks
            workflow.Tasks?.ToList().ForEach(task =>
            {
                var taskAssignTemplate = notifyWorkflowTemplate.GetNewTaskAssignTemplate(workflow, task);
                notificationProviders?.ToList().ForEach(provider => provider.Notify(taskAssignTemplate));
            });

            var distinctTasks = workflow.Tasks.ToList().Take(1).ToList();

            distinctTasks.ForEach(task => {
                var taskAssignTemplate = notifyWorkflowTemplate.GetNewWorkflowtNotifyGroupTemplate(workflow, task);
                notificationProviders?.ToList().ForEach(provider => provider.Notify(taskAssignTemplate));
            });
            return workflowId;
        }

        public bool UpdateRequestStatus(int requestId, WorkflowStatus status) =>
            workflowDataProvider.SetWorkflowStatus(requestId, status, (int)userContext.Id);

        public bool CompleteRequest(int requestId, WorkflowUser[] completeRequestNotifyRecipients)
        {
            var request = workflowDataProvider.GetWorkflow(requestId);

            // cancel all active tasks first
            var tasks = GetActiveTasks(requestId);
            tasks?.ToList().ForEach(item => CancelTask((int)item.Id));

            // complete request
            var result = workflowDataProvider.SetWorkflowStatus(requestId, WorkflowStatus.Complete, (int)userContext.Id);

            if (result)
            {
                // notify about process completion to all participants of the process
                // 1 - process creator 
                var usersToNotify = new List<WorkflowUser>();
                usersToNotify.Add(workflowDataProvider.GetWorkflowUser(request.CreatedBy.UserName));

                // 2 - users who completed the tasks 
                tasks = GetTasks(requestId);

                foreach (var task in tasks)
                {
                    if (task.Status == WorkflowTaskStatus.Complete) usersToNotify.Add(task.CompletedBy);
                    else usersToNotify.Add(task.AssignedTo);
                }

                // remove duplicates
                usersToNotify = usersToNotify?.GroupBy(t => t.Id)?.Select(grp => grp.First())?.ToList();

                var template = notifyWorkflowTemplate.GetCompleteWorkflowTemplate(GetRequest(requestId));
                template.ToAddresses = usersToNotify?.ToArray();
                template.CCAddresses = completeRequestNotifyRecipients;
                notificationProviders?.ToList().ForEach(provider => provider.Notify(template));
               
            }

            return result;
        }

        public bool CancelRequest(int requestId) => workflowDataProvider.SetWorkflowStatus(requestId, WorkflowStatus.Cancelled, (int)userContext.Id);

        public bool CompleteTask(int taskId)
        {
            var task = GetTask(taskId);
           
            if (task.Status != WorkflowTaskStatus.Assigned) throw new TaskAlreadyCompleteOrCancelledException(task.Name);

            var result = workflowDataProvider.CompleteTask(taskId, (int)userContext.Id);
            if (result)
            {
                task = GetTask(taskId);
                var request = GetRequest((int)task.WorkflowId);

                // notify about task completion.
                var template = notifyWorkflowTemplate.GetCompleteTaskTemplate(request, task);
                notificationProviders?.ToList().ForEach(provider => provider.Notify(template));


                // there may be other identical tasks which
                // could be assigned to more than one people as the group may have more than one.
                // if so, cancel all those tasks
                var similarTasks = GetTasks((int)request.Id, task.Name, WorkflowTaskStatus.Assigned);
                similarTasks?.ToList().ForEach(item => CancelTask((int)item.Id));
            }
            return result;
        }

        public bool CancelTask(int taskId) => workflowDataProvider.CancelTask(taskId, (int)userContext.Id);

        public int AddComment(WorkflowComment comment)
        {
            if (string.IsNullOrEmpty(comment.CommentText)) return -1;
            SetUserId(ref comment);
            if (comment.CreatedDate == null) comment.CreatedDate = DateTime.Now;
            return workflowDataProvider.AddComment(comment);
        }

        public WorkflowComment[] GetComments(int workflowId) =>
            workflowDataProvider.GetComments(workflowId);

        public int AddTask(WorkflowTask task)
        {
            if (task.WorkflowId == null) throw new InvalidWorkflowException();
            if (task.Id != null) throw new TaskAlreadyExistException((int)task.Id);

            SetUserId(ref task);
            task.Id = workflowDataProvider.AddTask(task);
            var request = GetRequest((int)task.WorkflowId);

            var taskAssignTemplate = notifyWorkflowTemplate.GetNewTaskAssignTemplate(request, task);
            notificationProviders?.ToList().ForEach(provider => provider.Notify(taskAssignTemplate));

            return (int)task.Id;
        }

        public string GetRequestData(int requestId) => workflowDataProvider.GetWorkflowData(requestId);

        public WorkflowTask[] GetTasks(int requestId) =>
            workflowDataProvider.GetTasks(requestId);

        public WorkflowTask[] GetTasks(string userName, WorkflowTaskStatus status)
        {
            var user = workflowDataProvider.GetWorkflowUser(userName);
            var tasks = workflowDataProvider.GetTasksByUserId((int)user.Id, status);
            var workflows = new Dictionary<int, Workflow>();
            var activeTasks = new List<WorkflowTask>();
            foreach (var task in tasks)
            {
                workflows.TryGetValue((int)task.WorkflowId, out Workflow workflow);
                if (workflow == null)
                {
                    workflow = workflowDataProvider.GetWorkflow((int)task.WorkflowId);
                    workflows.Add((int)task.WorkflowId, workflow);
                }

                if (workflow.Status == WorkflowStatus.Active)
                {
                    activeTasks.Add(task);
                }
            }

            return activeTasks.ToArray();
        }

        public WorkflowTask[] GetActiveTasks(int requestId) => 
            workflowDataProvider.GetActiveTasks(requestId);
        public WorkflowTask[] GetCompletedTasks(int requestId) => 
            workflowDataProvider.GetCompleteTasks(requestId);
        public WorkflowTask[] GetCancelledTasks(int requestId) => 
            workflowDataProvider.GetCancelledTasks(requestId);
        public WorkflowTask[] GetTasks(int requestId, string taskName, WorkflowTaskStatus status) =>
            workflowDataProvider.GetTasks(requestId, taskName, status);
        public WorkflowTask[] GetTasks(WorkflowTaskStatus status) =>
             workflowDataProvider.GetTasks(status);
        public WorkflowTask GetTask(int taskId) => workflowDataProvider.GetTask(taskId);

        public DefaultWorkflow GetRequestByWorkflowDataKey(int workflowDataKey, WorkflowStatus status)
        {
            var workflow = workflowDataProvider.GetWorkflowByWorkflowDataKey(workflowDataKey, status);
            if (workflow == null) return null;
            return new DefaultWorkflow
            {
                Id = workflow.Id,
                CreatedBy = workflowDataProvider.GetWorkflowUser(workflow.CreatedBy.UserName),
                CreatedDate = workflow.CreatedDate,
                DefaultWorkflowData = workflow.Data,
                Status = (WorkflowStatus)workflow.Status,
                WorkflowDataKey = workflow.WorkflowDataKey,
            };
        }

        public DefaultWorkflow GetRequest(int requestId)
        {
            if (requestId == -1) return null;
            var workflow = workflowDataProvider.GetWorkflow(requestId);
            return new DefaultWorkflow
            {
                Id = workflow.Id,
                CreatedBy = workflowDataProvider.GetWorkflowUser(workflow.CreatedBy.UserName),
                CreatedDate = workflow.CreatedDate,
                DefaultWorkflowData = workflow.Data,
                Status = (WorkflowStatus)workflow.Status,
                WorkflowDataKey = workflow.WorkflowDataKey,
            };
        }

        public Workflow[] GetRequests(WorkflowStatus status)
        {
            return workflowDataProvider.GetRequests(status);
        }

        public Workflow[] GetRequests(string userName, WorkflowStatus status)
        {
            var user = workflowDataProvider.GetWorkflowUser(userName);
            return workflowDataProvider.GetRequests((int)user.Id, status);
        }
      

    }
}
