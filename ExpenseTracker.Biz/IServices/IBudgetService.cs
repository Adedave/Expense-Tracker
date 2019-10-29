using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Biz.IServices
{
    public interface IBudgetService
    {
        List<Budget> GetBudgets(string userId);
        List<Budget> GetCurrentMonthBudgets(string userId,string month, string year);
        Budget GetById(int id);
        Budget GetByCategory(int categoryId,string userId, string month, string year);
        bool BudgetExists(Budget budget);
        void AddBudget(Budget Budget);
        void UpdateBudget(Budget Budget);
        void DeleteBudget(Budget Budget);
    }
}
