using BudgetManager.Models;
using BudgetManager.Services;
using BudgetManager.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    [QueryProperty(nameof(BudgetId), "BudgetId")]
    public partial class BudgetDetailsViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;

        private int _budgetId;
        public int BudgetId
        {
            get => _budgetId;
            set
            {
                _budgetId = value;
                LoadData();
            }
        }

        private BudgetDisplayItem _currentBudget;
        public BudgetDisplayItem CurrentBudget
        {
            get => _currentBudget;
            set
            {
                _currentBudget = value;
                OnPropertyChanged(nameof(CurrentBudget));
            }
        }

        private decimal _totalSpent;
        public decimal TotalSpent
        {
            get => _totalSpent;
            set
            {
                _totalSpent = value;
                OnPropertyChanged(nameof(TotalSpent));
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public ObservableCollection<DailyCostDisplayItem> Costs { get; } = new();

        public ICommand EditBudgetCommand => new Command(async () => await EditBudgetAsync());
        public ICommand DeleteBudgetCommand => new Command(async () => await DeleteBudgetAsync());
        public ICommand EditTransactionCommand => new Command<DailyCostDisplayItem>(async (item) =>
        {
            var navParam = new Dictionary<string, object>
            {
                { "TransactionId", item.Id }
            };
            await Shell.Current.GoToAsync(nameof(EditTransactionPage), navParam);
        });

        public BudgetDetailsViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
        }

        private async Task EditBudgetAsync()
        {
            if (BudgetId <= 0) return;
            // Navigate to AddBudgetPage passing the BudgetId to trigger edit mode
            var navParam = new Dictionary<string, object>
            {
                { "BudgetId", BudgetId }
            };
            await Shell.Current.GoToAsync(nameof(AddBudgetPage), navParam);
        }

        private async Task DeleteBudgetAsync()
        {
            if (BudgetId <= 0) return;

            bool confirm = await App.Current.MainPage.DisplayAlert("Delete Budget",
                "Are you sure you want to delete this budget? All associated cost entries will remain, but the budget link will be removed.",
                "Yes", "No");

            if (!confirm) return;

            var budget = await _sqlite.GetBudgetByIdAsync(BudgetId);
            if (budget != null)
            {
                await _sqlite.DeleteBudgetAsync(budget);
            }

            await Shell.Current.GoToAsync("..");
        }

        private async void LoadData()
        {
            if (BudgetId <= 0) return;

            try
            {
                // 1. Get the budget itself
                var budget = await _sqlite.GetBudgetByIdAsync(BudgetId);
                if (budget == null) return;

                // 2. Get all categories and find linked ones (hierarchy)
                var categories = await _sqlite.GetCategoriesAsync();
                var budgetCategory = categories
                    .FirstOrDefault(c => c.Id == budget.CategoryId);

                if (budgetCategory == null) return;

                var budgetCategoryIds = GetBudgetCategoryIds(
                    categories,
                    budgetCategory.Id).ToList();

                // 3. Get all entries for that month/year
                var entries = await _sqlite.GetMonthlyEntriesAsync(
                    budget.Month,
                    budget.Year);

                // 4. Filter entries that match linked categories
                var filteredEntries = entries
                    .Where(e => budgetCategoryIds.Contains(e.CategoryId))
                    .OrderByDescending(e => e.Date)
                    .ToList();

                // 5. Build display items
                Costs.Clear();
                decimal total = 0;

                foreach (var entry in filteredEntries)
                {
                    var cat = categories.FirstOrDefault(c => c.Id == entry.CategoryId);
                    Costs.Add(new DailyCostDisplayItem
                    {
                        Id = entry.Id,
                        CategoryId = entry.CategoryId,
                        CategoryName = cat?.Name ?? "Unknown",
                        Amount = entry.Amount,
                        Date = entry.Date,
                        Note = entry.Note
                    });
                    total += entry.Amount;
                }

                var monthName = CultureInfo
                    .GetCultureInfo("en-US")
                    .DateTimeFormat
                    .GetAbbreviatedMonthName(budget.Month);

                Title = $"Budget ({monthName} {budget.Year})";
                TotalSpent = total;
                CurrentBudget = new BudgetDisplayItem
                {
                    Id = budget.Id,
                    Name = budgetCategory.Name,
                    Month = budget.Month,
                    Year = budget.Year,
                    Amount = budget.Amount,
                    SpentAmount = total
                };
            }
            finally
            {
            }
        }

        private IEnumerable<int> GetBudgetCategoryIds(
            IEnumerable<Category> categories,
            int parentId)
        {
            var result = new List<int> { parentId };
            var children = categories.Where(c => c.ParentId == parentId);

            foreach (var child in children)
            {
                result.AddRange(GetBudgetCategoryIds(categories, child.Id));
            }

            return result;
        }
    }
}
