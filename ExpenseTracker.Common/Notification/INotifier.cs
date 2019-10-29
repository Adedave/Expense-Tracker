using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Common.Notification
{
    public interface INotifier
    {
       // Task<string> ReadTemplate(MessageTypes messageTypes);
        Task SendAsync(string to, string subject, string body);
        Task SendManyAsync(List<string> to, string subject, string body);
        Task SendSms(List<string> to, string body);
    }
}
