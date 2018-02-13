using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BuildingMonitor.Tests
{
  public class FloorsManagerShould : TestKit
  {
    [Fact]
    public void ReturnNoFloorIdsWhenNewlyCreated()
    {
      var probe = CreateTestProbe();
      var manager = Sys.ActorOf(FloorsManager.Props());

      manager.Tell(new RequestFloorIds(1), probe.Ref);
      var received = probe.ExpectMsg<RespondFloorIds>();

      Assert.Equal(1, received.RequestId);
      Assert.Empty(received.FloorIds);
    }

    [Fact]
    public void RegisterNewFloorWhenDoesNotExist()
    {
      var probe = CreateTestProbe();
      var manager = Sys.ActorOf(FloorsManager.Props());

      manager.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"), probe.Ref);
      probe.ExpectMsg<RespondSensorRegistered>(x => x.RequestId == 1);

      manager.Tell(new RequestFloorIds(2), probe.Ref);
      var received = probe.ExpectMsg<RespondFloorIds>();

      Assert.Equal(2, received.RequestId);
      Assert.Equal(1, received.FloorIds.Count);
      Assert.Contains("a", received.FloorIds);
    }

    [Fact]
    public void ReuseExistingFloorWhenAlreadyExist()
    {
      var probe = CreateTestProbe();
      var manager = Sys.ActorOf(FloorsManager.Props());

      manager.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"), probe.Ref);
      probe.ExpectMsg<RespondSensorRegistered>(x => x.RequestId == 1);

      manager.Tell(new RequestRegisterTemperatureSensor(2, "a", "900"), probe.Ref);
      probe.ExpectMsg<RespondSensorRegistered>(x => x.RequestId == 2);

      manager.Tell(new RequestFloorIds(3), probe.Ref);
      var received = probe.ExpectMsg<RespondFloorIds>();

      Assert.Equal(3, received.RequestId);
      Assert.Equal(1, received.FloorIds.Count);
      Assert.Contains("a", received.FloorIds);
    }

    [Fact]
    public async Task ReturnFloorIdsOnlyFromActiveActors()
    {
      var probe = CreateTestProbe();
      var manager = Sys.ActorOf(FloorsManager.Props(), "FloorsManager");

      manager.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"));
      manager.Tell(new RequestRegisterTemperatureSensor(2, "b", "900"));

      var firstFloor = await Sys.ActorSelection("akka://test/user/FloorsManager/floor-a")
        .ResolveOne(TimeSpan.FromSeconds(3));

      probe.Watch(firstFloor);
      firstFloor.Tell(PoisonPill.Instance);
      probe.ExpectTerminated(firstFloor);

      manager.Tell(new RequestFloorIds(3), probe.Ref);
      var received = probe.ExpectMsg<RespondFloorIds>();

      Assert.Equal(3, received.RequestId);
      Assert.Equal(1, received.FloorIds.Count);
      Assert.Contains("b", received.FloorIds);
    }
  }
}
