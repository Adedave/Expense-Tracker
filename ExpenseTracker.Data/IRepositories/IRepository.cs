using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ExpenseTracker.Data.IRepositories
{
    public interface IRepository<TEntity> where TEntity: class
    {
        IEnumerable<TEntity> Get(
          Expression<Func<TEntity, bool>> filter = null,
          Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
          string includeProperties = "");

        /// <summary>  
        /// Gets all.  
        /// </summary>  
        /// <returns>IEnumerable<TEntity></returns>  
        IEnumerable<TEntity> GetAll(string userId);
        /// <summary>  
        /// Gets the specified identifier.  
        /// </summary>  
        /// <param name="id">The identifier.</param>  
        /// <returns>TEntity</returns>  
        TEntity GetById(int id);
        void Insert(TEntity obj);
        void Update(TEntity obj);
        void Delete(TEntity obj);
    }
}
