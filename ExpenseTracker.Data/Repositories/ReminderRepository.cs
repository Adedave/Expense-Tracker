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
    public class ReminderRepository : IReminderRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public ReminderRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }
        
        public IEnumerable<Reminder> Get(Expression<Func<Reminder, bool>> filter = null, Func<IQueryable<Reminder>, IOrderedQueryable<Reminder>> orderBy = null, string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Reminder> GetAllReminders()
        {
            List<Reminder> result = new List<Reminder>();
            result = _context.Reminders
                            .Include(x => x.AppUser)
                            .ToList();
            return result;
        }

        public IEnumerable<Reminder> GetAll(string userId)
        {
            List<Reminder> result = new List<Reminder>();
            result = _context.Reminders
                .Where(x => x.AppUserId == userId)
                .ToList();
            return result;
        }

        public Reminder GetById(int id)
        {
            return _context.Reminders.Find(id);
        }

        public void Insert(Reminder obj)
        {
            _context.Reminders.Add(obj);
            _context.SaveChanges();
        }

        public void Update(Reminder obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(Reminder obj)
        {
            _context.Reminders.Remove(obj);
            _context.SaveChanges();
        }
    }
}
