using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.IRepositories
{
    public interface IReminderRepository: IRepository<Reminder>
    {
        IEnumerable<Reminder> GetAllReminders();
    }
}
