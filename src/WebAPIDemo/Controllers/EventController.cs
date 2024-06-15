using CodeWF.EventBus;
using CommandsAndQueries.Commands;
using CommandsAndQueries.Dto;
using CommandsAndQueries.Queries;
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

        [HttpPost("/add")]
        public Task AddAsync([FromBody] CreateProductRequest request)
        {
            _messenger.Publish(this, new CreateProductCommand { Name = request.Name, Price = request.Price });
            return Task.CompletedTask;
        }

        [HttpDelete("/delete")]
        public Task DeleteAsync([FromQuery] Guid id)
        {
            _messenger.Publish(this, new DeleteProductCommand { ProductId = id });
            return Task.CompletedTask;
        }

        [HttpGet("/get")]
        public async Task<ProductItemDto> GetAsync([FromQuery] Guid id)
        {
            var query = new ProductQuery { ProductId = id };
            await _messenger.PublishAsync(this, query);
            return query.Result;
        }

        [HttpGet("/list")]
        public async Task<List<ProductItemDto>> ListAsync([FromQuery] string? name)
        {
            var query = new ProductsQuery { Name = name };
            await _messenger.PublishAsync(this, query);
            return query.Result;
        }
    }
}