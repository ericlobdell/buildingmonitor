using System.Collections.Immutable;

namespace BuildingMonitor.Messages
{
  public class RespondFloorIds
  {
    public long RequestId { get; }
    public IImmutableSet<string> FloorIds { get; }

    public RespondFloorIds(long requestId, IImmutableSet<string> floorIds)
    {
      RequestId = requestId;
      FloorIds = floorIds;
    }
  }
}
