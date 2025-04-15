using System.Data;

namespace SnapNFix.Application.Interfaces;

public interface IUnitOfWork
{
    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    
    Task<int> SaveChanges();
    
    Task BeginTransactionAsync(IsolationLevel isolationLevel);
    
    Task CommitTransactionAsync();
    
    Task RollBackTransactionAsync();
}