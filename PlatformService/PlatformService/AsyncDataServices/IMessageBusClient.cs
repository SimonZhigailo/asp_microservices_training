using PlatformService.Dto;

namespace PlatformService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        Task PublishNewPlatform(PlatformPublishedDto platformPublishedDto);
    }
}