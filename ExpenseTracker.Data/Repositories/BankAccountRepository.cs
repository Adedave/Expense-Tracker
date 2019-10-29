using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ExpenseTracker.Data.Repositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public BankAccountRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public void Delete(BankAccount obj)
        {
            _context.BankAccounts.Remove(obj);
            _context.SaveChanges();
        }

        public IEnumerable<BankAccount> Get(Expression<Func<BankAccount, bool>> filter = null, Func<IQueryable<BankAccount>, IOrderedQueryable<BankAccount>> orderBy = null, string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BankAccount> GetAll(string userId)
        {
            var bankAccounts = _context.BankAccounts
                            .Where(x => x.AppUserId == userId)
                            .ToList();
            return bankAccounts;
        }

        public BankAccount GetById(int id)
        {
            var bankAccount = _context.BankAccounts.Find(id);
            return bankAccount;
        }

        public BankAccount GetByAccountNumberAndUserId(string bankAccountNumber,string userId)
        {
            var bankAccount = _context.BankAccounts
                            .SingleOrDefault(x => x.AccountNumber == bankAccountNumber 
                            && x.AppUserId == userId);
            return bankAccount;
        }

        public BankAccount GetByAccountNumber(string bankAccountNumber)
        {
            var bankAccount = _context.BankAccounts
                            .SingleOrDefault(x => x.AccountNumber == bankAccountNumber);
            return bankAccount;
        }

        public void Insert(BankAccount obj)
        {
            _context.BankAccounts.Add(obj);
            _context.SaveChanges();
        }

        public void Update(BankAccount obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}
