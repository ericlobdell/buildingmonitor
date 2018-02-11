using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingMonitor.Messages
{
  public sealed class RespondSensorRegistered
  {
    public long RequestId { get; }
    public IActorRef SensorReference { get; }

    public RespondSensorRegistered(long requestId, IActorRef sensorReference)
    {
      RequestId = requestId;
      SensorReference = sensorReference;
    }
  }
}
