using BudgetManager.Models;
using BudgetManager.Services;
using BudgetManager.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;
        public ObservableCollection<BudgetDisplayItem> Budgets { get; } = new();

        public ICommand GoToAddBudgetCommand { get; }
        public ICommand GoToBudgetDetailsCommand { get; }

        public BudgetViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            GoToAddBudgetCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(AddBudgetPage)));
            GoToBudgetDetailsCommand = new Command<BudgetDisplayItem>(async (budget) => 
            {
                if (budget == null) return;
                var navParam = new Dictionary<string, object>
                {
                    { "BudgetId", budget.Id }
                };
                await Shell.Current.GoToAsync(nameof(BudgetDetailsPage), navParam);
            });
            LoadBudgets();
        }

        public async void LoadBudgets()
        {
            var budgets = await _sqlite.GetAllBudgetsAsync();
            var allEntries = await _sqlite.GetEntriesByMonthAsync(DateTime.Now.Month, DateTime.Now.Year);
            
            Budgets.Clear();
            decimal totalBudget = 0;
            decimal totalSpent = 0;

            foreach (var budget in budgets)
            {
                // Calculate spent amount for this budget's month/year
                var spentAmount = allEntries
                    .Where(e => e.Date.Month == budget.Month && e.Date.Year == budget.Year)
                    .Sum(e => e.Amount);
                
                Budgets.Add(new BudgetDisplayItem
                {
                    Id = budget.Id,
                    Name = budget.Name,
                    Month = budget.Month,
                    Year = budget.Year,
                    Amount = budget.Amount,
                    SpentAmount = spentAmount
                });

                totalBudget += budget.Amount;
                totalSpent += spentAmount;
            }

            TotalBudget = totalBudget;
            TotalSpent = totalSpent;
        }

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
    }
}
