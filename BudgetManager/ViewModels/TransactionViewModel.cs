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
            LoadCostEntries();
        });
        public ICommand NextMonthCommand => new Command(() =>
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            LoadCostEntries();
        });

        public TransactionViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            CurrentMonth = DateTime.Now; // Initialize to current month
            SelectedDate = DateTime.Now; // Initialize to today
            LoadCategories(); // Initial load for picker
            LoadCostEntries(); // Initial load for list
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

        public async void LoadCostEntries()
        {
            var entries = await _sqlite.GetMonthlyEntriesAsync(
                CurrentMonth.Month,
                CurrentMonth.Year);

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
            foreach (var group in grouped)
            {
                GroupedCostEntries.Add(group);
            }
        }

        private async Task<Dictionary<int, Category>> GetMappedCategoriesAsync()
        {
            // We need categories to map names
            var categories = await _sqlite.GetCategoriesAsync();
            var categoryMap = categories.ToDictionary(c => c.Id, c => c);
            return categoryMap;
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
            LoadCostEntries();
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
