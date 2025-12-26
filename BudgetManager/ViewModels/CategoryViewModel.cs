using BudgetManager.Models;
using BudgetManager.Services;
using BudgetManager.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BudgetManager.ViewModels
{
    [QueryProperty(nameof(CategoryToEdit), "Category")]
    public class CategoryViewModel : BaseViewModel
    {
        private string _newCategory;
        private Category DragSource { get; set; }
        private readonly SQLiteService _sqlite;
        public ObservableCollection<Category> Categories { get; } = [];
        public ObservableCollection<Category> ParentCategories { get; } = [];

        private Category _categoryToEdit;
        public Category CategoryToEdit
        {
            get => _categoryToEdit;
            set
            {
                _categoryToEdit = value;
                if (value != null)
                {
                    NewCategory = value.Name;
                    // Filter parents to avoid circular dependency + current category
                    // Also select current parent
                    UpdateParentCategoriesForEdit(value);
                }
            }
        }

        private Category? _selectedParent;
        public Category? SelectedParent
        {
            get => _selectedParent;
            set
            {
                _selectedParent = value;
                OnPropertyChanged(nameof(SelectedParent));
            }
        }

        public string NewCategory
        {
            get => _newCategory;
            set
            {
                _newCategory = value;
                OnPropertyChanged(nameof(NewCategory));
            }
        }

        public ICommand AddCommand => new Command(async () => await AddCategoryAsync());
        public ICommand GoToAddCategoryCommand
            => new Command(async () => await Shell.Current.GoToAsync(nameof(CategoryCreationPage)));

        public ICommand EditCategoryCommand => new Command<Category>(async (cat) =>
        {
            var navParam = new Dictionary<string, object>
            {
                { "Category", cat }
            };
            await Shell.Current.GoToAsync(nameof(CategoryUpdatePage), navParam);
        });

        public ICommand UpdateCommand => new Command(async () =>
        {
            if (CategoryToEdit is null || CategoryToEdit.Id == SelectedParent?.Id)
            {
                return;
            }

            CategoryToEdit.Name = NewCategory;
            CategoryToEdit.ParentId = (SelectedParent != null && SelectedParent.Id != 0)
                ? SelectedParent.Id
                : null;

            await _sqlite.UpdateCategoryAsync(CategoryToEdit);

            // Navigate back
            await Shell.Current.GoToAsync("..");
        });

        public ICommand DeleteCommand
            => new Command<Category>(async (cat) => await DeleteCategoryAsync(cat));
        public ICommand DragStartingCommand => new Command<Category>((cat) =>
        {
            // Store the item being dragged
            DragSource = cat;
        });

        public ICommand DropCommand => new Command<Category>(async (target) =>
        {
            if (DragSource == null || target == null || DragSource.ParentId != target.ParentId)
                return;

            var sourceIndex = Categories.IndexOf(DragSource);
            var targetIndex = Categories.IndexOf(target);

            if (sourceIndex < 0 || targetIndex < 0) return;

            RecursiveMove(DragSource, targetIndex);
            await ReorderCategoriesAsync(DragSource, target);
        });

        public CategoryViewModel(SQLiteService sqlite)
        {
            _sqlite = sqlite;
            LoadCategories();
        }

        public async void LoadCategories()
        {
            var list = await _sqlite.GetCategoriesAsync();
            Categories.Clear();
            ParentCategories.Clear();

            // Simple sorting/indentation logic
            var roots = list
                .Where(c => c.ParentId == null || c.ParentId == 0)
                .OrderBy(c => c.SortOrder)
                .ToList();

            // Add "None" option
            ParentCategories.Add(new Category { Id = 0, Name = "<No Parent>" });

            foreach (var root in roots)
            {
                root.Indentation = "";
                root.Level = 0;
                Categories.Add(root);
                ParentCategories.Add(root);
                AddChildren(root, list, 1);
            }
        }

        private void UpdateParentCategoriesForEdit(Category current)
        {
            ParentCategories.Clear();
            ParentCategories.Add(new Category { Id = 0, Name = "<No Parent>" });

            // Get all categories again to ensure fresh list
            // In a real app we might want to reload from DB, but here we use cached List if possible or reload.
            // Since we are transient, LoadCategories might have been called.
            // But we need a full list to filter.
            // Let's reload to be safe.
            Task.Run(async () =>
            {
                var list = await _sqlite.GetCategoriesAsync();
                var roots = list.Where(c => c.ParentId == null || c.ParentId == 0).ToList();

                // Exclude current and its children
                // Helper to check if 'c' is 'current' or a descendant of 'current'
                bool IsDescendant(Category potentialDescendant)
                {
                    if (potentialDescendant.Id == current.Id) return true;
                    // Recursive check not easy with flat list without building tree
                    // Simple check: don't allow current.
                    // Fully robust check requires tree traversal.
                    // For now, let's just exclude 'current'.
                    // To properly exclude children, we need to know children.
                    return potentialDescendant.Id == current.Id;
                }

                // Better approach: build tree, then flatten, skipping the branch of 'current'.

                foreach (var root in roots)
                {
                    if (root.Id == current.Id) continue; // Skip current

                    root.Indentation = "";
                    root.Level = 0;
                    ParentCategories.Add(root);
                    AddChildrenForEdit(root, list, 1, current.Id);
                }

                // Set selected parent
                if (current.ParentId != null)
                {
                    SelectedParent = ParentCategories.FirstOrDefault(c => c.Id == current.ParentId);
                }
                else
                {
                    SelectedParent = ParentCategories.First(); // <No Parent>
                }
            });
        }

        private void AddChildrenForEdit(Category parent, List<Category> all, int level, int excludeId)
        {
            var children = all
                .Where(c => c.ParentId == parent.Id)
                .ToList();

            foreach (var child in children)
            {
                if (child.Id == excludeId) continue; // Should not happen if parent is not excluded, unless circular data exists

                child.Indentation = new string(' ', level * 4);
                child.Level = level;
                ParentCategories.Add(child);
                AddChildrenForEdit(child, all, level + 1, excludeId);
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
                child.Indentation = new string(' ', level * 4); // Keep for Picker
                child.Level = level;
                Categories.Add(child);
                ParentCategories.Add(child);
                AddChildren(child, all, level + 1);
            }
        }

        private async Task AddCategoryAsync()
        {
            if (!string.IsNullOrEmpty(NewCategory))
            {
                var cat = new Category
                {
                    Name = NewCategory,
                    ParentId = (SelectedParent != null && SelectedParent.Id != 0)
                    ? SelectedParent.Id
                    : null
                };
                await _sqlite.SaveCategoryAsync(cat);
                SelectedParent = null;

                // Navigate back
                await Shell.Current.GoToAsync("..");
            }
        }

        private async Task DeleteCategoryAsync(Category cat)
        {
            if (cat == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Delete Category",
                $"Are you sure you want to delete '{cat.Name}'? Any sub-categories will be removed too!",
                "Yes", "No");

            if (!confirm) return;

            // Find children and promote them to root
            var children = Categories.Where(c => c.ParentId == cat.Id).ToList();
            foreach (var child in children)
            {
                await _sqlite.DeleteCategoryAsync(child);
            }

            // Delete the category
            await _sqlite.DeleteCategoryAsync(cat);

            // Reload to reflect changes
            LoadCategories();
        }

        private void RecursiveMove(Category source, int targetIndex)
        {
            var categoriesToMove = Categories
                .Where(x => x.ParentId == source.Id)
                .OrderByDescending(x => x.SortOrder)
                .ToList();

            foreach (var category in categoriesToMove)
            {
                RecursiveMove(category, targetIndex);
            }

            var sourceIndex = Categories.IndexOf(source);
            Categories.Move(sourceIndex, targetIndex);
        }

        private async Task ReorderCategoriesAsync(Category source, Category target)
        {
            for (int i = 0; i < Categories.Count; i++)
            {
                Categories[i].SortOrder = i;
                await _sqlite.UpdateCategoryAsync(Categories[i]);
            }

            // Reload everything to show correct hierarchy/indentation
            LoadCategories();
        }
    }
}
