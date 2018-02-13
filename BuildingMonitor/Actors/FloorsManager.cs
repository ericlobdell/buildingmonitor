using Akka.Actor;
using BuildingMonitor.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BuildingMonitor.Actors
{
  public class FloorsManager : UntypedActor
  {
    private Dictionary<string, IActorRef> _floorIdToActorRefMap =
      new Dictionary<string, IActorRef>();

    protected override void OnReceive(object message)
    {
      switch (message)
      {
        case RequestFloorIds m:
          Sender.Tell(new RespondFloorIds(m.RequestId,
            ImmutableHashSet.CreateRange(_floorIdToActorRefMap.Keys)));
          break;
        case RequestRegisterTemperatureSensor m:
          if (_floorIdToActorRefMap.TryGetValue(m.FloorId, out var floorRef))
            floorRef.Forward(m);

          else
          {
            var newFloorActor = Context.ActorOf(Floor.Props(m.FloorId), $"floor-{m.FloorId}");
            Context.Watch(newFloorActor);

            _floorIdToActorRefMap.Add(m.FloorId, newFloorActor);
            newFloorActor.Forward(m);
          }
          break;
        case Terminated m:
          var terminatedFloorId = _floorIdToActorRefMap.First(x => x.Value == m.ActorRef).Key;
          _floorIdToActorRefMap.Remove(terminatedFloorId);
          break;
        default:
          Unhandled(message);
          break;
      }
    }

    public static Props Props() =>
      Akka.Actor.Props.Create(() => new FloorsManager());
  }
}
