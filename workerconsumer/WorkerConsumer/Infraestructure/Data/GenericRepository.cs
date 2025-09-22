using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using Dapper;

namespace WorkerConsumer.Infraestructure.Data;

/// <summary>
/// Implementacion de repositorio generico
/// </summary>
/// <typeparam name="T"></typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly IDatabaseContext _databaseContext;
    protected readonly string _tableName;
    protected readonly string? _primaryKeyName;

    /// <summary>
    /// Constructor de repositorio generico
    /// </summary>
    /// <param name="databaseContext"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public GenericRepository(IDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
        TableAttribute? tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();

        if (tableAttr is null)
            throw new InvalidOperationException($"La entidad {typeof(T).Name} no tiene definido el atributo [Table].");

        // Construir "schema.table"
        _tableName = string.IsNullOrWhiteSpace(tableAttr.Schema)
            ? tableAttr.Name
            : $"{tableAttr.Schema}.{tableAttr.Name}";

        PropertyInfo? keyProperty = typeof(T)
            .GetProperties()
            .FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));

        if (keyProperty != null)
            _primaryKeyName = keyProperty.Name;
        
    }

    /// <summary>
    /// Listar elementos con o sin where de una tabla usando conexion
    /// </summary>
    /// <param name="where"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> ListAsync(string? where = null, object? parameters = null)
    {
        string query = $"SELECT * FROM {_tableName}";
        query += (where != null && parameters != null) ? $" WHERE {where}" : "";

        return await _databaseContext.Connection.QueryAsync<T>(query, parameters);
    }

    /// <summary>
    /// Listar elementos con o sin where de una tabla usando transaccion
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="where"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> ListAsync(IDbTransaction transaction, string? where = null, object? parameters = null)
    {
        string query = $"SELECT * FROM {_tableName}";
        query += (where != null && parameters != null) ? $" WHERE {where}" : "";

        return await transaction.Connection!.QueryAsync<T>(query, parameters, transaction, null, transaction.Connection!.CreateCommand().CommandType);
    }

    /// <summary>
    /// Actualizar elemento de una tabla por medio de su dto
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task UpdateAsync(IDbTransaction transaction, T entity)
    {
        string queryPart1 = $"UPDATE {_tableName} SET ";
        string queryPart2 = $" WHERE {_primaryKeyName} = @{_primaryKeyName}";

        PropertyInfo[] properties = typeof(T).GetProperties();
        foreach (PropertyInfo p in properties)
        {
            if (p.Name != _primaryKeyName)
            {
                queryPart1 += $" {p.Name} = @{p.Name},";
            }
        }

        string query = $"{queryPart1.TrimEnd(',')} {queryPart2}";

        await transaction.Connection!.QueryAsync(query, entity, transaction, null, transaction.Connection!.CreateCommand().CommandType);

    }

    /// <summary>
    /// Insertar elemento en base de datos por medio de dto
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task InsertAsync(IDbTransaction transaction, T entity)
    {
        string queryPart1 = $"INSERT INTO {_tableName} (";
        string queryPart2 = ") VALUES (";

        PropertyInfo[] properties = typeof(T).GetProperties();

        foreach (PropertyInfo p in properties)
        {
            queryPart1 += $" {p.Name},";
            queryPart2 += $" @{p.Name},";
        }

        string query = $"{queryPart1.TrimEnd(',')} {queryPart2.TrimEnd(',')});";

        _ = await Task.Run(() => transaction.Connection!.QueryAsync(query, entity, transaction, null, transaction.Connection!.CreateCommand().CommandType));

    }

    /// <summary>
    /// Eliminar elemento de table por medio de dto
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task DeleteAsync(IDbTransaction transaction, T entity)
    {
        string query = $"DELETE FROM {_tableName} WHERE {_primaryKeyName} = @{_primaryKeyName}";
        await transaction.Connection!.QueryAsync(query, entity, transaction, null, transaction.Connection!.CreateCommand().CommandType);
    }

}
