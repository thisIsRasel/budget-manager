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
        }

        // Budget
        public Task<int> SaveBudgetAsync(Budget budget)
            => _db.InsertAsync(budget);

        public Task<int> UpdateBudgetAsync(Budget budget)
            => _db.UpdateAsync(budget);
        
        public async Task DeleteBudgetAsync(Budget budget)
        {
             await _db.DeleteAsync(budget);
        }

        public Task<Budget> GetBudgetByIdAsync(int budgetId) => _db
            .Table<Budget>()
            .Where(x => x.Id == budgetId)
            .FirstOrDefaultAsync();

        public Task<List<Budget>> GetAllBudgetsAsync() => _db
            .Table<Budget>()
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync();


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

        public Task<List<CostEntry>> GetEntriesByMonthAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            return _db.Table<CostEntry>()
                .Where(x => x.Date >= startDate && x.Date < endDate)
                .ToListAsync();
        }

        public Task<List<CostEntry>> GetEntriesByDateAsync(DateTime date)
            => _db.Table<CostEntry>()
                .Where(x => x.Date.Date == date.Date)
                .ToListAsync();

        public Task<int> UpdateEntryAsync(CostEntry entry) => _db.UpdateAsync(entry);
        public Task<int> DeleteEntryAsync(CostEntry entry) => _db.DeleteAsync(entry);
    }
}
