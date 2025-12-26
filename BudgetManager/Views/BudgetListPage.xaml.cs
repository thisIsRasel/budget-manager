using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class BudgetListPage : ContentPage
{
    public BudgetListPage(BudgetViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
        if (BindingContext is BudgetViewModel vm)
        {
            vm.LoadBudgets();
        }
    }
}