using CodeWF.EventBus;
using CommandAndQueryModel.Commands;
using CommandAndQueryModel.Dto;
using CommandAndQueryModel.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventBus _eventBus;

        public EventController(ILogger<EventController> logger, IEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        [HttpPost("/add")]
        public Task AddAsync([FromBody] CreateProductRequest request)
        {
            _eventBus.Publish(new CreateProductCommand { Name = request.Name, Price = request.Price });
            return Task.CompletedTask;
        }

        [HttpDelete("/delete")]
        public Task DeleteAsync([FromQuery] Guid id)
        {
            _eventBus.Publish(new DeleteProductCommand { ProductId = id });
            return Task.CompletedTask;
        }

        [HttpGet("/get")]
        public async Task<ProductItemDto> GetAsync([FromQuery] Guid id)
        {
            var query = new ProductQuery { ProductId = id };
            await _eventBus.PublishAsync(query);
            return query.Result;
        }

        [HttpGet("/list")]
        public async Task<List<ProductItemDto>> ListAsync([FromQuery] string? name)
        {
            var query = new ProductsQuery { Name = name };
            await _eventBus.PublishAsync(query);
            return query.Result;
        }
    }
}