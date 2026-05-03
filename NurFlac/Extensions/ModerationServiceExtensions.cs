using Microsoft.Extensions.DependencyInjection;
using NurFlac.UserModeration.Mediator;
using NurFlac.UserModeration.Observers;

namespace NurFlac.Extensions;

public static class ModerationServiceExtensions
{
    public static IServiceCollection AddUserModeration(this IServiceCollection services)
    {
        services.AddSingleton<IModerationObserver, LoggingModerationObserver>();
        services.AddSingleton<IModerationMediator>(sp => 
        {
            var mediator = new ModerationMediator();
            var observers = sp.GetServices<IModerationObserver>();
            foreach (var observer in observers)
            {
                mediator.AddObserver(observer);
            }
            return mediator;
        });
        return services;
    }
}
