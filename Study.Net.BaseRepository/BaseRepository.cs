using Microsoft.EntityFrameworkCore;
using Study.Net.EFCoreEnvironment.DbContexts;
using Study.Net.IBaseRepository;
using System.Linq.Expressions;

namespace Study.Net.BaseRepository;

public abstract class  BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
{
    protected MySqlDbContext _dbContext;

    public async Task<bool> CreateAsync(TEntity entity)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(TEntity entity)
    {
        return await UpdateAsync(entity);
    }

    public virtual async Task<List<TEntity>> FindAllAsync()
    {
        return await _dbContext.Set<TEntity>().ToListAsync();
    }

    public virtual async Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> del)
    {
        return await _dbContext.Set<TEntity>().Where(del).ToListAsync();
    }

    public virtual async Task<TEntity> FindOneAsync(Guid id)
    {
        return await _dbContext.Set<TEntity>().FindAsync(id);
    }

    public virtual async Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> del)
    {
        return await _dbContext.Set<TEntity>().FirstOrDefaultAsync(del);
    }

    public async Task<bool> UpdateAsync(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
        return await _dbContext.SaveChangesAsync() > 0;
    }
}