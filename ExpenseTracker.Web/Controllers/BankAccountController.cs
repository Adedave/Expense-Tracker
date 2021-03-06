﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data;
using ExpenseTracker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Biz.IServices;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Controllers
{
    [Authorize]
    public class BankAccountController : Controller
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBankAccountService _bankAccountService;

        public BankAccountController(ExpenseTrackerDbContext context, UserManager<AppUser> userManager,
            IBankAccountService bankAccountService)
        {
            _context = context;
            _userManager = userManager;
            _bankAccountService = bankAccountService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();
            var bankAccounts = _bankAccountService.GetBankAccounts(user.Id);
            //logic to map BankAccount to BankAccountViewModel could go here
            return View(bankAccounts);
        }

        [HttpGet]
        public async Task<IActionResult> RegisterAccount()
        {
            var user = await GetCurrentUser();
            BankAccountViewModel bankAccount = new BankAccountViewModel
            {
                AppUserId = user.Id
            };
            return View(bankAccount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAccount(BankAccountViewModel bankAccount)
        {
            if (!ModelState.IsValid)
            {
                return View(bankAccount);
            }
            if (!bankAccount.AlertEmail.EndsWith("@gmail.com",StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Kindly provide a gmail account for your bank alert email");
                return View(bankAccount);
            }
            var user = await GetCurrentUser();
            bool IsExists = _bankAccountService.RegisterBankAccountCheckIfExists
                                                (bankAccount.AccountNumber, bankAccount.AppUserId);
            if (IsExists)
            {
                ModelState.AddModelError("", "This Account number has already been registered");
                return View(bankAccount);
            }
            BankAccount account = new BankAccount()
            {
                AccountNumber = bankAccount.AccountNumber,
                AlertEmail = bankAccount.AlertEmail,
                AppUserId = bankAccount.AppUserId,
                BankName = bankAccount.BankName
            };
            _bankAccountService.AddBankAccount(account);
            return RedirectToAction("GoogleOAuth", "Email",new { accountNumber = account.AccountNumber, userId = account.AppUserId });
        }

        [HttpGet]
        public async Task<IActionResult> UpdateAccount(int id)
        {
            var user = await GetCurrentUser();
            var bankAccount = _bankAccountService.GetById(id);
            if (bankAccount != null)
            {
                BankAccountViewModel viewModel = new BankAccountViewModel
                {
                    AppUserId = user.Id,
                    BankAccountId = bankAccount.BankAccountId,
                    AccountNumber = bankAccount.AccountNumber,
                    PreviousAccountNumber = bankAccount.AccountNumber,
                    AlertEmail = bankAccount.AlertEmail,
                    PreviousAlertEmail = bankAccount.AlertEmail,
                    BankName = bankAccount.BankName,
                    IsConnnected = bankAccount.IsConnnected
                };
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAccount(BankAccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (!viewModel.AlertEmail.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Kindly provide a gmail account for your bank alert email");
                return View(viewModel);
            }

            bool IsExists = _bankAccountService.UpdateBankAccountCheckIfExists
                                            (viewModel.BankAccountId,viewModel.AccountNumber, viewModel.AppUserId);
            if (IsExists)
            {
                ModelState.AddModelError("", "This Account number has already been registered");
                return View(viewModel);
            }
            var bankAccount = _bankAccountService.GetById(viewModel.BankAccountId);
            if (bankAccount != null)
            {
                //bankAccount.BankAccountId = viewModel.BankAccountId;
                bankAccount.AccountNumber = viewModel.AccountNumber;
                bankAccount.AlertEmail = viewModel.AlertEmail;
                bankAccount.AppUserId = viewModel.AppUserId;
                bankAccount.BankName = viewModel.BankName;
                bankAccount.IsConnnected = (viewModel.PreviousAccountNumber == viewModel.AccountNumber
                                     && viewModel.PreviousAlertEmail == viewModel.AlertEmail) ? viewModel.IsConnnected : false;

                _bankAccountService.UpdateBankAccount(bankAccount);

                if (viewModel.PreviousAlertEmail != viewModel.AlertEmail)
                {
                    return RedirectToAction("GoogleOAuth", "Email", bankAccount.AccountNumber);
                }
                TempData["Message"] = $"Bank Account \"{viewModel.AccountNumber}\" updated successfully!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {

            var bankAccount = _bankAccountService.GetById(id);

            if (bankAccount == null)
            {
                var user = await GetCurrentUser();
                ModelState.AddModelError("", "Bank account not found");
                return View("Index", _bankAccountService.GetBankAccounts(user.Id));
            }

            _bankAccountService.DeleteBankAccount(bankAccount);
            return RedirectToAction("Index");
        }

        //private bool CheckAccount(string accountNumber, string userId)
        //{
        //    List<BankAccount> bankAccounts = new List<BankAccount>();
        //    bankAccounts = _context.BankAccounts
        //                    .Where(x => x.AppUserId == userId)
        //                    .ToList();
        //    bool IsExists = false;
        //    foreach (var item in bankAccounts)
        //    {
        //        if (item.AccountNumber == accountNumber)
        //        {
        //            IsExists = true;
        //        }
        //    }
        //    return IsExists;
        //}

        public async Task<AppUser> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            return user;
        }
    }
}
