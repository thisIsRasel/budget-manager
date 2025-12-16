using SQLite;

namespace BudgetManager.Models
{
    public class BudgetCategoryLink
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int BudgetId { get; set; }
        public int CategoryId { get; set; }
    }
}
