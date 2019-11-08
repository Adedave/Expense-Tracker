using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Biz.IServices
{
    public interface IReminderService
    {
        List<Reminder> GetReminders(string userId);
        Reminder GetById(int id);
        void Addreminder(Reminder reminder);
        void Updatereminder(Reminder reminder);
        void Deletereminder(int id);
        Task SendReminderEmail();
    }
}
