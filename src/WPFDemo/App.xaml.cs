using System.Windows;

namespace WPFDemo
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new MockAuto().Show();
        }
    }
}