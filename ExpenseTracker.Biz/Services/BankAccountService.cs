using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpenseTracker.Biz.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IBankAccountRepository _bankAccountRepository;

        public BankAccountService(IBankAccountRepository bankAccountRepository)
        {
            _bankAccountRepository = bankAccountRepository;
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
            var existingBankAccount = _bankAccountRepository.GetByAccountNumberAndUserId(accountNumber,userId);
            if (existingBankAccount.BankAccountId != bankAccountId)
            {
                IsExists = true;
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
            bankAccount.IsConnnected = true;
            _bankAccountRepository.Update(bankAccount);
        }

        public void SetAboutToConnectProperty(string accountNumber)
        {
            var bankAccount = _bankAccountRepository.GetByAccountNumber(accountNumber);
            bankAccount.AboutToConnect = true;
            _bankAccountRepository.Update(bankAccount);
        }
    }
}
