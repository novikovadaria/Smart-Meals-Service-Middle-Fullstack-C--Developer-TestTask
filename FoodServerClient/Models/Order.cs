namespace FoodServerClient.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public List<Item> Items { get; set; } = new();
    }
}
