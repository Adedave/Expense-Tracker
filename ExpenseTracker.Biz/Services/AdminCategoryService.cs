using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Biz.Services
{
    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly IAdminCategoryRepository _adminCategoryRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AdminCategoryService> _logger;
        private readonly IExpenseCategoryService _expenseCategoryService;

        public AdminCategoryService(IAdminCategoryRepository adminCategoryRepository, 
            UserManager<AppUser> userManager, ILogger<AdminCategoryService> logger,
            IExpenseCategoryService expenseCategoryService)
        {
            _adminCategoryRepository = adminCategoryRepository;
            _userManager = userManager;
            _logger = logger;
            _expenseCategoryService = expenseCategoryService;
        }

        public async Task AddCategory(string name, string userId)
        {
            try
            {
                AdminExpenseCategory category = new AdminExpenseCategory()
                {
                    Name = name,
                    DateCreated = DateTime.Now,
                    AppUserId = userId
                };
                var appUsers = await _userManager.GetUsersInRoleAsync("Users");
                List<string> idList = new List<string>();
                foreach (var item in appUsers)
                {
                    idList.Add(item.Id);
                }
                _adminCategoryRepository.Insert(category);
                AddCategoryForUsers(category.Name,idList);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogCritical(ex.StackTrace);
            }
        }

        private void AddCategoryForUsers(string catName, List<string> idList)
        {
            try
            {
                AdminExpenseCategory category = new AdminExpenseCategory();
                //category = GetCategoryById(catId);
                //List<AppUser> appUsersList = appUsers.ToList();
                foreach (var item in idList)
                {
                    _expenseCategoryService.AddCategoryWithoutSaveChanges(catName, item);
                }
                _expenseCategoryService.SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogCritical(ex.StackTrace);
            }
        }

        public void DeleteCategory(AdminExpenseCategory category)
        {
            _adminCategoryRepository.Delete(category);
        }

        public List<AdminExpenseCategory> GetCategories()
        {
            return _adminCategoryRepository.GetAdminCategories().ToList();
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
