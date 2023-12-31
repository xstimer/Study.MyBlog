﻿using Microsoft.EntityFrameworkCore;
using Study.Net.EFCoreEnvironment.DbContexts;
using Study.Net.IBaseRepository;
using Study.Net.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Study.Net.BaseRepository;

public class ArticleTypeRepository : BaseRepository<ArticleType>, IArticleTypeRepository
{
    private readonly MySqlDbContext _dbContext;

    public ArticleTypeRepository(MySqlDbContext dbContext)
    {
        base._dbContext = dbContext;
        _dbContext = dbContext;
    }

    public override async Task<List<ArticleType>> FindAllAsync()
    {
        return await _dbContext.articleTypes.Include(x=>x.Articles).ToListAsync();
    }

    public override async Task<List<ArticleType>> FindAllAsync(Expression<Func<ArticleType, bool>> del)
    {
        return await _dbContext.articleTypes.Where(del).Include(x => x.Articles).ToListAsync();
    }

    public override async Task<ArticleType> FindOneAsync(Guid id)
    {
        return await _dbContext.articleTypes.Include(x => x.Articles).SingleOrDefaultAsync(x => x.Id == id);
    }

    public override async Task<ArticleType> FindOneAsync(Expression<Func<ArticleType, bool>> del)
    {
        return await _dbContext.articleTypes.Include(x => x.Articles).SingleOrDefaultAsync(del);
    }
}
