using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Data.Repositories
{
    public class ExpenseRepository :  IExpenseRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public ExpenseRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public void Delete(Expense obj)
        {
            _context.Expenses.Remove(obj);
            _context.SaveChanges();
        }

        public List<Expense> FindExpensesByCategoryId(string userId, int expenseCategoryId)
        {
            List<Expense> expenses = new List<Expense>();
            expenses = _context.Expenses
                        .Where(x => x.AppUserId == userId && x.ExpenseCategoryId == expenseCategoryId
                        && x.DateOfExpense.ToString("MMMM") == DateTime.Now.ToString("MMMM"))
                        .ToList();
            return expenses;
        }

        public IEnumerable<Expense> Get(Expression<Func<Expense, bool>> filter = null, Func<IQueryable<Expense>, IOrderedQueryable<Expense>> orderBy = null, string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Expense> GetAll(string userId)
        {
            List<Expense> result = new List<Expense>();
            result = _context.Expenses
                .Where(x => x.AppUserId == userId)
                .ToList();
            return result;
        }

        public IEnumerable<Expense> GetExpensesPerTimePeriod(string userId, DateTime start, DateTime end)
        {
            var expenses = _context.Expenses
                        .Where(x => x.AppUserId == userId 
                         && x.DateOfExpense.Date >= start.Date && x.DateOfExpense.Date < end.AddDays(1).Date)
                        .OrderByDescending(c => c.DateOfExpense).ToList();
            return expenses;
        }

        public IEnumerable<Expense> GetExpensesPerTimePeriodPerCategory(string userId, int categoryId, DateTime start, DateTime end)
        {
            var expenses = _context.Expenses
                       .Where(x => x.AppUserId == userId && x.ExpenseCategoryId == categoryId
                        && x.DateOfExpense.Date >= start.Date && x.DateOfExpense.Date < end.AddDays(1).Date)
                       .OrderByDescending(c => c.DateOfExpense).ToList();
            return expenses;
        }

        public Expense GetById(int id)
        {
            return _context.Expenses.Find(id);
        }

        public void Insert(Expense obj)
        {
            _context.Expenses.Add(obj);
            _context.SaveChanges();
        }
        

        public void Update(Expense obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}
