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

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(nameof(Amount)); }
        }

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(nameof(SelectedCategory)); }
        }

        private string? _note;
        public string? Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(nameof(Note)); }
        }

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(nameof(SelectedDate)); }
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

        public ICommand GoToAddCostCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(AddTransactionPage));
        });

        public ICommand EditItemCommand => new Command<DailyCostDisplayItem>(async (item) =>
        {
            var navParam = new Dictionary<string, object>
            {
                { "TransactionId", item.Id }
            };
            await Shell.Current.GoToAsync(nameof(EditTransactionPage), navParam);
        });

        public ICommand SaveCommand => new Command(async () => await SaveAsync());
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

        public TransactionViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            CurrentMonth = DateTime.Now; // Initialize to current month
            SelectedDate = DateTime.Now; // Initialize to today
            LoadCategories(); // Initial load for picker
            LoadBudgetsAndCostEntries(); // Initial load for list
        }

        public async void LoadCategories()
        {
            var categories = (await GetMappedCategoriesAsync())
                .Select(x => x.Value)
                .ToList();

            Categories.Clear();
            var roots = categories
                .Where(c => c.ParentId == null || c.ParentId == 0)
                .OrderBy(c => c.SortOrder)
                .ToList();

            foreach (var root in roots)
            {
                root.Indentation = "";
                root.Level = 0;
                Categories.Add(root);
                AddChildren(root, categories, 1);
            }
        }

        private void AddChildren(Category parent, List<Category> all, int level)
        {
            var children = all
                .Where(c => c.ParentId == parent.Id)
                .OrderBy(c => c.SortOrder)
                .ToList();

            foreach (var child in children)
            {
                child.Indentation = new string(' ', level * 4);
                child.Level = level;
                Categories.Add(child);
                AddChildren(child, all, level + 1);
            }
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
                        g.Select(entry => {
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

            GroupedCostEntries.Clear();
            decimal totalSpent = 0;
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

        private async Task SaveAsync()
        {
            if (SelectedCategory == null) return;

            var entry = new CostEntry
            {
                Amount = Amount,
                CategoryId = SelectedCategory.Id,
                Note = Note,
                Date = SelectedDate
            };

            // Insert new entry
            await _sqlite.SaveEntryAsync(entry);

            // Clear form and navigate back
            ClearForm();
            await Shell.Current.GoToAsync("..");

            // Refresh list
            LoadBudgetsAndCostEntries();
        }

        private void ClearForm()
        {
            Amount = 0;
            Note = string.Empty;
            SelectedCategory = null;
            SelectedDate = DateTime.Now;
        }
    }
}
