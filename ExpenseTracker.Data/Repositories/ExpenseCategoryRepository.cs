using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Data.Repositories
{
    public class ExpenseCategoryRepository : IExpenseCategoryRepository
    {
        private readonly ExpenseTrackerDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ExpenseCategoryRepository(ExpenseTrackerDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public void Delete(ExpenseCategory category)
        {
            _context.ExpenseCategories.Remove(category);
            _context.SaveChanges();
        }

        public IEnumerable<ExpenseCategory> Get(Expression<Func<ExpenseCategory, bool>> filter = null,
            Func<IQueryable<ExpenseCategory>, IOrderedQueryable<ExpenseCategory>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<ExpenseCategory> query = _context.ExpenseCategories;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public IEnumerable<ExpenseCategory> GetAll(string userId)
        {
            List<ExpenseCategory> result = new List<ExpenseCategory>();
            result = _context.ExpenseCategories
                .Where(x => x.AppUserId == userId)
                .ToList();
            return result;
        }

        private async Task<bool> IsAdmin(string appUserId)
        {
            var appUser = await _userManager.FindByIdAsync(appUserId);
            return await _userManager.IsInRoleAsync(appUser, "Admins");
        }

        public ExpenseCategory GetById(int id)
        {
            var expenseCategory = _context.ExpenseCategories
                            .Include(x => x.Expenses)
                            .FirstOrDefault(x => x.Id == id);
            return expenseCategory;

        }

        public List<ExpenseCategory> GetExpenseCategories(string userId)
        {
            List<ExpenseCategory> result = new List<ExpenseCategory>();
            result = _context.ExpenseCategories
                .Where(x => x.AppUserId == userId)
                .ToList();
            return result;
        }

        public List<ExpenseCategory> GetExpenseCategoriesWithCurrentMonthExpenses(string userId, string month, string year)
        {
            List<ExpenseCategory> result = new List<ExpenseCategory>();
            result = _context.ExpenseCategories
                .Include(x => x.Expenses)
                .Where(x => x.AppUserId == userId)
                .ToList();
            foreach (var item in result)
            {
                item.Expenses = item.Expenses
                    .Where(exp => exp.DateOfExpense.ToString("MMMM") == month
                    && exp.DateOfExpense.Year.ToString() == year)
                    .ToList();
            }
            return result;
        }

        public List<ExpenseCategory> GetExpenseCategoriesWithExpenses(string userId)
        {
            List<ExpenseCategory> result = new List<ExpenseCategory>();
            result = _context.ExpenseCategories
                .Include(x => x.Expenses)
                .Where(x => x.AppUserId == userId)
                .ToList();
            return result;
        }

        public ExpenseCategory GetExpenseCategoryWithCurrentMonthExpenses(string userId, int categoryId, string month, string year)
        {
            ExpenseCategory category = new ExpenseCategory();
            category = _context.ExpenseCategories
                .Include(x => x.Expenses)
                .SingleOrDefault(x => x.AppUserId == userId && x.Id == categoryId);
            category.Expenses = category.Expenses
                .Where(exp => exp.DateOfExpense.ToString("MMMM") == month
                && exp.DateOfExpense.Year.ToString() == year)
                .ToList();
            return category;
        }

        public void Insert(ExpenseCategory obj)
        {
            _context.ExpenseCategories.Add(obj);
            _context.SaveChanges();
        }


        public void Update(ExpenseCategory obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}
