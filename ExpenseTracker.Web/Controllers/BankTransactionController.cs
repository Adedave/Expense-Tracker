using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data;
using ExpenseTracker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Common;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Controllers
{
    [Authorize]
    public class BankTransactionController : Controller
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IExpenseCategoryService _expenseCategoryService;
        private readonly IExpenseService _expenseService;
        private readonly IEmailService _emailService;

        public BankTransactionController(ExpenseTrackerDbContext context, UserManager<AppUser> userManager
            ,IExpenseCategoryService expenseCategoryService,IExpenseService expenseService, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _expenseCategoryService = expenseCategoryService;
            _expenseService = expenseService;
            _emailService = emailService;
        }
        private void SaveB()
        {
            List<string> vs = new List<string>()
            {
                "Access Bank Plc",
"Fidelity Bank Plc",
"First City Monument Bank Plc",
"First Bank of Nigeria Limited",
"Guaranty Trust Bank Plc",
"Union Bank of Nigeria Plc",
"United Bank for Africa Plc",
"Zenith Bank Plc",
"Citibank Nigeria Limited",
"Ecobank Nigeria Plc",
"Heritage Banking Company Limited",
"Keystone Bank Limited",
"Polaris Bank Limited",
"Stanbic IBTC Bank Plc",
"Standard Chartered",
"Sterling Bank Plc",
"Unity Bank Plc",
"Wema Bank Plc"
            };
            List<Bank> banks = new List<Bank>()
            {

            };
        }
        // GET: /<controller>/
        public async Task<IActionResult> Index(int accountId)
        {
            var user = await GetCurrentUser();

            ViewBag.AccountList = GetAccountList(user.Id);
            ViewBag.BankAccounts = GetBankAccounts(user.Id);

            BankAccount selectedAccount = new BankAccount();


            selectedAccount = _context.BankAccounts.Find(accountId);

            List<BankTransaction> bankTransactions = new List<BankTransaction>();

            // if redirecting from OAuth what default account number will be shown
            if (selectedAccount != null)
            {
                bankTransactions = _context.BankTransactions
                                .Where(x => x.AppUserId == user.Id && x.IsDeleted == false
                                && x.AccountNumber == selectedAccount.AccountNumber)
                                .OrderByDescending(x => x.TransactionDate)
                                .ToList();
            }
            else
            {
                bankTransactions = _context.BankTransactions
                                .Where(x => x.AppUserId == user.Id && x.IsDeleted == false)
                                .OrderByDescending(x => x.TransactionDate)
                                .ToList();
            }
            return View(bankTransactions);
        }

        private List<BankAccount> GetBankAccounts(string userId)
        {
            var accountList = _context.BankAccounts
                                 .Where(x => x.AppUserId == userId)
                                 .ToList();
            return accountList;
        }

        private List<dynamic> GetAccountList(string userId)
        {
            var accountList = _context.BankAccounts
                                 .Where(x => x.AppUserId == userId)
                                 .ToList();
            //to show both account number and bank name
            List<dynamic> dynamicList = new List<dynamic>();
            foreach (var item in accountList)
            {
                dynamicList.Add(
                    new
                    {
                        Id = item.BankAccountId,
                        Name = item.AccountNumber + " - " + item.BankName
                    });
            }
            return dynamicList;
        }

        [HttpGet]
        public async Task<BankTransaction> FindBankTransaction(int id)
        {
            // it might seem redundant to call GetCurrentUser method since the user
            // should only see and select the bank transactions that belong to him, but then
            //it would hurt to check again, would it....anything can happen...Or can it?s
            var user = await GetCurrentUser();
            var bankTransaction = _context.BankTransactions
                            .Where(x => x.AppUserId == user.Id && x.IsDeleted == false)
                            .FirstOrDefault(x => x.BankTransactionId == id);
            return bankTransaction;
        }
        public IActionResult SaveBank()
        {
            
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> RecordExpenseTransaction(int id)
        {
            var bankTransaction = await FindBankTransaction(id);
            if (bankTransaction == null)
            {
                ModelState.AddModelError("","Bank Transaction not found");
                return BadRequest();
            }
            return RedirectToAction("RecordTransaction",bankTransaction);
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
            ViewBag.BankTransactionId = bankTransaction.BankTransactionId;
            return View(expense);
        }

        [HttpPost]
        public async Task<IActionResult> RecordTransaction(Expense bankExpense,int bankTransactionId)
        {

            var user = await GetCurrentUser();

            ViewBag.CategoryList = _expenseCategoryService.GetCategories(user.Id);

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Incomplete Form! Kindly complete the required fields");
                return View(bankExpense);
            }

            bankExpense.AppUserId = user.Id;

            //check if expenses have exceeded budget
            _expenseService.AddExpense(bankExpense);

            SetBankTransactionAsRecorded(bankTransactionId);

            Dictionary<string, string> budgetMessage = CheckBudgetLimit(user, bankExpense);
            if (!string.IsNullOrEmpty(budgetMessage["BudgetStatus"]) &&
                !string.IsNullOrWhiteSpace(budgetMessage["BudgetStatus"]))
            {
                //log if email was sent successfully for example if there is no internet or the mail sending failed for
                //some other reason
                await SendBudgetEmail(user.Email, budgetMessage["BudgetStatus"], budgetMessage["Category"]);
            }
            ViewBag.IsBudgetSet = budgetMessage["IsBudgetSet"] == "true" ? true : false;
            ViewBag.BudgetMessage = budgetMessage["BudgetStatus"];
            ViewBag.Added = $"A new Transaction {bankExpense.NameOfExpense} has been recorded successfully";

            return View(bankExpense);
        }

        private void SetBankTransactionAsRecorded(int bankTransactionId)
        {
            var bankTransaction = _context.BankTransactions.Find(bankTransactionId);

            bankTransaction.IsRecorded = true;

            _context.Entry(bankTransaction).State = EntityState.Modified;

            _context.SaveChanges();
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
        public async Task<IActionResult> RecordIncomeTransaction(int id)
        {
            var bankTransaction = await FindBankTransaction(id);
            if (bankTransaction == null)
            {
                ModelState.AddModelError("", "Bank Transaction not found");
                return BadRequest();
            }
            return RedirectToAction("RecordTransaction", "Income", bankTransaction);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var bankTransaction = await FindBankTransaction(id);
            if (bankTransaction == null)
            {
                ModelState.AddModelError("", "Bank Transaction not found");
                return BadRequest();
            }
            bankTransaction.IsDeleted = true;
            _context.Entry(bankTransaction).State = EntityState.Modified;
            _context.SaveChanges();
            // should i delete the transaction or move it to recycle bin table ...sort of
            return RedirectToAction("Index");
            
        }

        public async Task<AppUser> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            return user;
        }
    }
}
