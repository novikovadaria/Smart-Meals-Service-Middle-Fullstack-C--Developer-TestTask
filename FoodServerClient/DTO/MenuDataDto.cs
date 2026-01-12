using FoodServerClient.Models;

namespace FoodServerClient.DTO
{
    public class MenuDataDto
    {
        public List<Dish> MenuItems { get; set; } = new();
    }
}
