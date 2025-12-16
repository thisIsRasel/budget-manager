namespace BudgetManager.Models
{
    public class Budget
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
    }
}
