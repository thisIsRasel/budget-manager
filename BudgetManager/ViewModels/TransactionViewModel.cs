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

        public string MonthDisplay => CurrentMonth.ToString("MMMM yyyy");

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
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }

        public TransactionViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            CurrentMonth = DateTime.Now; // Initialize to current month
            SelectedDate = DateTime.Now; // Initialize to today
            SaveCommand = new Command(async () => await SaveAsync());
            PreviousMonthCommand = new Command(PreviousMonth);
            NextMonthCommand = new Command(NextMonth);
            //DeleteCommand = new Command(async () => await DeleteAsync());
            LoadCategories(); // Initial load for picker
            LoadCostEntries(); // Initial load for list
        }

        public async void LoadCategories()
        {
            var list = await _sqlite.GetCategoriesAsync();
            Categories.Clear();

            var roots = list
                .Where(c => c.ParentId == null || c.ParentId == 0)
                .OrderBy(c => c.SortOrder)
                .ToList();

            foreach (var root in roots)
            {
                root.Indentation = "";
                root.Level = 0;
                Categories.Add(root);
                AddChildren(root, list, 1);
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
            var entries = await _sqlite.GetEntriesByMonthAsync(CurrentMonth.Month, CurrentMonth.Year);

            // We need categories to map names
            var categories = await _sqlite.GetCategoriesAsync();
            var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

            // Group entries by date
            var grouped = entries
                .GroupBy(e => e.Date.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new DailyCostGroup
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(e => e.Amount),
                    Items = new ObservableCollection<DailyCostDisplayItem>(
                        g.Select(entry => new DailyCostDisplayItem
                        {
                            Id = entry.Id,
                            CategoryId = entry.CategoryId,
                            Amount = entry.Amount,
                            Date = entry.Date,
                            Note = entry.Note,
                            CategoryName = categoryMap.ContainsKey(entry.CategoryId)
                                ? categoryMap[entry.CategoryId]
                                : "Unknown"
                        }).OrderByDescending(x => x.Date))
                });

            GroupedCostEntries.Clear();
            foreach (var group in grouped)
            {
                GroupedCostEntries.Add(group);
            }
        }

        private void PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            LoadCostEntries();
        }

        private void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            LoadCostEntries();
        }

        //private async Task DeleteAsync()
        //{
        //    if (_costItemToEdit == null) return;

        //    bool confirm = await Shell.Current.DisplayAlert(
        //        "Delete Entry",
        //        "Are you sure you want to delete this entry?",
        //        "Delete",
        //        "Cancel");

        //    if (!confirm) return;

        //    var entry = new CostEntry { Id = _costItemToEdit.Id };
        //    await _sqlite.DeleteEntryAsync(entry);

        //    // Clear form and navigate back
        //    ClearForm();
        //    await Shell.Current.GoToAsync("..");

        //    // Refresh list
        //    LoadCostEntries();
        //}

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
