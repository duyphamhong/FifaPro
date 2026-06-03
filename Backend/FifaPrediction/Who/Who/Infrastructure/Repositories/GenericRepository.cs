using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Who.Data;
using Who.Models;

namespace Who.Infrastructure.Repositories
{
    public interface IGenericRepository<TEntity>
       where TEntity : class
    {
        IQueryable<TEntity> GetAll();

        Task<TEntity> GetByIdAsync(Guid id);

        Task<List<TEntity>> GetAllAsync();

        Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task CreateAsync(TEntity entity, bool isSaved = true);

        Task UpdateAsync(TEntity entity, bool isSaved = true, List<string> excludeFieldNames = null);
        Task DeleteAsync(Guid id, bool isSaved = true);
        Task DeleteRangeAsync(TEntity[] entities, bool isSaved = true);
        Task SaveChanges();
        Task<List<TEntity>> Gets(Expression<Func<TEntity, bool>> spec,
            bool isReadOnly = true,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> preFilter = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] postFilters);

        Task<bool> Exist(Expression<Func<TEntity, bool>> spec = null);
        Task<int> Count(Expression<Func<TEntity, bool>> spec = null);
        Task<IEnumerable<TOutput>> GetsAs<TOutput>(Expression<Func<TEntity, TOutput>> projector,
            Expression<Func<TEntity, bool>> spec = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> preFilter = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] postFilters);
    }
    public class GenericRepository<TContext, TEntity> : IGenericRepository<TEntity>
        where TEntity : BaseModel
        where TContext : ApplicationDbContext
    {
        protected readonly TContext DbContext;
        public GenericRepository(TContext dbContext)
        {
            DbContext = dbContext;
        }

        public IQueryable<TEntity> GetAll()
        {
            try
            {
                return DbContext.Set<TEntity>().AsNoTracking();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            try
            {
                return await DbContext.Set<TEntity>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            try
            {
                return await DbContext.Set<TEntity>().AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            try
            {
                IQueryable<TEntity> query = DbContext.Set<TEntity>().AsNoTracking();

                if (include != null) query = include(query);

                query = query.Where(predicate);

                if (orderBy != null) query = orderBy(query);

                return query.ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            try
            {
                IQueryable<TEntity> query = DbContext.Set<TEntity>().AsNoTracking();

                if (include != null) query = include(query);

                query = query.Where(predicate);

                if (orderBy != null) query = orderBy(query);

                return query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CreateAsync(TEntity entity, bool isSaved = true)
        {
            try
            {
                await DbContext.Set<TEntity>().AddAsync(entity);
                if (isSaved) await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateAsync(TEntity entity, bool isSaved = true, List<string> excludeFieldNames = null)
        {
            try
            {
                DbContext.Entry(entity).State = EntityState.Modified;
                DbContext.Set<TEntity>().Update(entity);
                if (excludeFieldNames != null)
                {
                    foreach (var fieldName in excludeFieldNames)
                    {
                        DbContext.Entry(entity).Property(fieldName).IsModified = false;
                    }
                }
                if (isSaved) await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteAsync(Guid id, bool isSaved = true)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                DbContext.Set<TEntity>().Remove(entity);
                if (isSaved) await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteRangeAsync(TEntity[] entities, bool isSaved = true)
        {
            try
            {
                DbContext.Set<TEntity>().RemoveRange(entities);
                if (isSaved) await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveChanges()
        {
            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<TEntity>> Gets(Expression<Func<TEntity, bool>> spec,
            bool isReadOnly = true,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> preFilter = null,
            params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] postFilters)
        {
            return await FindCore(true, spec, preFilter, postFilters).ToListAsync();
        }

        public async Task<bool> Exist(Expression<Func<TEntity, bool>> spec = null)
        {
            return await (spec == null ? DbContext.Set<TEntity>().AnyAsync() : DbContext.Set<TEntity>().AnyAsync(spec));
        }

        public async Task<int> Count(Expression<Func<TEntity, bool>> spec = null)
        {
            return await (spec == null ? DbContext.Set<TEntity>().CountAsync() : DbContext.Set<TEntity>().CountAsync(spec));
        }

        public async Task<IEnumerable<TOutput>> GetsAs<TOutput>(Expression<Func<TEntity, TOutput>> projector,
                                               Expression<Func<TEntity, bool>> spec = null,
                                               Func<IQueryable<TEntity>, IQueryable<TEntity>> preFilter = null,
                                               params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] postFilters)
        {
            if (projector == null)
            {
                throw new ArgumentNullException("projector");
            }

            return await FindCore(true, spec, preFilter, postFilters).Select(projector).ToListAsync();
        }

        private IQueryable<TEntity> FindCore(bool isReadOnly, Expression<Func<TEntity, bool>> spec, Func<IQueryable<TEntity>, IQueryable<TEntity>> preFilter, params Func<IQueryable<TEntity>, IQueryable<TEntity>>[] postFilters)
        {
            var entities = isReadOnly ? DbContext.Set<TEntity>().AsNoTracking() : DbContext.Set<TEntity>();
            var result = preFilter != null ? preFilter(entities) : entities;

            if (spec != null)
            {
                result = result.Where(spec);
            }

            foreach (var postFilter in postFilters)
            {
                if (postFilter != null)
                {
                    result = postFilter(result);
                }
            }
            return result;
        }
    }
}
