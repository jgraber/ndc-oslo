using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrdersApi.Data.Domain;
using OrdersApi.Models;
using OrdersApi.Service.Clients;
using OrdersApi.Services;
using Stocks;
using ProductStock = OrdersApi.Service.Clients.ProductStock;

namespace OrdersApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IProductStockServiceClient _productStockServiceClient;
        private readonly IMapper _mapper;
        private readonly Greeter.GreeterClient grpcClient;
        private readonly IPublishEndpoint publishEndpoint;

        public OrdersController(IOrderService orderService,
            IProductStockServiceClient productStockServiceClient,
            IMapper mapper,
            Stocks.Greeter.GreeterClient grpcClient,
            IPublishEndpoint publish)
        {
            _orderService = orderService;
            _productStockServiceClient = productStockServiceClient;
            _mapper = mapper;
            this.grpcClient = grpcClient;
            publishEndpoint = publish;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            try
            {
                await _orderService.UpdateOrderAsync(order);
            }
            catch
            {
                if (!await _orderService.OrderExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //[HttpPost]
        //public async Task<ActionResult<Order>> PostOrder(OrderModel model)
        //{
        //    var stocks = await _productStockServiceClient.GetStock(
        //        model.OrderItems.Select(p => p.ProductId).ToList());

        //    var stockRequest = new StockRequest();
        //    stockRequest.ProductId.AddRange(model.OrderItems.Select(p => p.ProductId).ToList());

        //    var stockResponse = await grpcClient.GetStockAsync(stockRequest);

        //    //To do: Verify stock 
        //    // Verify if all products have stock
        //    if (!await VerifyStocks(stockResponse, model.OrderItems))
        //    {
        //        // Add model state error: "Sorry, we can't process your order, we don't have enough stock for item."
        //        ModelState.AddModelError("StockError", "Sorry, we can't process your order due to insufficient stock.");
        //        return BadRequest(ModelState);
        //    }

        //    var orderToAdd = _mapper.Map<Order>(model);
        //    var createdOrder = await _orderService.AddOrderAsync(orderToAdd);
        //    // Diminish stock

        //    return CreatedAtAction("GetOrder", new { id = createdOrder.Id }, createdOrder);
        //}

        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(OrderModel model)
        {

            var orderToAdd = _mapper.Map<Order>(model);
            var createdOrder = await _orderService.AddOrderAsync(orderToAdd);
            //notify an OrderCreated event

            var notifyOrderCreated = publishEndpoint.Publish(new OrderCreated()
            {
                CreatedAt = createdOrder.OrderDate,
                OrderId = createdOrder.Id
            });

            return CreatedAtAction("GetOrder", new { id = createdOrder.Id }, createdOrder);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }

        [HttpHead]
        public async Task<IActionResult> HeadOrder(int id)
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            HttpContext.Response.Headers["OrderStatus"] = order.Status.ToString();
            return Ok(order);
        }

        private async Task<bool> VerifyStocks(ProductStockList stocks, List<OrderItemModel> orderItems)
        {
            foreach (var item in orderItems)
            {
                var stock = stocks.Products.FirstOrDefault(s => s.ProductId == item.ProductId);
                if (stock == null || stock.Stock < item.Quantity)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
