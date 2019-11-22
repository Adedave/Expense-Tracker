using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data;
using ExpenseTracker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ExpenseTracker.Biz.IServices;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Controllers
{
    [Authorize]
    public class ExpensesCategoryController : Controller
    {
        private readonly IExpenseCategoryService _expenseCategoryService;
        private readonly ExpenseTrackerDbContext _context;
        private readonly ILogger<ExpensesCategoryController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBudgetService _budgetService;

        public ExpensesCategoryController(IExpenseCategoryService expenseCategoryService,
            ExpenseTrackerDbContext context, ILogger<ExpensesCategoryController> logger,
            UserManager<AppUser> userManager, 
            IBudgetService budgetService)
        {
            _expenseCategoryService = expenseCategoryService;
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _budgetService = budgetService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();
            List<ExpenseCategory> categoryList = new List<ExpenseCategory>();
            categoryList = _expenseCategoryService.GetCategories(user.Id);
            return View(categoryList);
        }
        
        public async Task<IActionResult> CategoryDetails(int id, string month = null, string year = null)
        {
            month = month ?? DateTime.Now.ToString("MMMM");
            year = year ?? DateTime.Now.Year.ToString();
            var user = await GetCurrentUser();

            var category = _expenseCategoryService.GetExpenseCategoryWithCurrentMonthExpenses(user.Id,id,month,year);
            if (category == null)
            {
                return NotFound("Category not found");
            }

            var budget = await GetCategoryBudget(id,month,year);

            DateTime currentMonth = Convert.ToDateTime(month + year);
            ViewBag.CurrentMonth = currentMonth;
            ViewBag.PreviousMonth = currentMonth.AddMonths(-1);
            ViewBag.NextMonth = currentMonth.AddMonths(1);
            ViewBag.Budget = budget?.Amount.ToString("0,0.00");

            return View(category);
        }

        private async Task<Budget> GetCategoryBudget(int id,string month, string year)
        {
            var user = await GetCurrentUser();
            return _budgetService.GetByCategory(id,user.Id,month,year);
        }

        public async Task<IActionResult> GetCategory(int id)
        {

            DateTime dateTime = DateTime.Now;
            var user = await GetCurrentUser();
            var cat = _context.ExpenseCategories.Find(id);
            //var category = _expenseCategoryService.GetCategoryById(id);
            return Json(cat.Name);
        }

        //public async Task<ExpenseCategory> FindCategory(int id)
        //{
        //    var user = await GetCurrentUser();
        //    var expenseCategory = _context.ExpenseCategories
        //                    .Include(x => x.Expenses)
        //                    .Where(x => x.AppUserId == user.Id)
        //                    .SingleOrDefault(x => x.Id == id);
        //    return expenseCategory;
        //}

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }
        

        [HttpPost]
        public async Task<IActionResult> AddCategory(string name)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //what happens when the user is null
            //this is an authorized controller, user cannot be null
            var user = await GetCurrentUser();

            _expenseCategoryService.AddCategory(name,user.Id);

            TempData["Message"] = $"Category \"{name}\" created successfully!";
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public IActionResult UpdateCategory(int id)
        {
            var category = _expenseCategoryService.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateCategory(ExpenseCategory category)
        {
            if (category == null || !ModelState.IsValid)
            {
                return View(category);
            }
            var user = await GetCurrentUser();
            //Which is a better practice, send the appUserId to the view or
            // set here in the controller with the HttpContext property
            //category.AppUserId = user.Id;
            _expenseCategoryService.UpdateCategory(category, user.Id);
           
            TempData["Message"] = $"Category \"{category.Name}\" updated successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult DeleteCategory(int id)
        {
            try
            {
                var category = _expenseCategoryService.GetCategoryById(id);
                if (category == null)
                {
                    return NotFound();
                }
                _expenseCategoryService.DeleteCategory(category);
                TempData["Message"] = $"Category \"{category.Name}\" deleted successfully!";

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
            return RedirectToAction("Index");
        }

        public async Task<List<CategoryDetailsViewModel>> GetCategoryDetailsModel(string month, string year)
        {
            List<CategoryDetailsViewModel> categoryDetails = new List<CategoryDetailsViewModel>();

            var user = await GetCurrentUser();

            if (string.IsNullOrEmpty(month) || string.IsNullOrWhiteSpace(month))
            {
                month = DateTime.Now.ToString("MMMM");
            }
            
            categoryDetails = CreateCategoryDetailsViewModels(month,year,user.Id);

            return categoryDetails;
        }

        private async Task<AppUser> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            return user;
        }

        private List<CategoryDetailsViewModel> CreateCategoryDetailsViewModels(string month, string year, string userId)
        {
            var categoryList = _expenseCategoryService.GetExpenseCategoriesWithCurrentMonthExpenses(userId,month,year);

            var budgetList = _budgetService.GetCurrentMonthBudgets(userId,month,year);

            List<CategoryDetailsViewModel> categoryDetails = new List<CategoryDetailsViewModel>();

            foreach (var item in categoryList)
            {
                //you can do this search at the database level instead of fetching all the data from the database
                var budget = budgetList.SingleOrDefault(budg => budg.ExpenseCategoryId == item.Id);
                budget = budget ?? new Budget();
                
                categoryDetails.Add(
                 new CategoryDetailsViewModel
                 {
                     CategoryId = item.Id,
                     CategoryName = item.Name,
                     CostOfTOtalExpenses = item.Expenses.Sum(x => x.CostOfExpense),
                     Budget = budget.Amount
                 });
            }
            return categoryDetails;
        }
    }
}
