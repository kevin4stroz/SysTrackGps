using System;

namespace WorkerConsumer.Infraestructure.Entities;

public class LocalidadRedis
{
    public string Name { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lon { get; set; }

    public double DistanciaHasta(double latDestino, double lonDestino)
    {
        double R = 6371000; // Radio de la Tierra en metros
        double lat1Rad = DegreesToRadians(Lat);
        double lat2Rad = DegreesToRadians(latDestino);
        double deltaLat = DegreesToRadians(latDestino - Lat);
        double deltaLon = DegreesToRadians(lonDestino - Lon);

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
