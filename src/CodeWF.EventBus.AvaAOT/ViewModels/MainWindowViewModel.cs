using CodeWF.EventBus.AvaAOT.Commands;
using CodeWF.Log.Core;
using ReactiveUI;
using System;
using System.Reactive;

namespace CodeWF.EventBus.AvaAOT.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public MainWindowViewModel()
        {
            EventBus.Default.Subscribe(this);
            SendEventCommand = ReactiveCommand.Create(SendEventHandler);
        }

        public ReactiveCommand<Unit, Unit>? SendEventCommand { get; private set; }

        private void SendEventHandler()
        {
            Logger.Info("Begin send event");
            EventBus.Default.Publish(new UpdateTimeCommand(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
            Logger.Info("End send event");
        }


        [EventHandler]
        private void ReceiveCommand(UpdateTimeCommand command)
        {
            Logger.Info($"Received event: {command.Time}");
        }
    }
}