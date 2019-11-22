using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.IRepositories
{
    public interface IExpenseCategoryRepository: IRepository<ExpenseCategory>
    {
        void InsertWithoutSaveChanges(ExpenseCategory expenseCategory);
        void SaveChanges();
        IEnumerable<ExpenseCategory> GetExpenseCategories(string userId);
        IEnumerable<ExpenseCategory> GetExpenseCategoriesWithExpenses(string userId);
        IEnumerable<ExpenseCategory> GetExpenseCategoriesWithCurrentMonthExpenses(string userId, string month, string year);
        ExpenseCategory GetExpenseCategoryWithCurrentMonthExpenses(string userId, int categoryId, string month, string year);
    }
}
