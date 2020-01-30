using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Biz.IServices;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly IExpenseCategoryService _expenseCategoryService;
        private readonly UserManager<AppUser> _userManager;

        public BudgetController(IBudgetService budgetService,
             IExpenseCategoryService expenseCategoryService, 
             UserManager<AppUser> userManager)
        {
            _budgetService = budgetService;
            _expenseCategoryService = expenseCategoryService;
            _userManager = userManager;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index(string month = null, string year =null)
        {
            var user = await GetCurrentUser();
            
            month = month ?? DateTime.Now.ToString("MMMM");
            year = year ?? DateTime.Now.Year.ToString();

            var budgets = _budgetService.GetCurrentMonthBudgets(user.Id, month,year);

            DateTime currentMonth = Convert.ToDateTime(month+year);
            ViewBag.CurrentMonth = currentMonth;
            ViewBag.PreviousMonth = currentMonth.AddMonths(-1);
            ViewBag.NextMonth = currentMonth.AddMonths(1);

            return View(budgets);
        }

        public async Task<IActionResult> BudgetByMonth(string month)
        {
            var user = await GetCurrentUser();

            var budgets = _budgetService.GetCurrentMonthBudgets(user.Id,month,"2019");

            return View(budgets);
        }
        
        public async Task<IActionResult> CreateBudget(int categoryId,string month, string year)
        {
            var user = await GetCurrentUser();

            Budget budget = new Budget()
            {
                AppUserId = user.Id,
                Month = month ?? DateTime.Now.ToString("MMMM"),
                Year = year ?? DateTime.Now.Year.ToString(),
                ExpenseCategoryId = categoryId != 0 ? categoryId : 0
            };
           
            ViewBag.Years = CalculateYears();
            ViewBag.CategoryList = _expenseCategoryService.GetCategories(user.Id);

            return View(budget);
        }

        private List<string> CalculateYears()
        {
            List<string> years = new List<string>();
            int year = DateTime.Now.Year;

            for (int intCount = 0; intCount < 10; intCount++)
            {
                years.Add((year+intCount).ToString());

            }
            return years;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBudget(Budget budget)
        {
            var user = await GetCurrentUser();
            budget.AppUserId = user.Id;

            ViewBag.CategoryList = _expenseCategoryService.GetCategories(user.Id);
            ViewBag.Years = CalculateYears();

            if (!ModelState.IsValid)
            {
                return View(budget);
            }
            if (_budgetService.BudgetExists(budget))
            {
                ModelState.AddModelError("", "A budget has already been created for this month. Consider editing it ");
                return View(budget);
            }

            _budgetService.AddBudget(budget);
            TempData["Message"] = $"Budget for \"{budget?.Category?.Name} category\" was created successfully!";
            return RedirectToAction("Index",new { month = budget?.Month, year = budget?.Year});
        }

        [HttpGet]
        public IActionResult UpdateBudget(int id)
        {
            var budget = _budgetService.GetById(id);
            if (budget == null)
            {
                return NotFound();
            }
            return View(budget);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateBudget(Budget budget)
        {
            if (budget == null || !ModelState.IsValid)
            {
                return View(budget);
            }

            var updatedBudget = _budgetService.UpdateBudget(budget);
            
            TempData["Message"] = $"Budget for \"{updatedBudget?.Category?.Name} category\" was updated successfully!";
            return RedirectToAction("Index", new { month = budget.Month, year = budget.Year });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var budget = _budgetService.GetById(id);
            if (budget == null)
            {
                return NotFound();
            }

            _budgetService.DeleteBudget(budget);

            TempData["Message"] = $"Budget for \"{budget?.Category?.Name} category\" was deleted successfully!";
            return RedirectToAction("Index",new { month = budget.Month, year = budget.Year });
        }
        
        public async Task<AppUser> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            return user;
        }
    }
}
