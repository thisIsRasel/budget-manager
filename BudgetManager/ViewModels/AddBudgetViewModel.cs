using BudgetManager.Models;
using BudgetManager.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    [QueryProperty(nameof(BudgetId), "BudgetId")]
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
        public ICommand SaveBudgetCommand { get; }

        public AddBudgetViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            SaveBudgetCommand = new Command(async () => await SaveBudgetAsync());
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
                PageTitle = "Add Budget";
                return;
            }

            PageTitle = "Edit Budget";
            var budget = await _sqlite.GetBudgetByIdAsync(BudgetId);
            if (budget != null)
            {
                BudgetAmount = budget.Amount;
                // Find associated category
                var links = await _sqlite.GetCategoriesForBudgetAsync(BudgetId);
                if (links.Any())
                {
                    // Assuming one category for now as per current logic
                    var catId = links.First().Id;
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == catId);
                }
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

            if (BudgetId > 0)
            {
                // Update existing
                var budget = await _sqlite.GetBudgetByIdAsync(BudgetId);
                if (budget != null)
                {
                    budget.Name = SelectedCategory.Name;
                    budget.Amount = BudgetAmount;
                    // We don't update Month/Year usually, or maybe we should? Leaving as is for now.
                    await _sqlite.UpdateBudgetAsync(budget);
                    
                    // Update link if category changed
                    // For simplicity, delete old links and add new
                    await _sqlite.DeleteBudgetCategoryLinksAsync(BudgetId);
                    
                     var link = new BudgetCategoryLink
                    {
                        BudgetId = budget.Id,
                        CategoryId = SelectedCategory.Id
                    };
                    await _sqlite.SaveBudgetCategoryLinkAsync(link);
                }
            }
            else
            {
                // Create new
                var budget = new Budget
                {
                    Name = SelectedCategory.Name,
                    Amount = BudgetAmount,
                    Month = DateTime.Today.Month,
                    Year = DateTime.Today.Year
                };

                await _sqlite.SaveBudgetAsync(budget);

                // Save category link
                var link = new BudgetCategoryLink
                {
                    BudgetId = budget.Id,
                    CategoryId = SelectedCategory.Id
                };
                await _sqlite.SaveBudgetCategoryLinkAsync(link);
            }
            await Shell.Current.GoToAsync("..");
        }
    }
}
