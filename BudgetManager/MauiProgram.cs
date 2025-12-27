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
                    fonts.AddFont("FA-Solid-900.otf", "fa-solid");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "budget.sqlite");
            builder.Services.AddSingleton(new SQLiteService(dbPath));

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CategoryListPage>();
            builder.Services.AddTransient<CategoryCreationPage>();
            builder.Services.AddTransient<CategoryUpdatePage>();
            builder.Services.AddTransient<TransactionListPage>();
            builder.Services.AddTransient<TransactionCreationPage>();
            builder.Services.AddTransient<BudgetListPage>();
            builder.Services.AddTransient<BudgetCreationPage>();
            builder.Services.AddTransient<BudgetDetailsPage>();
            builder.Services.AddTransient<BudgetUpdatePage>();
            builder.Services.AddTransient<NotesPage>();
            builder.Services.AddTransient<NoteEntryPage>();
            builder.Services.AddTransient<YearlyBudgetPage>();

            builder.Services.AddTransient<CategoryViewModel>();
            builder.Services.AddTransient<TransactionViewModel>();
            builder.Services.AddTransient<TransactionEntryViewModel>();
            builder.Services.AddTransient<BudgetListViewModel>();
            builder.Services.AddTransient<BudgetCreationViewModel>();
            builder.Services.AddTransient<BudgetUpdateViewModel>();
            builder.Services.AddTransient<BudgetDetailsViewModel>();
            builder.Services.AddTransient<YearlyBudgetViewModel>();
            builder.Services.AddTransient<NotesViewModel>();
            builder.Services.AddTransient<NoteEntryViewModel>();

            return builder.Build();
        }
    }
}
