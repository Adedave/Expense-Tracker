using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Biz.IServices
{
    public interface IExpenseService
    {
        Dictionary<string, string> CheckBugdetLimit(string userId, int expenseCategoryId,DateTime dateTime);
        Expense GetById(int id);
        IEnumerable<Expense> GetExpensesPerTimePeriod(string userId,int categoryId, string timePeriod, string end);
        DateTime SetTimePeriod(string timePeriod, DateTime endPeriod);
        void AddExpense(Expense expense);
        void UpdateExpense(Expense expense);
        void DeleteExpense(Expense expense);
        Task SendMonthlyReport();
        Task TestEmail();
    }
}
