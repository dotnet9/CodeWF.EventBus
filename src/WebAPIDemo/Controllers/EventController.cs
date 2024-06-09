using CodeWF.EventBus;
using MessageViewModel;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IMessenger _messenger;

        public EventController(ILogger<EventController> logger, IMessenger messenger)
        {
            _logger = logger;
            _messenger = messenger;

            _messenger.Subscribe<SayHelloMessage>(this, ReceiveSayHelloMessage);
            _messenger.Subscribe(this);
        }

        [HttpGet]
        public string Get()
        {
            _messenger.Publish(this, new SayHelloMessage(this, "Hello!"));

            return "Get success";
        }

        void ReceiveSayHelloMessage(SayHelloMessage message)
        {
            Console.WriteLine($"Receive SayHelloMessage, message is: {message.Word}");
        }

        [EventHandler(Order = 1)]
        void ReceiveMessage2(SayHelloMessage message)
        {
            Console.WriteLine($"Receive auto handler 1, message is: {message}");
        }

        [EventHandler(Order = 2)]
        void ReceiveMessage3(SayHelloMessage message)
        {
            Console.WriteLine($"Receive auto handler 2, message is: {message}");
        }
    }
}