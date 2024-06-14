using CodeWF.EventBus;
using Messages.Commands;
using Messages.Dto;
using Messages.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventBus _messenger;

        public EventController(ILogger<EventController> logger, IEventBus messenger)
        {
            _logger = logger;
            _messenger = messenger;
        }

        [HttpPost]
        public Task AddAsync([FromBody] CreateProductRequest request)
        {
            _messenger.Publish(this, new CreateProductCommand { Name = request.Name, Price = request.Price });
            return Task.CompletedTask;
        }

        [HttpDelete]
        public Task DeleteAsync([FromBody] DeleteProductRequest request)
        {
            _messenger.Publish(this, new DeleteProductCommand { Name = request.Name });
            return Task.CompletedTask;
        }

        [HttpGet]
        public async Task<List<ProductItemDto>> QueryAsync([FromQuery] string name)
        {
            var query = new ProductsQuery() { Name = name };
            _messenger.Publish(this, query);
            return await Task.FromResult(query.Result);
        }
    }
}