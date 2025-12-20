using BudgetManager.Models;
using BudgetManager.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    [QueryProperty(nameof(TransactionId), "TransactionId")]
    public class EditTransactionViewModel : BaseViewModel
    {
        private readonly SQLiteService _sqlite;
        public ObservableCollection<Category> Categories { get; } = [];

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(nameof(Amount)); }
        }

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(nameof(SelectedCategory)); }
        }

        private string? _note;
        public string? Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(nameof(Note)); }
        }

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(nameof(SelectedDate)); }
        }

        private int _transactionId;
        public int TransactionId
        {
            get => _transactionId;
            set
            {
                _transactionId = value;
                LoadEntry(value);
            }
        }

        public ICommand SaveCommand => new Command(async () => await SaveAsync());
        public ICommand DeleteCommand => new Command(async () => await DeleteAsync());

        public EditTransactionViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            SelectedDate = DateTime.Now;
        }

        private async void LoadEntry(int id)
        {
            await LoadCategories();
            var transaction = await _sqlite.GetEntryByIdAsync(id);
            if (transaction is null) return;

            Amount = transaction.Amount;
            SelectedDate = transaction.Date;
            Note = transaction.Note;
            SelectedCategory = Categories
                .FirstOrDefault(c => c.Id == transaction.CategoryId);
        }

        public async Task LoadCategories()
        {
            var list = await _sqlite.GetCategoriesAsync();
            Categories.Clear();

            var roots = list
                .Where(c => c.ParentId == null || c.ParentId == 0)
                .OrderBy(c => c.SortOrder)
                .ToList();

            foreach (var root in roots)
            {
                root.Indentation = "";
                root.Level = 0;
                Categories.Add(root);
                AddChildren(root, list, 1);
            }
        }

        private void AddChildren(Category parent, List<Category> all, int level)
        {
            var children = all
                .Where(c => c.ParentId == parent.Id)
                .OrderBy(c => c.SortOrder)
                .ToList();

            foreach (var child in children)
            {
                child.Indentation = new string(' ', level * 4);
                child.Level = level;
                Categories.Add(child);
                AddChildren(child, all, level + 1);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedCategory == null) return;

            var entry = new CostEntry
            {
                Id = TransactionId,
                Amount = Amount,
                CategoryId = SelectedCategory.Id,
                Note = Note!,
                Date = SelectedDate
            };

            await _sqlite.UpdateEntryAsync(entry);

            // Clear form and navigate back
            ClearForm();
            await Shell.Current.GoToAsync("..");
        }

        private async Task DeleteAsync()
        {
            if (TransactionId <= 0) return;

            bool confirm = await App.Current.MainPage.DisplayAlert("Delete transaction",
                "Are you sure you want to delete this transaction?",
                "Yes", "No");

            if (!confirm) return;

            var transaction = await _sqlite.GetEntryByIdAsync(TransactionId);
            if (transaction != null)
            {
                await _sqlite.DeleteEntryAsync(transaction);
            }

            await Shell.Current.GoToAsync("..");
        }

        private void ClearForm()
        {
            Amount = 0;
            Note = string.Empty;
            SelectedCategory = null;
            SelectedDate = DateTime.Now;
        }
    }
}
