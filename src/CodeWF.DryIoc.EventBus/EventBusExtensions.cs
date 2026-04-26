using CodeWF.EventBus;
using DryIoc;
using Prism.Ioc;
using System;
using System.Linq;
using System.Reflection;

namespace CodeWF.DryIoc.EventBus
{
    /// <summary>
    /// DryIoc / Prism 场景下的事件总线扩展。
    /// </summary>
    public static class EventBusExtensions
    {
        private static Assembly[] GetAssemblies(Assembly[] assemblies)
        {
            return assemblies
                .Concat(new[] { Assembly.GetCallingAssembly() })
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// 注册事件总线及自动发现的实例处理器。
        /// </summary>
        public static IContainerRegistry AddEventBus(this IContainerRegistry services, params Assembly[] assemblies)
        {
            services.RegisterSingleton<IEventBus, CodeWF.EventBus.EventBus>();

            var allAssemblies = GetAssemblies(assemblies);

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => services.RegisterScoped(type),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                allAssemblies);

            return services;
        }

        /// <summary>
        /// 启动事件总线，并通过 DryIoc 作用域解析实例处理器。
        /// </summary>
        public static void UseEventBus(this IContainer app, params Assembly[] assemblies)
        {
            if (app.Resolve<IEventBus>() is not { } messenger)
            {
                throw new InvalidOperationException("Please call AddEventBus before calling UseEventBus");
            }

            var allAssemblies = GetAssemblies(assemblies);

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => messenger.Subscribe(type),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);
            messenger.RegisterServiceHandlerAction((type, action) =>
            {
                using var scope = app.OpenScope();
                var obj = scope.Resolve(type);
                action(obj);
            });
            messenger.Subscribe(allAssemblies);
        }
    }
}
