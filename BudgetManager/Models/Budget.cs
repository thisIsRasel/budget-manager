namespace BudgetManager.Models
{
    public class Budget
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
    }
}
