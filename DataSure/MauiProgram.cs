﻿using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using DataSure.Contracts.AdminService;
using DataSure.Contracts.DatabaseServices;
using DataSure.Contracts.HelperServices;
using DataSure.Helper;
using DataSure.Service.AdminService;
using DataSure.Service.DatabaseServices;
using DataSure.Service.HelperServices;
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
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();


            builder.Services.AddSingleton<INotificationService, NotificationService>();

            builder.Services.AddSingleton<IDispatcher>(sp =>
                Application.Current?.Dispatcher ?? throw new InvalidOperationException("Dispatcher not available"));

            // Configure SQLite with EF
            builder.Services.AddScoped<IEntitiyConfigService, EntitiyConfigService>();
            builder.Services.AddScoped<IValidationService, ValidationService>();
            builder.Services.AddScoped<IFileOperationService, Helper.FileOperationService>();

            // Get SQLite database file path
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "mydatabase.db");
            // Register the service using the interface
            builder.Services.AddSingleton<ISQLiteService>(sp => new SQLiteService(dbPath));


            // Register FileSaver for dependency injection
            builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
