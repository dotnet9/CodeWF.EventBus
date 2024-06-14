using CodeWF.EventBus;
using Messages;
using Messages.Commands;
using Messages.Queries;
using Messages.Services;
using System.Windows;

namespace WPFDemo
{
    public partial class MockAuto : Window
    {
        private bool _isSubscribed;

        public MockAuto()
        {
            InitializeComponent();
            ChangeSubscribe();
        }

        private void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            AddLog($"Publish \"{this.TxtMessage.Text}\"");
            EventBus.Default.Publish(this, new DeleteProductCommand { Name = this.TxtMessage.Text });
        }

        private void Query_OnClick(object sender, RoutedEventArgs e)
        {
            var query = new ProductsQuery() { Name = this.TxtMessage.Text };
            EventBus.Default.Publish(this, query);
            AddLog($"The query result: \"{query}\"");
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
                EventBus.Default.Subscribe(this);
            }
            else
            {
                BtnEvent.Content = "Subscribe Message";
                EventBus.Default.Unsubscribe(this);
            }
        }

        private void AddLog(string message)
        {
            this.TxtLog.AppendText($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
        }


        [EventHandler(Order = 2)]
        private void ReceiveAutoMessage2(CreateProductCommand command)
        {
            AddLog($"Received automatic subscription message 2 \"{command}\"");
            ProductService.Default.CreateProduct(command);
        }

        [EventHandler(Order = 1)]
        private void ReceiveAutoMessage1(CreateProductCommand command)
        {
            AddLog($"Received automatic subscription message 1 \"{command}\"");
            ProductService.Default.CreateProduct(command);
        }

        [EventHandler(Order = 3)]
        private void ReceiveAutoProductsQuery(ProductsQuery query)
        {
            AddLog($"Received automatic subscription \"{query}\"");
            ProductService.Default.QueryProduct(query);
            AddLog($"After query \"{query}\"");
        }
    }
}