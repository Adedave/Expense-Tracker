using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ExpenseTracker.Biz.Services
{
    public class ExpenseCategoryService : IExpenseCategoryService
    {
        private readonly IExpenseCategoryRepository _expenseCategoryRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAdminCategoryRepository _adminCategoryRepository;

        public ExpenseCategoryService(IExpenseCategoryRepository expenseCategoryRepository,
            UserManager<AppUser> userManager, IAdminCategoryRepository adminCategoryRepository)
        {
            _expenseCategoryRepository = expenseCategoryRepository;
            _userManager = userManager;
            _adminCategoryRepository = adminCategoryRepository;
        }

        public void AddCategory(string name, string userId)
        {
            ExpenseCategory category = new ExpenseCategory()
            {
                Name = name,
                DateCreated = DateTime.Now,
                AppUserId = userId
            };
            _expenseCategoryRepository.Insert(category);
        }

        public void AddCategoryWithoutSaveChanges(string name, string userId)
        {
            ExpenseCategory category = new ExpenseCategory()
            {
                Name = name,
                DateCreated = DateTime.Now,
                AppUserId = userId
            };
            _expenseCategoryRepository.InsertWithoutSaveChanges(category);
        }

        public void SaveChanges()
        {
            _expenseCategoryRepository.SaveChanges();
        }

        private void CopyAdminCategories(string userId)
        {
            var adminCategories = _adminCategoryRepository.GetAdminCategories();
            foreach (var category in adminCategories)
            {
                AddCategory(category.Name, userId);
            }
        }

        public List<ExpenseCategory> GetExpenseCategoriesWithExpenses(string userId)
        {
            return _expenseCategoryRepository.GetExpenseCategoriesWithExpenses(userId);
        }

        public void UpdateCategory(ExpenseCategory category, string userId)
        {
            category.AppUserId = userId;
            _expenseCategoryRepository.Update(category);
        }

        public void DeleteCategory(ExpenseCategory category)
        {
            _expenseCategoryRepository.Delete(category);
        }

        public List<ExpenseCategory> GetCategories(string userId)
        {
            var categories = _expenseCategoryRepository.GetExpenseCategories(userId);
            if (categories.Count == 0)
            {
                CopyAdminCategories(userId);
                return _expenseCategoryRepository.GetExpenseCategories(userId);
            }
            return categories;
        }

        //didnt pass the userId because the user can only see what is his in the first place
        public ExpenseCategory GetCategoryById(int id)
        {
            return _expenseCategoryRepository.GetById(id);
        }

        public List<ExpenseCategory> GetExpenseCategoriesWithCurrentMonthExpenses(string userId, string month, string year)
        {
            return _expenseCategoryRepository.GetExpenseCategoriesWithCurrentMonthExpenses(userId, month, year);
        }
        public ExpenseCategory GetExpenseCategoryWithCurrentMonthExpenses(string userId,int categoryId, string month, string year)
        {
            return _expenseCategoryRepository.GetExpenseCategoryWithCurrentMonthExpenses(userId, categoryId, month, year);
        }
    }
}
