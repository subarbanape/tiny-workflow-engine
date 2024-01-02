using System.Net.Mail;
using TinyWorkflowEngine.Infrastructure;

namespace TinyWorkflowEngine
{
    public class EmailNotifier : INotificationProvider
    {
        string[] emailWhitelist;

        public EmailNotifier(string[] emailWhitelist) => 
            this.emailWhitelist = emailWhitelist;

        public bool Notify(WorkflowNotificationRequest request)
        {
            if (request == null) throw new NotifyTemplateInputEmptyException();

            string recipients = string.Join(",", FilterWhitelistEmails(request.ToAddresses));
            string ccRecipients = string.Join(",", FilterWhitelistEmails(request.CCAddresses));
            //string recipients = "naren@terminalcontacts.com,Vinayaka.amaresh@gvtc.net";
            //string ccRecipients = "naren@terminalcontacts.com,Vinayaka.amaresh@gvtc.net";
            string subject = request.Subject;
            string body = request.Content;
            if ((ccRecipients == null || ccRecipients.Count() == 0) &&
                (recipients == null || recipients.Count() == 0))
            {
                return true;
            }
            using (SmtpClient smtpClient = new SmtpClient())
            {
                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(request.From.Email),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                if(ccRecipients !=null && ccRecipients.Length > 0) mailMessage.CC.Add(ccRecipients);
                if(recipients != null && recipients.Length > 0) mailMessage.To.Add(recipients);
               
                smtpClient.Send(mailMessage);
            }

            return true;
        }
        string[] FilterWhitelistEmails(WorkflowUser[] users)
        {
            if (users == null || users.Length == 0) return new string[] { };

            if (emailWhitelist == null || emailWhitelist.Length == 0)
                return users?.Select(item => item.Email).Distinct()?.ToArray();
            else 
            {
                var whitelistedUsersOnly = new List<string>();
                foreach(var user in users)
                {
                    var email = user.Email.ToLower();
                    if (emailWhitelist?.ToList()?.FirstOrDefault(item => item.Equals(email, StringComparison.OrdinalIgnoreCase)) != null)
                        whitelistedUsersOnly.Add(email);
                }
                return whitelistedUsersOnly.ToArray();
            }

            return new string[] { };
        }
    }
}
