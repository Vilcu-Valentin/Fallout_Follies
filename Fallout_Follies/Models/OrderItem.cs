using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fallout_Follies.Models
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        // Additional properties like individual price if needed
    }

}
