namespace FoodServerClient.Models
{
    public class OrderItem
    {
        public string DishId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }
}
