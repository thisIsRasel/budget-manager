using BudgetManager.Models;
using BudgetManager.Services;
using BudgetManager.Views;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;
        public ObservableCollection<BudgetDisplayItem> Budgets { get; } = [];

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

        private Chart? _budgetChart;
        public Chart? BudgetChart
        {
            get => _budgetChart;
            private set
            {
                _budgetChart = value;
                OnPropertyChanged(nameof(BudgetChart));
                OnPropertyChanged(nameof(IsChartVisible));
            }
        }

        public string MonthDisplay => CurrentMonth.ToString("MMM yyyy");

        public bool IsChartVisible => BudgetChart is not null;

        public ICommand GoToAddBudgetCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(AddBudgetPage));
        });

        public ICommand GoToBudgetDetailsCommand => new Command<BudgetDisplayItem>(async (budget) =>
        {
            if (budget == null) return;
            var navParam = new Dictionary<string, object>
            {
                { "BudgetId", budget.Id }
            };

            await Shell.Current.GoToAsync(nameof(BudgetDetailsPage), navParam);
        });

        public ICommand PreviousMonthCommand => new Command(() =>
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            LoadBudgets();
        });

        public ICommand NextMonthCommand => new Command(() =>
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            LoadBudgets();
        });

        public BudgetViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            _currentMonth = DateTime.Now;
            LoadBudgets();
        }

        public async void LoadBudgets()
        {
            var categories = await _sqlite.GetCategoriesAsync();
            var budgets = await _sqlite.GetMonthlyBudgetsAsync(
                month: _currentMonth.Month,
                year: _currentMonth.Year);

            var monthlyEntries = await _sqlite.GetMonthlyEntriesAsync(
                month: _currentMonth.Month,
                year: _currentMonth.Year);

            decimal totalBudget = 0;
            decimal totalSpent = 0;

            Budgets.Clear();
            var random = new Random();
            var entries = new List<ChartEntry>();
            foreach (var budget in budgets)
            {
                var budgetCategory = categories
                    .FirstOrDefault(c => c.Id == budget.CategoryId);

                if (budgetCategory == null) continue;

                var budgetCategoryIds = GetBudgetCategoryIds(
                    categories,
                    budgetCategory.Id).ToList();

                // Calculate spent amount for this budget's month/year
                var spentAmount = monthlyEntries
                    .Where(e => budgetCategoryIds.Contains(e.CategoryId))
                    .Sum(e => e.Amount);

                Budgets.Add(new BudgetDisplayItem
                {
                    Id = budget.Id,
                    Name = budgetCategory.Name,
                    Month = budget.Month,
                    Year = budget.Year,
                    Amount = budget.Amount,
                    SpentAmount = spentAmount
                });

                if (spentAmount > 0)
                {
                    entries.Add(new ChartEntry((float)spentAmount)
                    {
                        Label = budgetCategory.Name,
                        ValueLabel = "৳ " + spentAmount.ToString("F0"),
                        Color = SKColor.Parse(GetRandomColor(random)),
                        TextColor = SKColor.Parse("#FFFFFF"),
                        ValueLabelColor = SKColor.Parse("#FF5722"),
                    });
                }

                totalBudget += budget.Amount;
                totalSpent += spentAmount;
            }

            TotalBudget = totalBudget;
            TotalSpent = totalSpent;
            RemainingAmount = totalBudget - totalSpent;

            BudgetChart = entries.Any() ? new PieChart
            {
                Entries = entries,
                LabelTextSize = 30,
                BackgroundColor = SKColor.Parse("#374046"),
                LabelMode = LabelMode.LeftAndRight,
                GraphPosition = GraphPosition.Center,
            } : null;
        }

        private string GetRandomColor(Random random)
        {
            return String.Format("#{0:X6}", random.Next(0x1000000));
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
