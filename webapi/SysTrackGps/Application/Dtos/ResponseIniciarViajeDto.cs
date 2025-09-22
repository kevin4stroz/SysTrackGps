using System;
using SysTrackGps.Domain.Entities.AlgoritmoA;

namespace SysTrackGps.Application.Dtos;

public class ResponseIniciarViajeDto
{
    public Guid id_vehiculo_viaje { get; set; }
    public required AStarResult ruta_mas_corta { get; set; }
}
