using SonatFramework.Systems.EventBus;

namespace GrillSort.Rail
{
    public struct RailEventCompletedEvent : IEvent
    {
        // Event này được bắn khi hoàn thành tất cả stage của Rail event
    }

    public struct RailEventStartedEvent : IEvent
    {
        // Event này được bắn khi user join Rail event lần đầu
    }
}

