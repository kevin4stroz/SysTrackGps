using System;
using System.Data;

namespace SysTrackGps.Infraestructure.Data;

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
