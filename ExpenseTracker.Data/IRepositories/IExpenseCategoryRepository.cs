using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.IRepositories
{
    public interface IExpenseCategoryRepository: IRepository<ExpenseCategory>
    {
        List<ExpenseCategory> GetExpenseCategories(string userId);
        List<ExpenseCategory> GetExpenseCategoriesWithExpenses(string userId);
        List<ExpenseCategory> GetExpenseCategoriesWithCurrentMonthExpenses(string userId, string month, string year);
        ExpenseCategory GetExpenseCategoryWithCurrentMonthExpenses(string userId, int categoryId, string month, string year);
    }
}
