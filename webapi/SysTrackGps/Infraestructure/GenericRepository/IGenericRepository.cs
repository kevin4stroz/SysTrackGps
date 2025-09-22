using System.Data;

namespace SysTrackGps.Infraestructure.GenericRepository;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> ListAsync(string? where = null, object? parameters = null);
    Task<IEnumerable<T>> ListAsync(IDbTransaction transaction, string? where = null, object? parameters = null);
    Task UpdateAsync(IDbTransaction transaction, T entity);
    Task InsertAsync(IDbTransaction transaction, T entity);
    Task DeleteAsync(IDbTransaction transaction, T entity);
}
