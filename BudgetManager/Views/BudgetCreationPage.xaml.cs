using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class BudgetCreationPage : ContentPage
{
    public BudgetCreationPage(BudgetCreationViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
