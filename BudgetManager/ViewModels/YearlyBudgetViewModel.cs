using BudgetManager.Models;
using BudgetManager.Services;
using BudgetManager.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    [QueryProperty(nameof(CategoryId), "CategoryId")]
    public class YearlyBudgetViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;

        public ObservableCollection<MonthlyBudgetDisplayItem> YearlyBudgets { get; } = [];

        private int _year;
        public int Year
        {
            get => _year;
            set
            {
                _year = value;
                OnPropertyChanged(nameof(Year));
            }
        }

        private int _categoryId;
        public int CategoryId
        {
            get => _categoryId;
            set
            {
                _categoryId = value;
                OnPropertyChanged(nameof(CategoryId));
                if (_categoryId > 0)
                {
                    LoadYearlyData();
                }
            }
        }

        private Category? _category;
        public Category? Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        public ICommand PreviousYearCommand => new Command(() =>
        {
            Year = Year - 1;
            LoadYearlyData();
        });
        public ICommand NextYearCommand => new Command(() =>
        {
            Year = Year + 1;
            LoadYearlyData();
        });

        public ICommand GoBackCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync("..");
        });

        private MonthlyBudgetDisplayItem _defaultBudget;
        public MonthlyBudgetDisplayItem DefaultBudget
        {
            get => _defaultBudget;
            set
            {
                _defaultBudget = value;
                OnPropertyChanged(nameof(DefaultBudget));
            }
        }

        public ICommand DeleteBudgetCommand => new Command(async () =>
        {
            bool confirm = await Shell.Current.DisplayAlert("Delete Budget",
                "Are you sure you want to delete this budget? All associated cost entries will remain, but the budget link will be removed.",
                "Yes", "No");

            if (!confirm) return;

            await _sqlite.DeleteBudgetsAsync(CategoryId);
            await Shell.Current.GoToAsync(nameof(BudgetListPage));
        });

        public ICommand EditBudgetCommand => new Command<MonthlyBudgetDisplayItem>(async (item) =>
        {
            if (item is null) return;
            var navParam = new Dictionary<string, object>
            {
                { "MonthlyBudgetItem", item }
            };
            await Shell.Current.GoToAsync(nameof(BudgetUpdatePage), navParam);
        });

        public YearlyBudgetViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            _year = DateTime.Now.Year;
        }

        public async void LoadYearlyData()
        {
            if (CategoryId == 0) return;

            var allCategories = await _sqlite.GetCategoriesAsync();
            Category = allCategories.FirstOrDefault(c => c.Id == CategoryId);

            if (Category == null) return;

            // Get budgets for this category (specific year + default year 0)
            var budgets = await _sqlite.GetYearlyAndDefaultBudgetsAsync(Year, CategoryId);

            DefaultBudget = new MonthlyBudgetDisplayItem
            {
                CategoryId = CategoryId,
                BudgetAmount = budgets.FirstOrDefault(b => b.Month == 0 && b.Year == 0)?.Amount ?? 0
            };

            YearlyBudgets.Clear();
            for (int month = 12; month >= 1; month--)
            {
                // Find specific budget for this month/year
                var monthlyBudget = budgets.FirstOrDefault(b => b.Month == month && b.Year == Year);
                decimal budgetAmount = monthlyBudget?.Amount ?? DefaultBudget.BudgetAmount;

                YearlyBudgets.Add(new MonthlyBudgetDisplayItem
                {
                    Year = Year,
                    Month = month,
                    MonthName = new DateTime(Year, month, 1).ToString("MMM"),
                    CategoryId = CategoryId,
                    BudgetAmount = budgetAmount
                });
            }
        }
    }
}
