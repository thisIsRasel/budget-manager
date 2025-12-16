using BudgetManager.Models;
using BudgetManager.Services;
using BudgetManager.Views;
using System.Collections.ObjectModel;
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

        public ObservableCollection<DailyCostDisplayItem> Costs { get; } = new();

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


        public ICommand EditBudgetCommand { get; }
        public ICommand DeleteBudgetCommand { get; }

        public BudgetDetailsViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            EditBudgetCommand = new Command(async () => await EditBudgetAsync());
            DeleteBudgetCommand = new Command(async () => await DeleteBudgetAsync());
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
            //if (BudgetId <= 0) return;

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

                // 2. Get linked categories
                var linkedCategories = await _sqlite.GetCategoriesForBudgetAsync(BudgetId);
                var linkedCategoryIds = linkedCategories.Select(c => c.Id).ToList();

                // 3. Get all entries for that month/year
                var entries = await _sqlite.GetEntriesByMonthAsync(budget.Month, budget.Year);

                // 4. Filter entries that match linked categories
                var filteredEntries = entries
                    .Where(e => linkedCategoryIds.Contains(e.CategoryId))
                    .OrderByDescending(e => e.Date)
                    .ToList();

                // 5. Build display items
                var allCategories = await _sqlite.GetCategoriesAsync();

                Costs.Clear();
                decimal total = 0;

                foreach (var entry in filteredEntries)
                {
                    var cat = allCategories.FirstOrDefault(c => c.Id == entry.CategoryId);
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

                TotalSpent = total;

                // Set CurrentBudget for display (re-calculating progress/etc if needed or just basics)
                CurrentBudget = new BudgetDisplayItem
                {
                    Id = budget.Id,
                    Name = budget.Name,
                    Month = budget.Month,
                    Year = budget.Year,
                    Amount = budget.Amount,
                    SpentAmount = total // This ensures it matches the detailed view exactly
                };

            }
            finally
            {
            }
        }
    }
}
