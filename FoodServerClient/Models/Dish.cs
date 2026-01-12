namespace FoodServerClient.Models
{
    public class Dish
    {
        /// <summary>
        /// Уникальный идентификатор блюда в системе.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Артикул блюда.
        /// </summary>
        public string Article { get; set; } = string.Empty;

        /// <summary>
        /// Наименование блюда.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Цена блюда.
        /// Для весовых блюд указывается цена за единицу веса.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Признак весового блюда.
        /// </summary>
        public bool IsWeighted { get; set; }

        /// <summary>
        /// Полный путь категории блюда в иерархии меню.
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// Список штрихкодов, связанных с блюдом.
        /// </summary>
        public List<string> Barcodes { get; set; } = new();
    }
}
