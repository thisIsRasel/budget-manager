using BudgetManager.Models;
using BudgetManager.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    [QueryProperty(nameof(MonthlyBudgetItem), "MonthlyBudgetItem")]
    public class BudgetUpdateViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;

        private MonthlyBudgetDisplayItem _monthlyBudgetItem;
        public MonthlyBudgetDisplayItem MonthlyBudgetItem
        {
            get => _monthlyBudgetItem;
            set
            {
                _monthlyBudgetItem = value;
                BudgetAmount = value.BudgetAmount;
                OnPropertyChanged(nameof(MonthlyBudgetItem));
            }
        }

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

        private string _pageTitle = "Edit Budget";
        public string PageTitle
        {
            get => _pageTitle;
            set
            {
                _pageTitle = value;
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        public ObservableCollection<Category> Categories { get; } = new();

        public ICommand SaveBudgetCommand => new Command(async () => await SaveBudgetAsync());

        public BudgetUpdateViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
        }

        private async Task SaveBudgetAsync()
        {
            if (BudgetAmount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please enter a valid amount", "OK");
                return;
            }

            await UpsertBudgetAsync();
            await Shell.Current.GoToAsync("..");
        }

        private async Task UpsertBudgetAsync()
        {
            var budget = await _sqlite.GetMonthlyBudgetByCategoryAsync(
                month: MonthlyBudgetItem.Month,
                year: MonthlyBudgetItem.Year,
                categoryId: MonthlyBudgetItem.CategoryId);

            if (budget is null)
            {
                budget = new Budget
                {
                    CategoryId = MonthlyBudgetItem.CategoryId,
                    Month = MonthlyBudgetItem.Month,
                    Year = MonthlyBudgetItem.Year,
                    Amount = BudgetAmount
                };
                await _sqlite.SaveBudgetAsync(budget);
                return;
            }

            budget.Amount = BudgetAmount;
            await _sqlite.UpdateBudgetAsync(budget);
        }
    }
}
