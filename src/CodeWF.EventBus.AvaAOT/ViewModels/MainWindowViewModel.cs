using System;
using CodeWF.EventBus.AvaAOT.Commands;
using CodeWF.LogViewer.Avalonia;

namespace CodeWF.EventBus.AvaAOT.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            EventBus.Default.Subscribe(this);
        }

        public void SendEventHandler()
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