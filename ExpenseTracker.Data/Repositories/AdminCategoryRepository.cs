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
    public class AdminCategoryRepository : IAdminCategoryRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public AdminCategoryRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public void Delete(AdminExpenseCategory category)
        {
            _context.AdminExpenseCategories.Remove(category);
            _context.SaveChanges();
        }

       
        public IEnumerable<AdminExpenseCategory> GetCategoriesByUserId(string userId)
        {
            List<AdminExpenseCategory> result = new List<AdminExpenseCategory>();
            result = _context.AdminExpenseCategories
                .Where(x => x.AppUserId == userId)
                .ToList();
            return result;
        }

       
        public AdminExpenseCategory GetById(int id)
        {
            var expenseCategory = _context.AdminExpenseCategories
                            .SingleOrDefault(x => x.Id == id);
            return expenseCategory;

        }

        public List<AdminExpenseCategory> GetAdminCategories()
        {
            var adminCategories = _context.AdminExpenseCategories
                                    .ToList();
            return adminCategories;
        }
        
        public void Insert(AdminExpenseCategory obj)
        {
            _context.AdminExpenseCategories.Add(obj);
            
         //   _context.SaveChanges();
        }

        public void Update(AdminExpenseCategory obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public IEnumerable<AdminExpenseCategory> Get(Expression<Func<AdminExpenseCategory, bool>> filter = null, Func<IQueryable<AdminExpenseCategory>, IOrderedQueryable<AdminExpenseCategory>> orderBy = null, string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AdminExpenseCategory> GetAll(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
