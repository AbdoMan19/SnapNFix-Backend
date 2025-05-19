using System.Linq.Expressions;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces;

public interface IGenericRepository<TEntity>: IScoped where TEntity : class 
{
    Task<IEnumerable<TEntity>> GetAll();

	TEntity? GetById(Guid id);

	Task<List<TEntity>> GetByIds(Expression<Func<TEntity, bool>> wherePredicate, Guid[] ids);

	Task<List<TEntity>> GetByIds(Expression<Func<TEntity, bool>> wherePredicate, Guid[] ids, List<string> include);

	Task<List<TEntity>> GetByIds(Expression<Func<TEntity, bool>> wherePredicate, Guid[] ids, string columnName);

	Task<List<TEntity>> GetByIds(Expression<Func<TEntity, bool>> wherePredicate, Guid[] ids, string columnName, List<string> include);

	public Task<TEntity> Add(TEntity entity);

	Task AddRange(IEnumerable<TEntity> entityList);

	Task<TEntity> Update(TEntity entity);

	Task UpdateRange(IEnumerable<TEntity> entity);

	TEntity? Delete(Guid id);
	public Task DeleteAll(Expression<Func<TEntity, bool>> predicate);

	Task<IEnumerable<TEntity>> GetData(Expression<Func<TEntity, bool>> predicate);

	Task<IQueryable<TEntity>> GetQueryableData(Expression<Func<TEntity, bool>> predicate);

	Task<TEntity> SingleOrDefault(Expression<Func<TEntity, bool>> predicate);

	Task<TEntity?> FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

	Task<TEntity?> FirstOrDefault();

	bool ExistsById(Guid id);

	bool ExistsByName(Expression<Func<TEntity, bool>> predicate);

	TEntity? GetById(Guid Guid, List<string> include);

	Task<IEnumerable<TEntity>> GroupBy(Expression<Func<TEntity, TEntity>> predicate, List<string> include);

	Task<IEnumerable<TEntity>> GroupBy(Expression<Func<TEntity, TEntity>> predicate);

	Task<IEnumerable<TEntity>> GetData(Expression<Func<TEntity, bool>> predicate, List<string> include);

	Task<IEnumerable<object>> GetData(Expression<Func<TEntity, bool>> wherePredicate, Expression<Func<TEntity, object>> selectIncludePredicate, List<string> include);

	Task<IQueryable<TEntity>> GetQueryableData(Expression<Func<TEntity, bool>> predicate, List<string> include);

	Task<TEntity> SingleOrDefault(Expression<Func<TEntity, bool>> predicate, List<string> include);

	Task<object> FirstOrDefault(Expression<Func<TEntity, bool>> wherePredicate, Expression<Func<TEntity, object>> selectPredicate, List<string> include);

	Task<object> SingleOrDefault(Expression<Func<TEntity, bool>> wherePredicate, Expression<Func<TEntity, object>> selectPredicate, List<string> include);

	Task<IEnumerable<TEntity>> GetAll(params Expression<Func<TEntity, object>>[] includes);

	Task RemoveRange(IEnumerable<TEntity> myObject);

	IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate, bool disableTracking = true);
	IQueryable<TEntity> GetQuerableData();

}