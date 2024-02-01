using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fallout_Follies.Models
{
    [Route("api/[controller]")]
    [ApiController]
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        // Additional properties like OrderDate, Status, etc.
    }

}
