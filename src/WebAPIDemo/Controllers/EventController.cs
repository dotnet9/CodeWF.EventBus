using CodeWF.EventBus;
using Microsoft.AspNetCore.Mvc;
using WebAPIDemo.EventBus.Events;

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
        }

        [HttpGet]
        public string Get()
        {
            _messenger.Publish(this, new SayHelloMessage(this, "Hello!"));

            return "Get success";
        }
    }
}