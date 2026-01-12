using System.Windows;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace SmsWpfApp
{
    public partial class App : Application
    {
        public static IConfiguration Configuration { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "Logs/test-sms-wpf-app-.log",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Приложение запущено");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Приложение завершено");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
