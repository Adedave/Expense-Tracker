using System.Threading.Tasks;

namespace ExpenseTracker.Common
{
    public interface IEmailService
    {
        Task<bool> BudgetExceeded(string recipient, string message, string category);
        Task<bool> SendReminderEmail(string recipient, string message);
        Task<bool> ConfirmEmail(string recipient, string cTokenLink);
        Task<bool> ResetPassword(string recipient, string resetPasswordLink);
        Task<bool> SendMessage(string subject, string message, string recipient);
    }
}