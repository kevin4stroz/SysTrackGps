using System;
using System.Data;

namespace WorkerConsumer.Infraestructure.Data;

/// <summary>
/// Interface del contexto de base de datos
/// </summary>
public interface IDatabaseContext
{
    IDbConnection Connection { get; }
    IDbTransaction BeginTransaction();
    void Commit();
    void RollBack();
}

