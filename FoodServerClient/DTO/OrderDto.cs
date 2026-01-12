namespace FoodServerClient.DTO
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public List<OrderItemDto> MenuItems { get; set; } = new();
    }
}
