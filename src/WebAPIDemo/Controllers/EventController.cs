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
        }

        [HttpPost]
        public void Add()
        {
            _messenger.Publish(this, new CreateProductMessage(this, $"{DateTime.Now:HHmmss}ºÅ²úÆ·"));
        }

        [HttpDelete]
        public void Delete()
        {
            _messenger.Publish(this, new DeleteProductMessage(this, $"{DateTime.Now:HHmmss}ºÅ"));
        }
    }
}