using ExpenseTracker.Biz.IServices;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Data.Repositories;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Data.Domain.Models;
using System.Linq;
using ExpenseTracker.Data.IRepositories;

namespace ExpenseTracker.Biz.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IExpenseCategoryRepository _expenseCategoryRepository;
        private readonly IBudgetRepository _budgetRepository;

        public ExpenseService(IExpenseRepository expenseRepository,
            IExpenseCategoryRepository expenseCategoryRepository,
            IBudgetRepository budgetRepository, 
            UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _expenseRepository = expenseRepository;
            _expenseCategoryRepository = expenseCategoryRepository;
            _budgetRepository = budgetRepository;
        }

        //can this operation be done asyncrounously
        public Dictionary<string, string> CheckBugdetLimit(string userId, int expenseCategoryId, DateTime date)
        {
            var category = _expenseCategoryRepository.GetById(expenseCategoryId);
            Dictionary<string, string> budgetMessage = new Dictionary<string, string>
            {
                { "BudgetStatus", "" },
                {"IsBudgetSet","false" },
                { "Category",$"{category?.Name}"}
            };
            var expenses = _expenseRepository.FindExpensesByCategoryId(userId,expenseCategoryId);

            var totalExpenseCost = expenses.Sum(x => x.CostOfExpense);

            var budget = _budgetRepository.GetByCategory(userId,expenseCategoryId,date.ToString("MMMM"),date.Year.ToString());

            if (budget != null)
            {
                budgetMessage["IsBudgetSet"] = "true";
                string budgetedAmount = budget?.Amount.ToString("0,0.00");
                if (totalExpenseCost > budget?.Amount)
                {
                    string amountExceeded = (totalExpenseCost - budget.Amount).ToString("0,0.00");
                    budgetMessage["BudgetStatus"] = $"Your expenses for this month has exceeded your " +
                        $"budget of \u20A6{budgetedAmount} for {category?.Name} category" +
                        $" by \u20A6{amountExceeded} ";
                }

                if (budget.Amount == totalExpenseCost)
                {
                    budgetMessage["BudgetStatus"] = $"Your expenses for this month has met your budget" +
                       $" of \u20A6{budgetedAmount} for {category?.Name} category";
                }
            }

            return budgetMessage;
        }
        
        public void DeleteExpense(Expense expense)
        {
            _expenseRepository.Delete(expense);
        }
        public void AddExpense(Expense expense)
        {
            _expenseRepository.Insert(expense);
        }

        public void UpdateExpense(Expense expense)
        {
            _expenseRepository.Update(expense);
        }

        public Expense GetById(int id)
        {
            return _expenseRepository.GetById(id);
        }

        public IEnumerable<Expense> GetExpensesPerTimePeriod(string userId,int categoryId,string timePeriod, string end)
        {
            DateTime endPeriod = Convert.ToDateTime(end);
            DateTime startPeriod = SetTimePeriod(timePeriod, endPeriod);
            if (categoryId != 0)
            {
                return _expenseRepository.GetExpensesPerTimePeriodPerCategory(userId, categoryId,startPeriod,endPeriod);
            }
            return _expenseRepository.GetExpensesPerTimePeriod(userId, startPeriod, endPeriod);
        }

        public DateTime SetTimePeriod(string timePeriod, DateTime end)
        {
            end = end.Date;
            DateTime startPeriod;
            if (timePeriod == "Today")
            {
                startPeriod = end.Date;
            }
            //else if (timePeriod == "ThisWeek")
            //{
            //    startPeriod = DateTime.Now.AddDays(-6);
            //}
            else if (timePeriod == "LastWeek")
            {
                startPeriod = end.AddDays(-6);
            }
            else if (timePeriod == "LastTwoWeeks")
            {
                startPeriod = end.AddDays(-13);
            }
            else if (timePeriod == "LastMonth")
            {
                startPeriod = end.AddMonths(-1);
            }
            else
            {
                startPeriod = end.Date.AddMonths(-1);
            }
            return startPeriod;
        }
    }
}
