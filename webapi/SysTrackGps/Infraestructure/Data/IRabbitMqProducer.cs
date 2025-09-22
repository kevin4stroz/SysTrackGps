using System;

namespace SysTrackGps.Infraestructure.Data;

public interface IRabbitMqProducer
{
    void SendToProcessCurrentPosition<T>(T message);
}
