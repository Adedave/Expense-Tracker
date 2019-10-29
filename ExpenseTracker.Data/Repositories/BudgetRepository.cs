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
    public class BudgetRepository :  IBudgetRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public BudgetRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        
        public Budget GetByCategory(string userId,int categoryId, string month, string year)
        {
            var budget = _context.Budgets
                            .SingleOrDefault(x => x.ExpenseCategoryId == categoryId
                            && x.Month == month && x.Year == year
                            && x.AppUserId == userId);
            return budget;
        }

        public IEnumerable<Budget> Get(Expression<Func<Budget, bool>> filter = null, Func<IQueryable<Budget>, IOrderedQueryable<Budget>> orderBy = null, string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Budget> GetAll(string userId)
        {
            List<Budget> result = new List<Budget>();
            result = _context.Budgets
                .Include(x => x.Category)
                .Where(x => x.AppUserId == userId)
                .ToList();
            return result;
        }

        public Budget GetById(int id)
        {
            var budget = _context.Budgets.Include(x => x.Category)
                        .SingleOrDefault( x => x.Id == id);
            return budget;
        }

        public void Insert(Budget obj)
        {
            _context.Budgets.Add(obj);
            _context.SaveChanges();
        }
        

        public void Update(Budget obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(Budget budget)
        {
            _context.Budgets.Remove(budget);
            _context.SaveChanges();
        }

        public List<Budget> GetCurrentMonthBudgets(string userId, string month, string year)
        {
            List<Budget> result = new List<Budget>();
            result = _context.Budgets
                .Include(x => x.Category)
                .Where(x => x.AppUserId == userId && x.Month == month && x.Year == year)
                .ToList();
            return result;
        }
    }
}
