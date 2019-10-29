using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpenseTracker.Biz.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;

        public BudgetService(IBudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }

        public List<Budget> GetBudgets(string userId)
        {
           return _budgetRepository.GetAll(userId).ToList();
        }

        public List<Budget> GetCurrentMonthBudgets(string userId,string month,string year)
        {
            
            return _budgetRepository.GetCurrentMonthBudgets(userId,month,year).ToList();
        }

        public bool BudgetExists(Budget budget)
        {
            bool IsExists = false;
            var existingBudget = _budgetRepository.GetByCategory(budget.AppUserId, budget.ExpenseCategoryId, budget.Month, budget.Year);
            if (existingBudget != null)
            {
                IsExists = true;
            }
            return IsExists;
        }
        public void AddBudget(Budget budget)
        {
            //check if budget already exists for the selceted category before saving it
            
            _budgetRepository.Insert(budget);
        }

        public void DeleteBudget(Budget budget)
        {
            _budgetRepository.Delete(budget);
        }

        public Budget GetById(int id)
        {
            return _budgetRepository.GetById(id);
        }

        public void UpdateBudget(Budget budget)
        {
            _budgetRepository.Update(budget);
        }

        public Budget GetByCategory(int categoryId, string userId, string month, string year)
        {
            return _budgetRepository.GetByCategory(userId, categoryId, month, year);
        }
    }
}
