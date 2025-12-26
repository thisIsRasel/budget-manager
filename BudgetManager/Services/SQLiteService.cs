using BudgetManager.Models;
using SQLite;


namespace BudgetManager.Services
{
    public class SQLiteService
    {
        private readonly SQLiteAsyncConnection _db;

        public SQLiteService(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<Budget>().Wait();
            _db.CreateTableAsync<Category>().Wait();
            _db.CreateTableAsync<CostEntry>().Wait();
            _db.CreateTableAsync<Note>().Wait();
        }

        // Budget
        public Task<int> SaveBudgetAsync(Budget budget)
            => _db.InsertAsync(budget);

        public Task<int> UpdateBudgetAsync(Budget budget)
            => _db.UpdateAsync(budget);

        public async Task DeleteBudgetsAsync(int categoryId)
        {
            var budgets = await _db.Table<Budget>()
                .Where(x => x.CategoryId == categoryId)
                .ToListAsync();

            foreach (var item in budgets)
            {
                await _db.DeleteAsync(item);
            }
        }

        public Task<Budget> GetBudgetByIdAsync(int budgetId) => _db
            .Table<Budget>()
            .Where(x => x.Id == budgetId)
            .FirstOrDefaultAsync();

        public Task<List<Budget>> GetYearlyAndDefaultBudgetsAsync(
            int year,
            int categoryId) => _db.Table<Budget>()
                .Where(x => (x.Year == 0 || x.Year == year) && x.CategoryId == categoryId)
                .ToListAsync();

        public Task<List<Budget>> GetMonthlyBudgetsAsync(int month, int year)
        {
            return _db.Table<Budget>()
                .Where(x => x.Month == month && x.Year == year)
                .ToListAsync();
        }

        public Task<Budget> GetMonthlyBudgetByCategoryAsync(
            int month,
            int year,
            int categoryId)
        {
            return _db.Table<Budget>()
                .Where(x => x.Month == month && x.Year == year && x.CategoryId == categoryId)
                .FirstOrDefaultAsync();
        }

        // Categories
        public Task<List<Category>> GetCategoriesAsync() => _db
            .Table<Category>()
            .ToListAsync();
        public Task<int> SaveCategoryAsync(Category category) => _db.InsertAsync(category);
        public Task<int> DeleteCategoryAsync(Category category) => _db.DeleteAsync(category);
        public Task<int> UpdateCategoryAsync(Category category) => _db.UpdateAsync(category);


        // Cost Entries
        public Task<int> SaveEntryAsync(CostEntry entry)
            => _db.InsertAsync(entry);
        public Task<CostEntry> GetEntryByIdAsync(int id) =>
            _db.Table<CostEntry>()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

        public Task<List<CostEntry>> GetMonthlyEntriesAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            return _db.Table<CostEntry>()
                .Where(x => x.Date >= startDate && x.Date < endDate)
                .ToListAsync();
        }

        public Task<int> UpdateEntryAsync(CostEntry entry) => _db.UpdateAsync(entry);
        public Task<int> DeleteEntryAsync(CostEntry entry) => _db.DeleteAsync(entry);

        // Notes
        public Task<List<Note>> GetNotesAsync() => _db.Table<Note>().OrderByDescending(x => x.Date).ToListAsync();
        public Task<int> SaveNoteAsync(Note note) => _db.InsertAsync(note);
        public Task<int> UpdateNoteAsync(Note note) => _db.UpdateAsync(note);
        public Task<int> DeleteNoteAsync(Note note) => _db.DeleteAsync(note);
    }
}
