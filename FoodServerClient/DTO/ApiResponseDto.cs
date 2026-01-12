namespace FoodServerClient.DTO
{
    public class ApiResponseDto<T>
    {
        public string Command { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
