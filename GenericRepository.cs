using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Repository
{
    public class GenericRepository<TEntity, Context> : IGenericRepository<TEntity> where TEntity : class where Context : DbContext, new()
    {
        private Context _context = new Context();
        private bool disposed = false;

        public Context context
        {
            get { return _context; }
            set { _context = value; }
        }

        internal DbSet<TEntity> dbSet;

        public GenericRepository()
        {
            this.dbSet = context.Set<TEntity>();
        }


        #region GETS
        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderby != null)
            {
                return orderby(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public virtual IQueryable<TEntity> GetQueryable(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderby != null)
            {
                return orderby(query);
            }
            else
            {
                return query;
            }
        }        

        public virtual TEntity GetByID(object id)
        {
            return dbSet.Find(id);
        }

        public virtual int GetCount(Expression<Func<TEntity, bool>> filter)
        {
            return GetQueryable(filter).Count();
        }

        #endregion


        #region CRUD
        public virtual void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public void Save()
        {
            try
            {
                context.SaveChanges();
            }
            catch(DbEntityValidationException)
            {
                throw;
            }            
        }


        #endregion

        #region async
        public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null, string includeProperties = "")
        {
            return await GetQueryable(filter, orderby, includeProperties).ToListAsync();
        }

        public virtual Task<TEntity> GetByIDAsync(object id)
        {
            return dbSet.FindAsync(id);
        }

        public virtual Task SaveAsync()
        {
            try
            {
                context.SaveChangesAsync();
            }
            catch (DbEntityValidationException)
            {
                throw;
            }
            return Task.FromResult(0);
        }
        #endregion

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
