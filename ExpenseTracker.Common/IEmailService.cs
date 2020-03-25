using System;
using System.Threading.Tasks;

namespace ExpenseTracker.Common
{
    public interface IEmailService
    {
        Task<bool> ConfirmEmail(string recipient, string message);
        Task<bool> ResetPassword(string recipient, string message);
        Task<bool> SendReminderEmail(string recipient, string message);
        Task<bool> BudgetExceeded(string recipient, string message, string category);
        Task<bool> SendMonthlyReport(string recipient, string message, DateTime month);
        Task<bool> TestEmail(string recipient, string message);
        Task<bool> SendMessage(string subject, string message, string recipient);
    }
}