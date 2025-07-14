using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;

namespace CommandService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory,
            IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch (eventType)
            {
                case EventType.PlatformPublished:
                    addPlatform(message);
                    break;
                default:
                    break;
            }
        }

        private EventType DetermineEvent(string message)
        {
            Console.WriteLine("--> Determining Event type");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(message);

            switch (eventType.Event)
            {
                case "Platform_Published":
                    Console.WriteLine("Platform Puyblished Event");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine("Platform Puyblished Event Undertermined");
                    return EventType.Undertermined;
            }
        }

        private void addPlatform(string platformPublishedMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                try
                {
                    var plat = _mapper.Map<Platform>(platformPublishedDto);

                    if (!repo.ExternalPlatformExists(plat.ExternalId))
                    {
                        repo.CreatePlatform(plat);
                        repo.SaveChanges();
                        Console.WriteLine($"--> Платформа {plat.Name} создана");
                    }
                    else
                    {
                        Console.WriteLine($"--> Платформа уже существует");
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"--> Не могу отправить платформу в БД {ex.Message}");
                }
            }
        }
    }

    enum EventType
    {
        PlatformPublished,
        Undertermined
    }
}