using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpenseTracker.Biz.Services
{
    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly IAdminCategoryRepository _adminCategoryRepository;

        public AdminCategoryService(IAdminCategoryRepository adminCategoryRepository)
        {
            _adminCategoryRepository = adminCategoryRepository;
        }

        public void AddCategory(string name, string userId)
        {
            AdminExpenseCategory category = new AdminExpenseCategory()
            {
                Name = name,
                DateCreated = DateTime.Now,
                AppUserId = userId
            };
            _adminCategoryRepository.Insert(category);
        }

        public void DeleteCategory(AdminExpenseCategory category)
        {
            _adminCategoryRepository.Delete(category);
        }

        public List<AdminExpenseCategory> GetCategories()
        {
            return _adminCategoryRepository.GetAdminCategories();
        }

        public List<AdminExpenseCategory> GetCategoriesByUserId(string userId)
        {
            return _adminCategoryRepository.GetCategoriesByUserId(userId).ToList();
        }

        public AdminExpenseCategory GetCategoryById(int id)
        {
            return _adminCategoryRepository.GetById(id);
        }

        public void UpdateCategory(AdminExpenseCategory adminCategory, string userId)
        {
            adminCategory.AppUserId = userId;
            _adminCategoryRepository.Update(adminCategory);
        }
    }
}
