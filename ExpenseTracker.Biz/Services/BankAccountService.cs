using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExpenseTracker.Biz.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly ILogger<BankAccountService> _logger;

        public BankAccountService(IBankAccountRepository bankAccountRepository,
            ILogger<BankAccountService> logger)
        {
            _bankAccountRepository = bankAccountRepository;
            _logger = logger;
        }

        public List<BankAccount> GetBankAccounts(string userId)
        {
            return _bankAccountRepository.GetAll(userId).ToList();
        }
        
        public bool RegisterBankAccountCheckIfExists(string accountNumber, string userId)
        {
            bool IsExists = false;
            var existingBankAccount = _bankAccountRepository.GetByAccountNumberAndUserId(accountNumber,userId);
            if (existingBankAccount != null)
            {
                IsExists = true;
            }
            return IsExists;
        }

        public bool UpdateBankAccountCheckIfExists(int bankAccountId, string accountNumber, string userId)
        {
            bool IsExists = false;
            try
            {
                var existingBankAccount = _bankAccountRepository.GetByAccountNumberAndUserId(accountNumber,userId);
                
                //If bankAccountId of existingBankAccount is same as what was gotten from the database then we know 
                //it is still the same bank account we are working with. But if not, we know the new account number 
                //has been registered before by this userId
                if (existingBankAccount != null && existingBankAccount?.BankAccountId != bankAccountId)
                {
                    IsExists = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogCritical(ex.StackTrace);
            }
            return IsExists;
        }

        public void AddBankAccount(BankAccount bankAccount)
        {
            //check if BankAccount already exists before saving it
            //CHECK already done in the controller
           _bankAccountRepository.Insert(bankAccount);
        }

        public void DeleteBankAccount(BankAccount bankAccount)
        {
            _bankAccountRepository.Delete(bankAccount);
        }

        public BankAccount GetById(int id)
        {
            return _bankAccountRepository.GetById(id);
        }

        public void UpdateBankAccount(BankAccount BankAccount)
        {
            _bankAccountRepository.Update(BankAccount);
        }

        public void SetIsConnectedProperty(int bankAccountId)
        {
            var bankAccount = GetById(bankAccountId);
            if (bankAccount != null)
            {
                bankAccount.IsConnnected = true;
                _bankAccountRepository.Update(bankAccount);
            }
        }

        public void SetAboutToConnectProperty(string accountNumber, string userId)
        {
            var bankAccount = _bankAccountRepository.GetByAccountNumberAndUserId(accountNumber,userId);
            if (bankAccount != null)
            {
                bankAccount.AboutToConnect = true;
                _bankAccountRepository.Update(bankAccount);
            }
        }
    }
}
