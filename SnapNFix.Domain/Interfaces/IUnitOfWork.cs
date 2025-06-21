using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces;

public interface IUnitOfWork : IScoped
{
    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    
    Task<int> SaveChanges();
    
    Task BeginTransactionAsync(IsolationLevel isolationLevel);
    
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    Task CommitTransactionAsync();
    
    Task RollBackTransactionAsync();
}
