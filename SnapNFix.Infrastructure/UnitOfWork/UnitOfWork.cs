using System.Collections;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Infrastructure.Context;
using SnapNFix.Infrastructure.GenericRepository;
using IsolationLevel = System.Data.IsolationLevel;

namespace SnapNFix.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly SnapNFixContext _dbContext;
    private Hashtable _repositories;

    public UnitOfWork(SnapNFixContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        _repositories ??= [];
        string entityName = typeof(TEntity).Name;
        if (!_repositories.ContainsKey(entityName))
        {
            Type repoType = typeof(GenericRepository<>);
            object repoInstance = Activator.CreateInstance(repoType.MakeGenericType(typeof(TEntity)) , _dbContext);
            _repositories.Add(entityName , repoInstance);
        }

        return (IGenericRepository<TEntity>)_repositories[entityName];
    }

    public async Task<int> SaveChanges()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel)
    {
        await _dbContext.Database.BeginTransactionAsync(isolationLevel);
    }

    public async Task CommitTransactionAsync()
    {
        await _dbContext.Database.CommitTransactionAsync();
    }

    public async Task RollBackTransactionAsync()
    {
        await _dbContext.Database.CommitTransactionAsync();
    }
}