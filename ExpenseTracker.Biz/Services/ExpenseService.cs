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
using ExpenseTracker.Common;
using System.Diagnostics;

namespace ExpenseTracker.Biz.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IEmailService _emailService;
        private readonly IExpenseCategoryRepository _expenseCategoryRepository;
        private readonly IViewRenderService _viewRenderService;
        private readonly IBudgetRepository _budgetRepository;

        public ExpenseService(IExpenseRepository expenseRepository, IEmailService emailService,
            IExpenseCategoryRepository expenseCategoryRepository, IViewRenderService viewRenderService,
            IBudgetRepository budgetRepository, 
            UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _expenseRepository = expenseRepository;
            _emailService = emailService;
            _expenseCategoryRepository = expenseCategoryRepository;
            _viewRenderService = viewRenderService;
            _budgetRepository = budgetRepository;
        }

        //can this operation be done asyncrounously
        //maybe you should take this method to BudgetService
        public Dictionary<string, string> CheckBugdetLimit(string userId, int expenseCategoryId, DateTime date)
        {
            var category = _expenseCategoryRepository.GetById(expenseCategoryId);
            Dictionary<string, string> budgetDetails = new Dictionary<string, string>
            {
                { "BudgetStatus", "" },
                {"IsBudgetSet","false" },
                { "IsBudgetBelowExpense","true"},
                { "Category",$"{category?.Name}"},
                {"Month",$"{date.ToString("MMMM")}" },
                {"Year",$"{date.Year.ToString()}" },
                {"Total Expenses","" },
                {"BudgetAmount","" }
            };
            var expenses = _expenseRepository.FindExpensesByCategoryIdByMonth(userId,expenseCategoryId, date);

            var totalExpenseCost = expenses.Sum(x => x.CostOfExpense);

            var budget = _budgetRepository.GetByCategory(userId,expenseCategoryId,date.ToString("MMMM"),date.Year.ToString());

            if (budget != null)
            {
                //come back to this later to fix the extra values you added to the dictionary
                budgetDetails["IsBudgetSet"] = "true";
                string budgetedAmount = budget?.Amount.ToString("0,0.00");
                if (totalExpenseCost > budget?.Amount)
                {
                    budgetDetails["IsBudgetBelowExpense"] = "false";
                    string amountExceeded = (totalExpenseCost - budget.Amount).ToString("0,0.00");
                    budgetDetails["BudgetStatus"] = $"Your expenses for {date.ToString("MMMM yyyy")} has exceeded your " +
                        $"budget of \u20A6{budgetedAmount} for {category?.Name} category" +
                        $" by \u20A6{amountExceeded} ";
                }
                else if (budget?.Amount == totalExpenseCost)
                {
                    budgetDetails["IsBudgetBelowExpense"] = "false";
                    budgetDetails["BudgetStatus"] = $"Your expenses for {date.ToString("MMMM yyyy")} has met your budget" +
                       $" of \u20A6{budgetedAmount} for {category?.Name} category";
                }
                else
                {
                    //set to true already at initialization
                    //budgetDetails["IsBudgetBelowExpense"] = "true";
                    string amountRemaining = (budget.Amount - totalExpenseCost).ToString("0,0.00");
                    budgetDetails["BudgetStatus"] = $"You have \u20A6{amountRemaining} remaining of your budgeted \u20A6{budgetedAmount} for {category?.Name} in {date.ToString("MMMM yyyy")}.";
                }

            }

            return budgetDetails;
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

        public IEnumerable<Expense> GetAllExpenses(string userId)
        {
            var expenses = _expenseRepository.GetAll(userId);
            return expenses;
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

        public async Task TestEmail()
        {
            //string message = "";
            //message = await _viewRenderService.RenderToStringAsync("BudgetEmailMessage", null);
            //await _emailService.TestEmail("onidavid97@gmail.com",message);
            await SendMonthlyReport();
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

        public async Task SendMonthlyReport()
        {
            try
            {
                DateTime month = DateTime.Now.AddMonths(-1);
                DateTime firstDayofTheMonth = new DateTime(month.Year, month.Month, 1);
                DateTime lastDayOfTheMOnth = firstDayofTheMonth.AddMonths(1).AddSeconds(-1);

                string message = "";

                List<Expense> monthlyExpenses = new List<Expense>();

                var appUsers = await _userManager.GetUsersInRoleAsync("Users");
                var appUsersList = appUsers.ToList();
                var activeUsers = appUsersList.RemoveAll(x => x.IsActive == false);

                foreach (var user in appUsersList)
                {
                    monthlyExpenses = _expenseRepository.GetExpensesPerTimePeriod(user.Id, start: firstDayofTheMonth, end: lastDayOfTheMOnth).ToList();

                    if (monthlyExpenses.Count == 0) // check if user has no expense for that time period
                    {
                        message = $"You have recorded no expense for {month.ToString("MMMM yyyy")}";
                    }
                    else
                    {
                        message = await _viewRenderService.RenderToStringAsync("MonthlyReport", monthlyExpenses);
                    }
                    message = message.Replace("{date}", month.ToString("MMMM yyyy"));
                    await _emailService.SendMonthlyReport(user.Email, message, month);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
           
        }
        
    }
}
