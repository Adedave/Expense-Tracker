using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Controllers
{
    public class IncomeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ExpenseTrackerDbContext _context;

        public IncomeController(ExpenseTrackerDbContext context, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RecordTransaction(BankTransaction bankTransaction)
        {
            Income expense = new Income
            {
                Amount = bankTransaction.TransactionAmount,
                DateOfIncome = bankTransaction.TransactionDate,
                IncomeSource = bankTransaction.Description,
                MoreDescription = bankTransaction.Remarks,
                AppUserId = bankTransaction.AppUserId
            };

            ViewBag.CategoryList = await RetrieveAllExpenseCategories();
            return View("AddExpenses", expense);
        }

        public async Task<AppUser> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            return user;
        }
        //context doesnt work in static class
        public async Task<List<IncomeCategory>> RetrieveAllExpenseCategories()
        {
            var user = await GetCurrentUser();
            List<IncomeCategory> result = new List<IncomeCategory>();
            result = _context.IncomeCategories
                .Where(x => x.AppUserId == user.Id)
                .ToList();
            return result;
        }
    }
}
