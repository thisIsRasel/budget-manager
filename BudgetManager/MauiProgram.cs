using BudgetManager.Services;
using BudgetManager.ViewModels;
using BudgetManager.Views;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;

namespace BudgetManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMicrocharts()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FA-Solid-900.otf", "FASolid");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "budget.sqlite");
            builder.Services.AddSingleton(new SQLiteService(dbPath));

            builder.Services.AddTransient<BudgetViewModel>();
            builder.Services.AddTransient<CategoryViewModel>();
            builder.Services.AddTransient<TransactionViewModel>();
            builder.Services.AddTransient<EditTransactionViewModel>();
            builder.Services.AddTransient<ReportViewModel>();
            builder.Services.AddTransient<AddBudgetViewModel>();

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<BudgetPage>();
            builder.Services.AddTransient<CategoryPage>();
            builder.Services.AddTransient<AddCategoryPage>();
            builder.Services.AddTransient<EditCategoryPage>();
            builder.Services.AddTransient<TransactionListPage>();
            builder.Services.AddTransient<AddTransactionPage>();
            builder.Services.AddTransient<ReportPage>();
            builder.Services.AddTransient<AddBudgetPage>();
            builder.Services.AddTransient<BudgetDetailsPage>();
            builder.Services.AddTransient<BudgetDetailsViewModel>();

            return builder.Build();
        }
    }
}
