using ExpenseTracker.Data.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ExpenseTracker.Data.Repositories
{
    //public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    //{
    //    private readonly ExpenseTrackerDbContext _context;
    //    private readonly DbSet<TEntity> dbSet;
    //    private string _errorMessage = string.Empty;
    //    public BaseRepository(ExpenseTrackerDbContext context)
    //    {
    //        _context = context;
    //        dbSet = _context.Set<TEntity>();
    //    }

    //    public virtual IEnumerable<TEntity> Get(
    //        Expression<Func<TEntity, bool>> filter = null,
    //        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
    //        string includeProperties = "")
    //    {
    //        IQueryable<TEntity> query = dbSet;

    //        if (filter != null)
    //        {
    //            query = query.Where(filter);
    //        }

    //        foreach (var includeProperty in includeProperties.Split
    //            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
    //        {
    //            query = query.Include(includeProperty);
    //        }

    //        if (orderBy != null)
    //        {
    //            return orderBy(query).ToList();
    //        }
    //        else
    //        {
    //            return query.ToList();
    //        }
    //    }

    //    public IEnumerable<TEntity> GetAll()
    //    {
    //        return dbSet.ToList();
    //    }

    //    public TEntity GetById(Expression<Func<TEntity, bool>> predicate)
    //    {
    //        return dbSet.Where(predicate).SingleOrDefault();
    //    }

    //    public void Insert(TEntity obj)
    //    {
    //        dbSet.Add(obj);
    //    }


    //    public void Update(TEntity obj)
    //    {
    //        _context.Entry(obj).State = EntityState.Modified;
    //    }
    //    public void Delete(object id)
    //    {
    //        TEntity entity = GetById(id);
    //        dbSet.Remove(entity);
    //    }
    //    public void Save()
    //    {
    //        _context.SaveChanges();
    //    }
    //}
}
