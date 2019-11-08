using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.IRepositories
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        List<Expense> FindExpensesByCategoryId(string userId, int expenseCategoryId);
        List<Expense> FindExpensesByCategoryIdByMonth(string userId, int expenseCategoryId, DateTime month);
        IEnumerable<Expense> GetExpensesPerTimePeriod(string userId, DateTime start, DateTime end);
        IEnumerable<Expense> GetExpensesPerTimePeriodPerCategory(string userId, int categoryId, DateTime start, DateTime end);
    }
}
