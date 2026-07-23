using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.IO;
using CodeWF.Log.Core;

namespace CodeWF.EventBus.AvaAOT
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            Logger.Initialize(new LoggerOptions
            {
                File = new FileLogOptions
                {
                    DirectoryPath = Path.Combine(Environment.CurrentDirectory, "Log")
                }
            });

            try
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            finally
            {
                Logger.ShutdownAsync().GetAwaiter().GetResult();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI(_ => { });
    }
}
