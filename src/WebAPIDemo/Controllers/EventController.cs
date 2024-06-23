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
        public async Task AddAsync([FromBody] CreateProductRequest request)
        {
            await _eventBus.PublishAsync(new CreateProductCommand { Name = request.Name, Price = request.Price });
        }

        [HttpDelete("/delete")]
        public async Task DeleteAsync([FromQuery] Guid id)
        {
            await _eventBus.PublishAsync(new DeleteProductCommand { ProductId = id });
        }

        [HttpGet("/get")]
        public async Task<ProductItemDto> GetAsync([FromQuery] Guid id)
        {
            var product = await _eventBus.QueryAsync(new ProductQuery { ProductId = id });
            return product;
        }

        [HttpGet("/list")]
        public async Task<List<ProductItemDto>> ListAsync([FromQuery] string? name)
        {
            var products = await _eventBus.QueryAsync(new ProductsQuery { Name = name });
            return products;
        }
    }
}