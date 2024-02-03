using Fallout_Follies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;

    public OrdersController(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return Ok(await _orderRepository.GetAllOrdersAsync());
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _orderRepository.GetOrderByIdAsync(id);

        if (order == null) return NotFound();

        return order;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Order>> PostOrder([FromBody] OrderDto orderDto)
    {
        var order = new Order
        {
            UserId = orderDto.UserId,
        };

        var createdOrder = await _orderRepository.CreateOrderAsync(order);

        foreach (var itemDto in orderDto.OrderItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = createdOrder.Id,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
            };

            await _orderRepository.CreateOrderItemAsync(orderItem);
        }

        return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
    }


    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutOrder(int id, Order order)
    {
        if (id != order.Id) return BadRequest();

        try
        {
            await _orderRepository.UpdateOrderAsync(order);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!(await OrderExists(id))) return NotFound();
            else throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        await _orderRepository.DeleteOrderAsync(id);
        return NoContent();
    }

    private async Task<bool> OrderExists(int id)
    {
        var order = await _orderRepository.GetOrderByIdAsync(id);
        return order != null;
    }
}
