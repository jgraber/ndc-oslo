using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrdersApi.Data.Domain;
using OrdersApi.Models;
using OrdersApi.Service.Clients;
using OrdersApi.Services;

namespace OrdersApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IProductStockServiceClient _productStockServiceClient;
        private readonly IMapper _mapper;

        public OrdersController(IOrderService orderService,
            IProductStockServiceClient productStockServiceClient,
            IMapper mapper)
        {
            _orderService = orderService;
            _productStockServiceClient = productStockServiceClient;
            _mapper = mapper;
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

        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(OrderModel model)
        {
            var stocks = await _productStockServiceClient.GetStock(
                model.OrderItems.Select(p => p.ProductId).ToList());


            //To do: Verify stock 
            // Verify if all products have stock
            if (!await VerifyStocks(stocks, model.OrderItems))
            {
                // Add model state error: "Sorry, we can't process your order, we don't have enough stock for item."
                ModelState.AddModelError("StockError", "Sorry, we can't process your order due to insufficient stock.");
                return BadRequest(ModelState);
            }

            var orderToAdd = _mapper.Map<Order>(model);
            var createdOrder = await _orderService.AddOrderAsync(orderToAdd);
            // Diminish stock

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

        private async Task<bool> VerifyStocks(List<ProductStock> stocks, List<OrderItemModel> orderItems)
        {
            foreach (var item in orderItems)
            {
                var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);
                if (stock == null || stock.Stock < item.Quantity)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
