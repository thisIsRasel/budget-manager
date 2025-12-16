using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class AddBudgetPage : ContentPage
{
    public AddBudgetPage(AddBudgetViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
