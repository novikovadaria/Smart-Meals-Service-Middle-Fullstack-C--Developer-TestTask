using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Sms.Test;
using FoodServerClient.Models;
using OrderItem = Sms.Test.OrderItem;
using Order = FoodServerClient.Models.Order;

namespace FoodServerClient.Client;

public class GrpcFoodApiClient
{
    private readonly SmsTestService.SmsTestServiceClient _client;

    public GrpcFoodApiClient(string address)
    {
        GrpcChannel channel = GrpcChannel.ForAddress(address);
        _client = new SmsTestService.SmsTestServiceClient(channel);
    }

    /// <summary>
    /// Получение списка блюд через gRPC
    /// </summary>
    public async Task<IReadOnlyCollection<Dish>> GetDishes()
    {
        GetMenuResponse response = await _client.GetMenuAsync(
            new BoolValue { Value = true }
        );

        if (!response.Success)
            throw new InvalidOperationException(response.ErrorMessage);

        List<Dish> dishes = response.MenuItems
            .Select(item => new Dish
            {
                Id = item.Id,
                Article = item.Article,
                Name = item.Name,
                Price = (decimal)item.Price,
                IsWeighted = item.IsWeighted,
                FullPath = item.FullPath,
                Barcodes = item.Barcodes.ToList()
            })
            .ToList();

        return dishes;
    }


    /// <summary>
    /// Отправка заказа через gRPC
    /// </summary>
    public async Task PlaceOrder(Order order)
    {
        Sms.Test.Order grpcOrder = new Sms.Test.Order
        {
            Id = order.OrderId.ToString()
        };

        grpcOrder.OrderItems.AddRange(
            order.Items.Select(i => new OrderItem
            {
                Id = i.DishId,
                Quantity = (double)i.Quantity
            })
        );

        SendOrderResponse response = await _client.SendOrderAsync(grpcOrder);

        if (!response.Success)
            throw new InvalidOperationException(response.ErrorMessage);
    }
}
