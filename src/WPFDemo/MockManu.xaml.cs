using CodeWF.EventBus;
using Messages.Commands;
using Messages.Queries;
using Messages.Services;
using System.Windows;

namespace WPFDemo
{
    public partial class MockManu : Window
    {
        private bool _isSubscribed;

        public MockManu()
        {
            InitializeComponent();
            ChangeSubscribe();
        }

        void ReceiveManuDeleteProductCommand(DeleteProductCommand command)
        {
            AddLog($"Received manually registered \"{command}|");
            ProductService.Default.DeleteProduct(command);
        }

        private void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            AddLog($"Publish \"{this.TxtMessage.Text}\"");
            EventBus.Default.Publish(this, new CreateProductCommand { Name = this.TxtMessage.Text });
        }

        private void Query_OnClick(object sender, RoutedEventArgs e)
        {
            var query = new ProductsQuery() { Name = this.TxtMessage.Text };
            EventBus.Default.Publish(this, query);
            AddLog($"The query result: \"{query}\"");
        }

        private void AddLog(string message)
        {
            this.TxtLog.AppendText($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
        }

        private void SubscribeOrUnsubscribe_OnClick(object sender, RoutedEventArgs e)
        {
            ChangeSubscribe();
        }

        private void ChangeSubscribe()
        {
            _isSubscribed = !_isSubscribed;
            if (_isSubscribed)
            {
                BtnEvent.Content = "Unsubscribe Message";
                EventBus.Default.Subscribe<DeleteProductCommand>(this, ReceiveManuDeleteProductCommand);
            }
            else
            {
                BtnEvent.Content = "Subscribe Message";
                EventBus.Default.Unsubscribe<DeleteProductCommand>(this, ReceiveManuDeleteProductCommand);
            }
        }
    }
}