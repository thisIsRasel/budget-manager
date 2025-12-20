using BudgetManager.Models;
using BudgetManager.Services;
using Microcharts;
using SkiaSharp;

namespace BudgetManager.ViewModels
{
    public class ReportViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;
        public decimal MonthlyTotal { get; private set; }

        private Chart _budgetChart;
        public Chart BudgetChart
        {
            get => _budgetChart;
            private set
            {
                _budgetChart = value;
                OnPropertyChanged(nameof(BudgetChart));
            }
        }

        public ReportViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            LoadTotals();
        }

        private async void LoadTotals()
        {
            var today = DateTime.Today;
            var monthlyEntries = await _sqlite.GetMonthlyEntriesAsync(today.Month, today.Year);
            MonthlyTotal = monthlyEntries.Sum(e => e.Amount);

            // Chart Logic
            var budgets = await _sqlite.GetAllBudgetsAsync();
            // Filter only budgets for current month/year 
            // (Assuming user wants to compare current budgets)
            var monthlyBudgets = budgets
                .Where(b => b.Month == today.Month && b.Year == today.Year)
                .ToList();

            var entries = new List<ChartEntry>();
            var random = new Random();
            var categories = await _sqlite.GetCategoriesAsync();

            foreach (var budget in monthlyBudgets)
            {
                // Calculate spent for this budget
                // We need categories linked to this budget
                var budgetCategory = categories
                    .FirstOrDefault(c => c.Id == budget.CategoryId);
                if (budgetCategory == null) continue;

                var budgetCategoryIds = GetBudgetCategoryIds(
                    categories,
                    budgetCategory.Id);

                var budgetSpent = monthlyEntries
                    .Where(e => budgetCategoryIds.Contains(e.CategoryId))
                    .Sum(e => e.Amount);

                if (budgetSpent > 0)
                {
                    entries.Add(new ChartEntry((float)budgetSpent)
                    {
                        Label = budgetCategory.Name,
                        ValueLabel = "৳ " + budgetSpent.ToString("F0"),
                        Color = SKColor.Parse(GetRandomColor(random)),
                        TextColor = SKColor.Parse("#FFFFFF"),
                        ValueLabelColor = SKColor.Parse("#FF5722"),
                    });
                }
            }

            BudgetChart = new PieChart {
                Entries = entries,
                LabelTextSize = 30,
                BackgroundColor = SKColor.Parse("#374046"),
            };

            OnPropertyChanged(nameof(MonthlyTotal));
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
