
namespace BudgetManager.Models
{
    public class MonthlyBudgetDisplayItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal BudgetAmount { get; set; }
    }
}
