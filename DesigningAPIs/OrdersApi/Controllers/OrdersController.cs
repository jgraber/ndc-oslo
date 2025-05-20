using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrdersApi.Data.Domain;
using OrdersApi.Models;
using OrdersApi.Service.Clients;
using OrdersApi.Services;
using Stocks;

namespace OrdersApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        // private readonly IProductStockServiceClient _productStockServiceClient;
        private readonly IMapper _mapper;
        private readonly Greeter.GreeterClient grpcClient;


        public OrdersController(IOrderService orderService,
            //IProductStockServiceClient productStockServiceClient,
            IMapper mapper,
            Greeter.GreeterClient grpcClient
            )
        {

            _orderService = orderService;
            this.grpcClient = grpcClient;
            // _productStockServiceClient = productStockServiceClient;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(OrderModel model)
        {
            var stockRequest = new StockRequest();
            stockRequest.ProductId.AddRange
                (model.OrderItems.Select(p => p.ProductId).ToList());

            var stockResponse = await grpcClient.GetStockAsync(stockRequest);

            //To do: Verify stock 
            var orderToAdd = _mapper.Map<Order>(model);
            var createdOrder = await _orderService.AddOrderAsync(orderToAdd);
            // Diminish stock

            return CreatedAtAction("GetOrder", new { id = createdOrder.Id }, createdOrder);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }
    }
}
