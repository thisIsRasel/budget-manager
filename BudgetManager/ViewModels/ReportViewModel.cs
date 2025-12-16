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
            var monthlyEntries = await _sqlite.GetEntriesByMonthAsync(today.Month, today.Year);
            MonthlyTotal = monthlyEntries.Sum(e => e.Amount);

            // Chart Logic
            var budgets = await _sqlite.GetAllBudgetsAsync();
            // Filter only budgets for current month/year 
            // (Assuming user wants to compare current budgets)
            var currentBudgets = budgets.Where(b => b.Month == today.Month && b.Year == today.Year).ToList();
            
            var entries = new List<ChartEntry>();
            var random = new Random();

            foreach (var budget in currentBudgets)
            {
                // Calculate spent for this budget
                // We need categories linked to this budget
                var linkedCategories = await _sqlite.GetCategoriesForBudgetAsync(budget.Id);
                var linkedIds = linkedCategories.Select(c=>c.Id).ToList();
                
                var budgetSpent = monthlyEntries
                    .Where(e => linkedIds.Contains(e.CategoryId))
                    .Sum(e => e.Amount);
                
                if (budgetSpent > 0)
                {
                    entries.Add(new ChartEntry((float)budgetSpent)
                    {
                        Label = budget.Name,
                        ValueLabel = budgetSpent.ToString("F0"),
                         Color = SKColor.Parse(GetRandomColor(random))
                    });
                }
            }
            
            BudgetChart = new PieChart { Entries = entries, LabelTextSize = 30, LabelMode = LabelMode.RightOnly };

            OnPropertyChanged(nameof(MonthlyTotal));
        }

        private string GetRandomColor(Random random)
        {
             return String.Format("#{0:X6}", random.Next(0x1000000));
        }
    }
}
