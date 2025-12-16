using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class BudgetPage : ContentPage
{
    public BudgetPage(BudgetViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BudgetViewModel vm)
        {
            vm.LoadBudgets();
        }
    }
}