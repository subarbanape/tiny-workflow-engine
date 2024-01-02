using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyWorkflowEngine.Infrastructure
{
    public interface INotifyWorkflowTemplate
    {
        WorkflowNotificationRequest GetNewTaskAssignTemplate(Workflow workflow, WorkflowTask task);
        WorkflowNotificationRequest GetNewWorkflowtNotifyGroupTemplate(Workflow workflow, WorkflowTask task);
        WorkflowNotificationRequest GetCompleteTaskTemplate(Workflow workflow, WorkflowTask task);
        WorkflowNotificationRequest GetCompleteWorkflowTemplate(Workflow workflow);
      
    }
    
}
