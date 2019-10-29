using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Common.Notification
{
    public class MessageNotification 
    {
        //private readonly INotificationService _emailSvc;
        private readonly IHostingEnvironment _env;

        public MessageNotification(//INotificationService emailSvc,
            IHostingEnvironment env)
        {
            //_emailSvc = emailSvc;
            _env = env;
        }

        //public Task<string> ReadTemplate(MessageTypes messageType)
        //{
        //    string filepath = $@"{ApplicationEnvironment.ApplicationBasePath}App_Data/EmailNotifications";
        //    string htmlPath = $@"{filepath}/_template.html";
        //    string contentPath = $@"{filepath}/{messageType.ToString().ToLower()}.txt";
        //    string html = string.Empty;
        //    string body = string.Empty;

        //    // get global html template
        //    if (File.Exists(htmlPath))
        //        html = File.ReadAllText(htmlPath);
        //    else
        //        return null;

        //    // get specific message content
        //    if (File.Exists(contentPath))
        //        body = File.ReadAllText(contentPath);
        //    else return null;

        //    string msgBody = html.Replace("{body}", body);
        //    return Task.FromResult(msgBody);
        //}

        //public async Task SendAsync(string to, string subject, string body)
        //{
        //    await _emailSvc.SendEmail(to, subject, body);
        //}

        //public async Task SendManyAsync(List<string> to, string subject, string body)
        //{
        //    List<Task> job = new List<Task>();
        //    to.ForEach(num => job.Add(Task.Run(async () => await _emailSvc.SendEmail(num, subject, body))));
        //    await Task.WhenAll(job);
        //}

        //public async Task SendSms(List<string> to, string body)
        //{
        //    List<Task> job = new List<Task>();
        //    to.ForEach(num => job.Add(Task.Run(async () => await _emailSvc.SendSMS(num, body))));
        //    await Task.WhenAll(job);
        //}
    }
}
