using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class CategoryListPage : ContentPage
{
    public CategoryListPage(CategoryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        //if (BindingContext is CategoryViewModel vm)
        //{
        //    vm.LoadCategories();
        //}
    }
}