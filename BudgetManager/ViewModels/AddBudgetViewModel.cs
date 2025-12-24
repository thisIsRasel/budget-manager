using BudgetManager.Models;
using BudgetManager.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    [QueryProperty(nameof(BudgetId), "BudgetId")]
    [QueryProperty(nameof(BudgetDate), "BudgetDate")]
    public class AddBudgetViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;

        private decimal _budgetAmount;
        public decimal BudgetAmount
        {
            get => _budgetAmount;
            set
            {
                _budgetAmount = value;
                OnPropertyChanged(nameof(BudgetAmount));
            }
        }

        public ObservableCollection<Category> Categories { get; } = new();

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        private int _budgetId;
        public int BudgetId
        {
            get => _budgetId;
            set
            {
                _budgetId = value;
                LoadBudget();
            }
        }

        private DateTime _budgetDate;
        public DateTime BudgetDate
        {
            get => _budgetDate;
            set
            {
                _budgetDate = value;
                _pageTitle = $"Add Budget ({value:MMM yyyy})";
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        private string _pageTitle = "Add Budget";
        public string PageTitle
        {
            get => _pageTitle;
            set
            {
                _pageTitle = value;
                OnPropertyChanged(nameof(PageTitle));
            }
        }
        public ICommand SaveBudgetCommand => new Command(async () => await SaveBudgetAsync());

        public AddBudgetViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            LoadCategories();
        }

        private async void LoadCategories()
        {
            var categories = await _sqlite.GetCategoriesAsync();
            Categories.Clear();

            // Build hierarchical list similar to CategoryViewModel
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

        private async void LoadBudget()
        {
            if (BudgetId <= 0)
            {
                PageTitle = $"Add Budget ({BudgetDate:MMM yyyy})";
                return;
            }

            var budget = await _sqlite.GetBudgetByIdAsync(BudgetId);
            if (budget != null)
            {
                var monthName = CultureInfo
                    .GetCultureInfo("en-US")
                    .DateTimeFormat
                    .GetAbbreviatedMonthName(budget.Month);

                PageTitle = $"Edit Budget ({monthName} {budget.Year})";
                BudgetAmount = budget.Amount;
                SelectedCategory = Categories
                    .FirstOrDefault(c => c.Id == budget.CategoryId);
            }
        }

        private async Task SaveBudgetAsync()
        {
            if (BudgetAmount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please enter a valid amount", "OK");
                return;
            }

            if (SelectedCategory == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please select a category", "OK");
                return;
            }

            var budget = await _sqlite.GetBudgetByCategoryIdAsync(SelectedCategory.Id);

            if (budget != null)
            {
                budget.Amount = BudgetAmount;
                await _sqlite.UpdateBudgetAsync(budget);
            }
            else
            {
                budget = new Budget
                {
                    CategoryId = SelectedCategory.Id,
                    Amount = BudgetAmount,
                    Month = BudgetDate.Month,
                    Year = BudgetDate.Year
                };

                await _sqlite.SaveBudgetAsync(budget);
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}
