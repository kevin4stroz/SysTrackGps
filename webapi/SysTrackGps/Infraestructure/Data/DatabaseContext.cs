using System.Data;
using Npgsql;

namespace SysTrackGps.Infraestructure.Data;

/// <summary>
/// Clase de contexto de la base de datos
/// </summary>
public class DatabaseContext : IDisposable, IDatabaseContext
{

    private readonly string _connectionString;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;

    /// <summary>
    /// Constructor de contexto
    /// </summary>
    /// <param name="connectionString"></param>
    public DatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Getter de conexion a base de datos
    /// </summary>
    public IDbConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
            }

            return _connection;
        }
    }

    /// <summary>
    /// Metodo para iniciar transaccion
    /// </summary>
    /// <returns></returns>
    public IDbTransaction BeginTransaction()
    {
        if (_transaction == null)
            _transaction = Connection.BeginTransaction();
        return _transaction;
    }


    /// <summary>
    /// Metodo para realizar commit a la transaccion
    /// </summary>
    public void Commit()
    {
        _transaction?.Commit();
        DisposeTransaction();
    }


    /// <summary>
    /// Metodo para realizar rollback a la transaccion
    /// </summary>
    public void RollBack()
    {
        _transaction?.Rollback();
        DisposeTransaction();
    }


    /// <summary>
    /// Metodo de liberacion de conexion
    /// </summary>
    public void Dispose()
    {
        DisposeTransaction();
        _connection?.Dispose();
        _connection = null;
    }

    /// <summary>
    /// Metodo de liberacion de transacion
    /// </summary>
    private void DisposeTransaction()
    {
        _transaction?.Dispose();
        _transaction = null;
    }

}
