namespace BudgetManager.Models
{
    public class DailyCostDisplayItem
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        
        public string DisplayDate => Date.ToShortDateString();
    }
}
