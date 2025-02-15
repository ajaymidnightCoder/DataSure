using DataSure.Contracts.AdminService;
using DataSure.Contracts.HelperServices;
using DataSure.Data;
using DataSure.Helper;
using DataSure.Service.AdminService;
using DataSure.Service.HelperServices;
//using DataSure.WinUI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataSure
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();


            builder.Services.AddSingleton<NotificationService>();
            builder.Services.AddSingleton<NotificationService>();
            builder.Services.AddSingleton<IDispatcher>(sp => Application.Current?.Dispatcher ?? throw new InvalidOperationException("Dispatcher not available"));



            // Configure SQLite with EF
            builder.Services.AddScoped<IEntitiyConfigService, EntitiyConfigService>();
            builder.Services.AddScoped<IValidationService, ValidationService>();
            builder.Services.AddScoped<IFileOperationService, FileOperationService>();


            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
