using Fallout_Follies.Models;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<Order> GetOrderByIdAsync(int orderId);
    Task<Order> CreateOrderAsync(Order order);
    Task<OrderItem> CreateOrderItemAsync(OrderItem orderItem);
    Task UpdateOrderAsync(Order order);
    Task DeleteOrderAsync(int orderId);
}
