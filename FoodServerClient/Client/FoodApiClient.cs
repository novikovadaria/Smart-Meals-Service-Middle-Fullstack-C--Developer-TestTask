using FoodServerClient.Models;
using System.Net.Http.Headers;
using System.Text;

namespace FoodServerClient.Client
{
    public class FoodApiClient
    {
        private readonly HttpClient _httpClient;

        public FoodApiClient(string baseUrl, string login, string password)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            var authToken = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{login}:{password}")
            );

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authToken);
        }

        public async Task<IReadOnlyCollection<Dish>> GetDishesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task PlaceOrderAsync(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
