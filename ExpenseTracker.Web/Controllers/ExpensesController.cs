using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Biz.Services;
using ExpenseTracker.Biz.IServices;
using DevExtreme.AspNet.Data.ResponseModel;
using ExpenseTracker.Common;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Controllers
{
    /*************************************************
            Handles the logic about expenses
    ****************************************************/
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly IExpenseService _expenseService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ExpenseTrackerDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IExpenseCategoryService _expenseCategoryService;

        public ExpensesController(IExpenseService expenseService, 
            ExpenseTrackerDbContext context, IEmailService emailService,
            IExpenseCategoryService expenseCategoryService,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _expenseCategoryService = expenseCategoryService;
            _expenseService = expenseService;
            _userManager = userManager;
        }
        public async Task<IActionResult> GetData(int pageIndex, int pageSize)
        {
            var user = await GetCurrentUser();
            var query = _context.Expenses
                        .Where(x => x.AppUserId == user.Id)
                        .OrderByDescending(c => c.DateOfExpense)
                        .Skip(pageIndex * pageSize)
                        .Take(pageSize);
            //var queryc = (from c in _context.Expenses
            //             orderby c.DateOfExpense descending
            //             select c)
            //             .Skip(pageIndex * pageSize)
            //             .Take(pageSize);
            var expenses = query.ToList();
            return Json(expenses);
        }
        public IActionResult Expenses()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        //Test
        public async Task<IActionResult> SendEmail()
        {
            string subject = "HelloWorld";
            string message = "HelloWorld";
            string receipent = "onidavid97@gmail.com";
            await _emailService.SendMessage(subject,message,receipent);
            return Json("Email sent");
        }

        [HttpGet]
        public async Task<LoadResult> Get(DataSourceLoadOptions loadOptions)
        {
            var user = await GetCurrentUser();
            var expenses = _context.Expenses
                        .Where(x => x.AppUserId == user.Id)
                        .OrderByDescending(c => c.DateOfExpense).ToList();
            return DataSourceLoader.Load(expenses, loadOptions);
        }
        public async Task<IActionResult> Index(string end, string timePeriod = "LastWeek", int categoryId = 0)
        {
            end = end ?? DateTime.Now.ToString();

            DateTime currentEnd = Convert.ToDateTime(end);
            ViewBag.CurrentEnd = currentEnd;
            
            int days = SwitchDays(timePeriod);
            ViewBag.Days = days;
            ViewBag.PreviousEnd = currentEnd.AddDays(-days);
            ViewBag.NextEnd = currentEnd.AddDays(days);
            ViewBag.CurrentTimePeriod = timePeriod;
            ViewBag.CategoryId = categoryId;

            var user = await GetCurrentUser();

            ViewBag.CategoryList = _expenseCategoryService.GetCategories(user.Id);

            var expenses = _expenseService.GetExpensesPerTimePeriod(user.Id,categoryId,timePeriod,end);

            return View(expenses);
        }

        private int SwitchDays(string timePeriod)
        {
            int days = 0;
            switch (timePeriod)
            {
                case "Today":
                    days = 1;
                    break;
                case "LastWeek":
                    days = 7;
                    break;
                case "LastTwoWeeks":
                    days = 14;
                    break;
                case "LastMonth":
                    days = 30;
                    break;
                default:
                    days = 30;
                    break;
            }
            return days;
        }

        [HttpGet("{id}")]
        public async Task<Expense> GetById(int id)
        {
            var user = await GetCurrentUser();
            var expense = _expenseService.GetById(id);
            return expense;
        }

        [HttpGet]
        public async Task<IActionResult> AddExpenses()
        {
            var user = await GetCurrentUser();
            
            Expense expenses = new Expense()
            {
                AppUserId = user.Id
            };

            ViewBag.CategoryList = _expenseCategoryService.GetCategories(user.Id);

            return View(expenses);
        }

        public IActionResult ExpensesInsights()
        {
            return View("Viewer");
        }
        public IActionResult BarChart()
        {
            return View("StatisticsTest");
        }

        public async Task<IActionResult> CategorizeExpenses(string timePeriod,int page=1)
        {
            ViewData["CurrentSort"] = timePeriod;
            // DateTime end;
            //end = SetTimePeriod(timePeriod,out DateTime start);
            //var query = _context.Expenses.Where(x => x.DateOfExpense <= end && x.DateOfExpense >= start);
            //var pagedExpensesList = await PaginatedList<Expenses>.CreateAsync(query, page, 10);
            var user = await GetCurrentUser();

            ViewBag.CategoryList = _expenseCategoryService.GetCategories(user.Id);
            return View();
        }

        public async Task<IActionResult> PreviousExpenses(string timePeriod)
        {
            DateTime end;
            end = SetTimePeriod(timePeriod, out DateTime start);
            //var query = _context.Expenses.Where(x => x.DateOfExpense <= end && x.DateOfExpense >= start);
            //var pagedExpensesList = await PaginatedList<Expenses>.CreateAsync(query, 1, 10);
            var user = await GetCurrentUser();

            ViewBag.CategoryList = _expenseCategoryService.GetCategories(user.Id);
            return View();
        }

        private DateTime SetTimePeriod(string timePeriod, out DateTime start)
        {
            DateTime end;
            if (timePeriod == "OneWeek")
            {
                start = DateTime.Now.AddDays(-7);
                end = DateTime.Now;
            }
            else if (timePeriod == "TwoWeeks")
            {
                start = DateTime.Now.AddDays(-14);
                end = DateTime.Now;
            }
            else if (timePeriod == "OneMonth")
            {
                start = DateTime.Now.AddMonths(-1);
                end = DateTime.Now;
            }
            else
            {
                start = DateTime.Now.AddMonths(-1);
                end = DateTime.Now;
            }
            return end;
        }

        
        public IActionResult RecordTransaction(BankTransaction bankTransaction)
        {
            Expense expense = new Expense
            {
                CostOfExpense = bankTransaction.TransactionAmount,
                DateOfExpense = bankTransaction.TransactionDate,
                NameOfExpense = bankTransaction.Description,
                MoreDescription = bankTransaction.Remarks,
                AppUserId = bankTransaction.AppUserId
            };
            
            return View("AddExpenses",expense);
        }

        [HttpPost]
        public async Task<IActionResult> AddExpenses(Expense expenses)
         {
            var user = await GetCurrentUser();

            ViewBag.CategoryList = _expenseCategoryService.GetCategories(user.Id);

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Incomplete Form! Kindly complete the required fields");
                return View(expenses);
            }

            expenses.AppUserId = user.Id;
            
            //check if expenses have exceeded budget
            _expenseService.AddExpense(expenses);
            
            Dictionary<string, string> budgetMessage = CheckBudgetLimit(user,expenses);
            if (!string.IsNullOrEmpty(budgetMessage["BudgetStatus"]) &&
                !string.IsNullOrWhiteSpace(budgetMessage["BudgetStatus"]))
            {
                //log if email was sent successfully for example if there is no internet or the mail sending failed for
                //some other reason
                await SendBudgetEmail(user.Email, budgetMessage["BudgetStatus"], budgetMessage["Category"]);
            }
            ViewBag.IsBudgetSet = budgetMessage["IsBudgetSet"] == "true" ? true : false;
            ViewBag.BudgetMessage = budgetMessage["BudgetStatus"]; 
            ViewBag.Added = $"A new Expense {expenses.NameOfExpense} has been added successfully";

            return View(expenses);
        }

        private Dictionary<string, string> CheckBudgetLimit(AppUser user, Expense expense)
        {
            return
                 _expenseService.CheckBugdetLimit(user.Id, expense.ExpenseCategoryId, expense.DateOfExpense);
        }

        private async Task SendBudgetEmail(string email, string message, string category)
        {
            await _emailService.BudgetExceeded(email, message, category);
        }

        

        [HttpGet]
        public async Task<IActionResult> UpdateExpenses(int id)
        {
            var user = await GetCurrentUser();

            ViewBag.CategoryList = _expenseCategoryService.GetCategories(user.Id);

            var expenses = await GetById(id);

            if (expenses == null)
            {
                ModelState.AddModelError("", "Selected expense not found!");
                View(expenses);
            }

            return View(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateExpenses(Expense expenses)
        {
            var user = await GetCurrentUser();

            ViewBag.CategoryList = _expenseCategoryService.GetCategories(user.Id);

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Incomplete Form! Kindly complete the required fields");
                return View(expenses);
            }
            
            expenses.AppUserId = user.Id;
            _expenseService.UpdateExpense(expenses);
            Dictionary<string, string> budgetMessage = CheckBudgetLimit(user, expenses);
            if (!string.IsNullOrEmpty(budgetMessage["BudgetStatus"]) &&
                !string.IsNullOrWhiteSpace(budgetMessage["BudgetStatus"]))
            {
                //log if email was sent successfully for example if there is no internet or the mail sending failed for
                //some other reason
                await SendBudgetEmail(user.Email, budgetMessage["BudgetStatus"], budgetMessage["Category"]);
            }
            ViewBag.IsBudgetSet = budgetMessage["IsBudgetSet"] == "true" ? true : false;
            ViewBag.BudgetMessage = budgetMessage["BudgetStatus"];
            ViewBag.Updated = $"Expenses {expenses.NameOfExpense} has been updated successfully";
            return View(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExpenses(int id)
        {
            var expense = await GetById(id);

            if (expense == null)
            {
                return BadRequest();
            }

            _expenseService.DeleteExpense(expense);

            return RedirectToAction("Index");
        }

        public async Task<AppUser> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            return user;
        }
    }
}
