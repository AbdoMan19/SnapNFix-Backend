using System.Data;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces;

public interface IUnitOfWork : IScoped
{
    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    
    Task<int> SaveChanges();
    
    Task BeginTransactionAsync(IsolationLevel isolationLevel);
    
    Task CommitTransactionAsync();
    
    Task RollBackTransactionAsync();
}