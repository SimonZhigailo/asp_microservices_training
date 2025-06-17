using PlatformService.Dto;

namespace PlatformService.SyncDataServices.http
{
    public interface ICommandDataClient
    {
        Task SendPlatformToCommand(PlatformReadDto platformReadDto);
    }
}