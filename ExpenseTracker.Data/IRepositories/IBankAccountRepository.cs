using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.IRepositories
{
    public interface IBankAccountRepository : IRepository<BankAccount>
    {
        BankAccount GetByAccountNumberAndUserId(string bankAccountNumber, string userId);
        BankAccount GetByAccountNumber(string bankAccountNumber);
    }
}
