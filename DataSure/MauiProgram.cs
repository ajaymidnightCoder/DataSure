using DataSure.Contracts.AdminService;
using DataSure.Contracts.HelperServices;
using DataSure.Data;
using DataSure.Service.AdminService;
using DataSure.Service.HelperServices;
using Microsoft.EntityFrameworkCore;
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
