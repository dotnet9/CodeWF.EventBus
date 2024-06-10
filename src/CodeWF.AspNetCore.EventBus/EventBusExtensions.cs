using CodeWF.EventBus;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWF.AspNetCore.EventBus
{
    /// <summary>  
    /// EventBus 的扩展类，提供添加 EventBus 服务和订阅事件的功能
    /// </summary>
    public static class EventBusExtensions
    {
        /// <summary>  
        /// 为服务集合添加 EventBus 服务和相关的事件处理器实例
        /// </summary>  
        /// <param name="services">IServiceCollection 实例，用于添加服务</param>  
        /// <param name="assemblies">包含事件处理器的程序集数组。</param>  
        /// <returns>修改后的 IServiceCollection 实例</returns> 
        public static IServiceCollection AddEventBus(this IServiceCollection services, params Assembly[] assemblies)
        {
            // 将 IMessenger 接口的实现类 Messenger 注册为单例服务
            services.AddSingleton<IMessenger, Messenger>();

            // 遍历程序集，注册事件处理器为单例 
            HandleMessageObject(type => services.AddSingleton(type), assemblies);

            return services;
        }

        /// <summary>  
        /// 初始化 EventBus 并订阅事件处理器实例
        /// </summary>  
        /// <param name="app">IApplicationBuilder 实例，用于构建应用程序的请求处理管道</param>  
        /// <param name="assemblies">包含事件处理器的程序集数组。</param>  
        public static void UseEventBus(this IApplicationBuilder app, params Assembly[] assemblies)
        {
            // 从应用程序服务中获取 IMessenger 实例
            var messenger = app.ApplicationServices.GetRequiredService<IMessenger>();

            // 遍历程序集，订阅事件处理器
            HandleMessageObject(type => messenger.Subscribe(app.ApplicationServices.GetRequiredService(type)),
                assemblies);
        }

        /// <summary>  
        /// 私有辅助方法，用于处理具有 EventHandlerAttribute 的类型
        /// </summary>  
        /// <param name="handleRecipient">处理类型的委托</param>  
        /// <param name="assemblies">包含事件处理器的程序集</param>  
        private static void HandleMessageObject(Action<Type> handleRecipient, params Assembly[] assemblies)
        {
            // 遍历指定的程序集以及当前调用程序集
            foreach (var assembly in assemblies)
            {
                // 获取程序集中满足条件的类型（具有 EventHandlerAttribute 的非抽象类）
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract &&
                                t.GetMethods().Any(m => m.GetCustomAttributes<EventHandlerAttribute>().Any()));

                // 遍历这些类型，并调用 handleRecipient 委托 
                foreach (var type in types)
                {
                    handleRecipient(type);
                }
            }
        }
    }
}