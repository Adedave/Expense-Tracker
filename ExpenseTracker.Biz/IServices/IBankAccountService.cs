using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Biz.IServices
{
    public interface IBankAccountService
    {
        List<BankAccount> GetBankAccounts(string userId);
        BankAccount GetById(int id);
        void AddBankAccount(BankAccount bankAccount);
        void UpdateBankAccount(BankAccount BankAccount);
        void DeleteBankAccount(BankAccount bankAccount);
        bool RegisterBankAccountCheckIfExists(string accountNumber, string userId);
        bool UpdateBankAccountCheckIfExists(int bankAccountId, string accountNumber, string userId);
        void SetIsConnectedProperty(int bankAccountId);
        void SetAboutToConnectProperty(string accountNumber);

    }
}
