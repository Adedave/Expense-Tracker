using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Common;
using ExpenseTracker.Common.EmailModels;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Biz.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IViewRenderService _viewRenderService;
        private readonly IEmailService _emailService;

        public BudgetService(IBudgetRepository budgetRepository,
            IViewRenderService viewRenderService, IEmailService emailService)
        {
            _budgetRepository = budgetRepository;
            _viewRenderService = viewRenderService;
            _emailService = emailService;
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

        public async Task SendBudgetExceededMail(string userName, string email, string budgetStatus, string category)
        {
            try
            {
                string message = "";

                BudgetEmailModel budgetEmailModel = new BudgetEmailModel()
                {
                    UserName = userName,
                    HeadingA = "Budget for",
                    HeadingB = budgetStatus.Contains("exceeded") ? $"{category}" + " Exceeded": $"{category}" + " Reached",
                    Message = budgetStatus
                };

                message = await _viewRenderService.RenderToStringAsync("BudgetEmailMessage", budgetEmailModel);

                await _emailService.BudgetExceeded(email, message, category);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }
    }
}
