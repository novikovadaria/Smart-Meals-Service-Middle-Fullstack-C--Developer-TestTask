using FoodServerClient.Client;
using FoodServerClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmsConsoleApp.Data;

namespace SmsConsoleApp;

internal static class Program
{
    private static bool _isMockWork = false;
    private static  string _grpcAddress = string.Empty;

    private static async Task Main()
    {
        string logFileName = $"test-sms-console-app-{DateTime.Now:yyyyMMdd}.log";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(logFileName)
            .CreateLogger();

        try
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _grpcAddress = configuration["App:GrpcAddress"]!;
            _isMockWork = configuration.GetValue<bool>("App:UseMockData");

            string connectionString = configuration["App:ConnectionString"]!;

            Log.Information("UseMockData = {UseMockData}", _isMockWork);

            DbContextOptions<AppDbContext> options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseNpgsql(connectionString)
                    .Options;

            await using AppDbContext dbContext = new AppDbContext(options);

            await dbContext.Database.EnsureCreatedAsync();
            Log.Information("База данных и таблицы инициализированы через EF Core");

            IReadOnlyCollection<Dish>? dishes = await GetDishes();

            if (dishes is null)
                return;

            await SaveDishes(dbContext, dishes);

            foreach (Dish dish in dishes.OrderBy(x => x.Name))
            {
                string line = $"{dish.Name} – {dish.Article} – {dish.Price}";
                Log.Information(line);
            }

            Order order = BuildOrderFromConsoleInput(dishes);

            await PlaceOrder(order);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Ошибка при запуске приложения");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Получение меню
    /// </summary>
    private static async Task<IReadOnlyCollection<Dish>?> GetDishes()
    {
        if (_isMockWork)
        {
            Log.Information("Используются mock-данные для меню");
            return GetMockDishes();
        }

        GrpcFoodApiClient api = new GrpcFoodApiClient(_grpcAddress);

        try
        {
            return await api.GetDishes();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка получения меню");
            return null;
        }
    }

    /// <summary>
    /// Отправить заказ
    /// </summary>
    private static async Task PlaceOrder(Order order)
    {
        if (_isMockWork)
        {
            Log.Information("УСПЕХ");
            return;
        }

        GrpcFoodApiClient api = new GrpcFoodApiClient(_grpcAddress);

        try
        {
            await api.PlaceOrder(order);
            Log.Information("УСПЕХ");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка отправки заказа");
        }
    }

    /// <summary>
    /// Сохранение блюд в БД
    /// </summary>
    private static async Task SaveDishes(AppDbContext dbContext, IReadOnlyCollection<Dish> dishes)
    {
        dbContext.Dishes.RemoveRange(dbContext.Dishes);
        await dbContext.SaveChangesAsync();

        await dbContext.Dishes.AddRangeAsync(dishes);
        await dbContext.SaveChangesAsync();

        Log.Information("Сохранено блюд в БД: {Count}", dishes.Count);
    }

    #region Utils

    /// <summary>
    /// Вывести заказ из консоли
    /// </summary>
    private static Order BuildOrderFromConsoleInput(IReadOnlyCollection<Dish> dishes)
    {
        Order order = new Order();

        Dictionary<string, Dish> dishByArticle =
            dishes.ToDictionary(d => d.Article, d => d);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Введите позиции в формате: Код1:Количество1;Код2:Количество2;...");

            string? raw = Console.ReadLine();
            Log.Information("UserInput: {Raw}", raw);

            Dictionary<string, decimal> parsed;
            string error;

            if (!TryParseOrderLine(raw, out parsed, out error))
            {
                Log.Warning("Некорректный ввод: {Error}", error);
                continue;
            }

            List<string> unknownCodes = parsed.Keys
                .Where(code => !dishByArticle.ContainsKey(code))
                .ToList();

            if (unknownCodes.Count > 0)
            {
                string message = $"Неизвестные коды: {string.Join(", ", unknownCodes)}";
                Log.Warning(message);
                continue;
            }

            foreach (KeyValuePair<string, decimal> item in parsed)
            {
                Dish dish = dishByArticle[item.Key];
                order.Items.Add(new OrderItem
                {
                    DishId = dish.Id,
                    Quantity = item.Value
                });
            }

            return order;
        }
    }

    /// <summary>
    /// Распарсить строку заказа
    /// </summary>
    private static bool TryParseOrderLine(string? raw, out Dictionary<string, decimal> result, out string error)
    {
        result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "Пустой ввод. Повторите.";
            return false;
        }

        string[] parts = raw.Split(
            ';',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string part in parts)
        {
            string[] kv = part.Split(
                ':',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (kv.Length != 2)
            {
                error = $"Некорректная позиция '{part}'. Формат: Код:Количество";
                return false;
            }

            if (!decimal.TryParse(
                    kv[1].Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal qty))
            {
                error = $"Некорректное количество в позиции '{part}'.";
                return false;
            }

            if (qty <= 0)
            {
                error = $"Количество должно быть больше нуля в позиции '{part}'.";
                return false;
            }

            string code = kv[0];

            result[code] = result.TryGetValue(code, out decimal existing)
                ? existing + qty
                : qty;
        }

        return true;
    }


    /// <summary>
    /// Вернуть моковые данные
    /// </summary>
    private static IReadOnlyCollection<Dish> GetMockDishes()
    {
        return new List<Dish>
        {
            new Dish
            {
                Id = Guid.NewGuid().ToString(),
                Article = "A001",
                Name = "Пицца Маргарита",
                Price = 450,
                IsWeighted = false,
                FullPath = "Пицца/Классическая"
            },
            new Dish
            {
                Id = Guid.NewGuid().ToString(),
                Article = "A002",
                Name = "Бургер Классический",
                Price = 350,
                IsWeighted = false,
                FullPath = "Бургеры"
            },
            new Dish
            {
                Id = Guid.NewGuid().ToString(),
                Article = "A003",
                Name = "Картофель фри",
                Price = 180,
                IsWeighted = false,
                FullPath = "Гарниры"
            }
        };
    }

    #endregion
}
