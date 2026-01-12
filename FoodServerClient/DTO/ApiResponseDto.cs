namespace FoodServerClient.DTO
{
    /// <summary>
    /// Универсальный DTO для ответов API сервера.
    /// </summary>
    public class ApiResponseDto<T>
    {
        /// <summary>
        /// Имя команды, обработанной сервером.
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Признак успешного выполнения запроса.
        /// В случае false см ErrorMessage.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Сообщение об ошибке, возвращаемое сервером,
        /// если Success имеет значение false.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Данные ответа, возвращаемые сервером
        /// при успешном выполнении команды.
        /// </summary>
        public T? Data { get; set; }
    }
}
