using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.IRepositories
{
    public interface IBudgetRepository:IRepository<Budget>
    {
        Budget GetByCategory(string userId, int categoryId, string month, string year);
        List<Budget> GetCurrentMonthBudgets(string userId, string month, string year);
    }
}
