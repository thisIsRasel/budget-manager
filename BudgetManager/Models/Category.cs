using SQLite;

namespace BudgetManager.Models
{
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }

        [Ignore]
        public string Indentation { get; set; } = string.Empty;
        
        [Ignore]
        public int Level { get; set; }

        [Ignore]
        public Thickness IndentationMargin => new(Level * 20, 0, 0, 0);

        [Ignore]
        public string DisplayName => Indentation + Name;
    }
}
