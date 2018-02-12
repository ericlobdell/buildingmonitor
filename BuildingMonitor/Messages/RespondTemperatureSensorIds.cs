using System.Collections.Generic;

namespace BuildingMonitor.Messages
{
  public sealed class RespondTemperatureSensorIds
  {
    public long RequestId { get; }
    public ISet<string> SensorIds { get; }

    public RespondTemperatureSensorIds(long requestId, ISet<string> sensorIds)
    {
      RequestId = requestId;
      SensorIds = sensorIds;
    }
  }
}
