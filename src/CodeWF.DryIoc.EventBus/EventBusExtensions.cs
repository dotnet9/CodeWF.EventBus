using DryIoc;
using System.Reflection;

namespace CodeWF.EventBus
{
    public static partial class EventBusExtensions
    {
        public static IContainerRegistry AddEventBus(this IContainerRegistry services, params Assembly[] assemblies)
        {
            AddEventBus((t1, t2) => services.RegisterSingleton(t1, t2),
                t => services.RegisterSingleton(t),
                assemblies);

            return services;
        }

        public static void UseEventBus(this DryIoc.IContainer app, params Assembly[] assemblies)
        {
            UseEventBus(app.Resolve, assemblies);
        }
    }
}