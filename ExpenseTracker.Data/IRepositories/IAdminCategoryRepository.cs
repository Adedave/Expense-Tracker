using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.IRepositories
{
    public interface IAdminCategoryRepository : IRepository<AdminExpenseCategory>
    {
        IEnumerable<AdminExpenseCategory> GetAdminCategories();
        IEnumerable<AdminExpenseCategory> GetCategoriesByUserId(string userId);
    }
}
