namespace BudgetManager.Models
{
    public class BudgetDisplayItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public decimal SpentAmount { get; set; }
        
        public decimal RemainingAmount => Amount - SpentAmount;
        public double ProgressPercentage => Amount > 0 ? (double)(SpentAmount / Amount) : 0;
        public string FormattedSpent => $"৳ {SpentAmount:F2}";
        public string FormattedRemaining => $"৳ {RemainingAmount:F2}";
        public string FormattedPercentage => $"{ProgressPercentage * 100:F0}%";
    }
}
