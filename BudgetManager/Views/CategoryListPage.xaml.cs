using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class CategoryPage : ContentPage
{
    public CategoryPage(CategoryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CategoryViewModel vm)
        {
            vm.LoadCategories();
        }
    }
}