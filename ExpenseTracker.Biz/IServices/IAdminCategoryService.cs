using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Biz.IServices
{
    public interface IAdminCategoryService
    {
        List<AdminExpenseCategory> GetCategories();
        List<AdminExpenseCategory> GetCategoriesByUserId(string userId);
        //void AddCategory(string name, string userId);
        Task AddCategory(string name, string userId);
        AdminExpenseCategory GetCategoryById(int id);
        void UpdateCategory(AdminExpenseCategory name, string userId);
        void DeleteCategory(AdminExpenseCategory category);
    }
}
