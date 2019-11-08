﻿using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using Microsoft.AspNetCore.Identity;
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
        private readonly IExpenseCategoryService _expenseCategoryService;

        public AdminCategoryService(IAdminCategoryRepository adminCategoryRepository, 
            UserManager<AppUser> userManager,
            IExpenseCategoryService expenseCategoryService)
        {
            _adminCategoryRepository = adminCategoryRepository;
            _userManager = userManager;
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            _expenseCategoryService.SaveChanges();
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