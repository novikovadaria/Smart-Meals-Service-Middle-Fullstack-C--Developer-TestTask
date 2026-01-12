using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FoodServerClient.DTO;
using FoodServerClient.Models;

namespace FoodServerClient.Client;

public class FoodApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions =
        new(JsonSerializerDefaults.Web);

    public FoodApiClient(string baseUrl, string login, string password)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        string? authToken = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{login}:{password}")
        );

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", authToken);
    }

    /// <summary>
    /// Получение списка блюд
    /// </summary>
    public async Task<IReadOnlyCollection<Dish>> GetDishes()
    {
        object requestBody = new
        {
            Command = "GetMenu",
            CommandParameters = new
            {
                WithPrice = true
            }
        };

        ApiResponseDto<MenuDataDto> response = await Send<MenuDataDto>(requestBody);

        return response.Data?.MenuItems
               ?? throw new InvalidOperationException("MenuItems not found in response");
    }

    /// <summary>
    /// Отправка заказа
    /// </summary>
    public async Task PlaceOrder(Order order)
    {
        OrderDto orderDto = new OrderDto
        {
            OrderId = order.OrderId,
            MenuItems = order.Items.Select(i => new OrderItemDto
            {
                Id = i.DishId,
                Quantity = i.Quantity
            }).ToList()
        };

        object requestBody = new
        {
            Command = "SendOrder",
            CommandParameters = orderDto
        };

        await Send<object>(requestBody);
    }

    /// <summary>
    /// Общий метод отправки запросов
    /// </summary>
    private async Task<ApiResponseDto<T>> Send<T>(object body)
    {
        string json = JsonSerializer.Serialize(body, _jsonOptions);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage httpResponse = await _httpClient.PostAsync(string.Empty, content);
        string responseJson = await httpResponse.Content.ReadAsStringAsync();

        var response = JsonSerializer.Deserialize<ApiResponseDto<T>>(
            responseJson, _jsonOptions);

        if (response is null)
            throw new InvalidOperationException("Failed to deserialize server response");

        if (!response.Success)
            throw new InvalidOperationException(response.ErrorMessage);

        return response;
    }
}
