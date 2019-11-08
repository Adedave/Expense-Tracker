using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Biz.IServices
{
    public interface IExpenseCategoryService
    {
        List<ExpenseCategory> GetCategories(string userId);
        void AddCategory(string name, string userId);
        void AddCategoryWithoutSaveChanges(string name, string userId);
        void SaveChanges();
        //void CopyAdminCategoriesForExistingUsers(List<string> vs);
        ExpenseCategory GetCategoryById(int id);
        void UpdateCategory(ExpenseCategory name, string userId);
        void DeleteCategory(ExpenseCategory category);
        List<ExpenseCategory> GetExpenseCategoriesWithExpenses(string userId);
        List<ExpenseCategory> GetExpenseCategoriesWithCurrentMonthExpenses(string userId, string month, string year);
        ExpenseCategory GetExpenseCategoryWithCurrentMonthExpenses(string userId, int categoryId, string month, string year);
    }
}
