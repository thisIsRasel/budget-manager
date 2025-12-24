using System.Collections.ObjectModel;

namespace BudgetManager.Models
{
    public class DailyCostGroup
    {
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public ObservableCollection<DailyCostDisplayItem> Items { get; set; } = new();
        
        public string DayNumber => Date.Day.ToString();
        public string DayOfWeek => Date.ToString("ddd");
        public string YearMonth => Date.ToString("yyyy.MM");
        public string DisplayDate => Date.ToString("MMMM, ddd");
    }
}
