using LedgerX.Models;

namespace LedgerX.Repositories;

/// <summary>
/// In-memory generic repository base class
/// </summary>
public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly List<T> _data;

    protected RepositoryBase()
    {
        _data = new List<T>();
    }

    public virtual Task<T?> GetByIdAsync(string id)
    {
        return Task.FromResult(_data.FirstOrDefault(x => GetId(x) == id));
    }

    public virtual Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<T>>(_data.AsEnumerable());
    }

    public virtual Task<T> AddAsync(T entity)
    {
        _data.Add(entity);
        return Task.FromResult(entity);
    }

    public virtual Task<T> UpdateAsync(T entity)
    {
        var index = _data.FindIndex(x => GetId(x) == GetId(entity));
        if (index >= 0)
        {
            _data[index] = entity;
        }
        return Task.FromResult(entity);
    }

    public virtual Task<bool> DeleteAsync(string id)
    {
        var entity = _data.FirstOrDefault(x => GetId(x) == id);
        if (entity != null)
        {
            _data.Remove(entity);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public virtual Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(_data.Any(x => GetId(x) == id));
    }

    protected abstract string GetId(T entity);
}
