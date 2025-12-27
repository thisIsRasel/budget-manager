using BudgetManager.Models;
using BudgetManager.Services;
using BudgetManager.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<DailyCostGroup> GroupedCostEntries { get; } = new();

        private decimal _totalBudget;
        public decimal TotalBudget
        {
            get => _totalBudget;
            set
            {
                _totalBudget = value;
                OnPropertyChanged(nameof(TotalBudget));
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

        private decimal _remainingAmount;
        public decimal RemainingAmount
        {
            get => _remainingAmount;
            set
            {
                _remainingAmount = value;
                OnPropertyChanged(nameof(RemainingAmount));
            }
        }

        private DateTime _currentMonth;
        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set
            {
                _currentMonth = value;
                OnPropertyChanged(nameof(CurrentMonth));
                OnPropertyChanged(nameof(MonthDisplay));
            }
        }

        public string MonthDisplay => CurrentMonth.ToString("MMM yyyy");
        public ICommand PreviousMonthCommand => new Command(() =>
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            LoadBudgetsAndCostEntries();
        });
        public ICommand NextMonthCommand => new Command(() =>
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            LoadBudgetsAndCostEntries();
        });

        public ICommand GoToAddCostCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(TransactionCreationPage));
        });

        public ICommand EditItemCommand => new Command<DailyCostDisplayItem>(async (item) =>
        {
            var navParam = new Dictionary<string, object>
            {
                { "TransactionId", item.Id }
            };
            await Shell.Current.GoToAsync(nameof(TransactionUpdatePage), navParam);
        });

        public TransactionViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            CurrentMonth = DateTime.Now; // Initialize to current month
            //LoadBudgetsAndCostEntries(); // Initial load for list
        }

        public async void LoadBudgetsAndCostEntries()
        {
            var entries = await _sqlite.GetMonthlyEntriesAsync(
                month: CurrentMonth.Month,
                year: CurrentMonth.Year);

            var mappedCategories = await GetMappedCategoriesAsync();

            // Group entries by date
            var grouped = entries
                .GroupBy(e => e.Date.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new DailyCostGroup
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(e => e.Amount),
                    Items = new ObservableCollection<DailyCostDisplayItem>(
                        g.Select(entry =>
                        {
                            mappedCategories.TryGetValue(entry.CategoryId, out var category);
                            return new DailyCostDisplayItem
                            {
                                Id = entry.Id,
                                Amount = entry.Amount,
                                Date = entry.Date,
                                Note = entry.Note,
                                CategoryId = entry.CategoryId,
                                CategoryName = category?.Name ?? "Unknown",
                            };
                        }).OrderByDescending(x => x.Date))
                });

            decimal totalSpent = 0;
            GroupedCostEntries.Clear();
            foreach (var group in grouped)
            {
                GroupedCostEntries.Add(group);
                totalSpent += group.TotalAmount;
            }

            decimal totalBudget = 0;
            var budgtes = await GetBudgtesAsync();
            foreach (var item in budgtes)
            {
                totalBudget += item.Amount;
            }

            TotalSpent = totalSpent;
            TotalBudget = totalBudget;
            RemainingAmount = totalBudget - totalSpent;
        }

        private async Task<Dictionary<int, Category>> GetMappedCategoriesAsync()
        {
            // We need categories to map names
            var categories = await _sqlite.GetCategoriesAsync();
            var categoryMap = categories.ToDictionary(c => c.Id, c => c);
            return categoryMap;
        }

        private async Task<List<Budget>> GetBudgtesAsync()
        {
            var defaultBudgtes = await _sqlite.GetMonthlyBudgetsAsync(
                month: 0,
                year: 0);

            var monthlyBudgets = await _sqlite.GetMonthlyBudgetsAsync(
                month: _currentMonth.Month,
                year: _currentMonth.Year);

            var budgets = new List<Budget>();
            foreach (var defaultBudget in defaultBudgtes)
            {
                var budget = monthlyBudgets
                    .FirstOrDefault(x => x.CategoryId == defaultBudget.CategoryId);

                if (budget is null)
                {
                    budgets.Add(defaultBudget);
                    continue;
                }

                budgets.Add(budget);
            }

            return budgets;
        }
    }
}
