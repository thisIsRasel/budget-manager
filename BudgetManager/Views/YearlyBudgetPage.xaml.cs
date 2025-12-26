using BudgetManager.ViewModels;

namespace BudgetManager.Views;

public partial class YearlyBudgetPage : ContentPage
{
	public YearlyBudgetPage(YearlyBudgetViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetNavBarIsVisible(this, false);
    }
}
