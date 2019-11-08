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
    public class GoogleOAuthRepository : IGoogleOAuthRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public GoogleOAuthRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public void Delete(GoogleAuth obj)
        {
            _context.GoogleAuths.Remove(obj);
            _context.SaveChanges();
        }

        public IEnumerable<GoogleAuth> Get(Expression<Func<GoogleAuth, bool>> filter = null, Func<IQueryable<GoogleAuth>, IOrderedQueryable<GoogleAuth>> orderBy = null, string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GoogleAuth> GetAll(string userId)
        {
            var googleAuths = _context.GoogleAuths
                            .Where(x => x.AppUserId == userId)
                            .ToList();
            return googleAuths;
        }

        public GoogleAuth GetById(int id)
        {
            GoogleAuth googleAuth = new GoogleAuth();
            googleAuth = _context.GoogleAuths.Find(id);
            return googleAuth;
        }

        public GoogleAuth GetGoogleOAuthByEmailAndAccNumber(string emailAddress, string accNumber)
        {
            var oauth = _context.GoogleAuths
                                .LastOrDefault(x => x.Email == emailAddress && x.AccountNumber == accNumber);
            return oauth;
        }

        public void Insert(GoogleAuth obj)
        {
            _context.GoogleAuths.Add(obj);
            _context.SaveChanges();
        }

        public void Update(GoogleAuth obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}
